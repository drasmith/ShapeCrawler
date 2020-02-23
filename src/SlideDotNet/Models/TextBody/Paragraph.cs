﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SlideDotNet.Extensions;
using SlideDotNet.Models.Settings;
using SlideDotNet.Statics;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
// ReSharper disable PossibleMultipleEnumeration

namespace SlideDotNet.Models.TextBody
{
    /// <summary>
    /// Represents a text paragraph.
    /// </summary>
    public class Paragraph
    {
        #region Fields

        private readonly A.Paragraph _aParagraph;
        private readonly IShapeContext _spContext;
        private string _text;
        private readonly Lazy<int> _lvl;
        private readonly Lazy<List<Portion>> _portions;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Returns the paragraph's text string.
        /// </summary>
        public string Text {
            get
            {
                if (_text == null)
                {
                    InitText();
                }

                return _text;
            }
        }

        /// <summary>
        /// Returns paragraph text portions.
        /// </summary>
        public IList<Portion> Portions => _portions.Value;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes an instance of the <see cref="Paragraph"/> class.
        /// </summary>
        /// <param name="elSetting"></param>
        /// <param name="aParagraph">A XML paragraph which contains a text.</param>
        public Paragraph(IShapeContext elSetting, A.Paragraph aParagraph)
        {
            _aParagraph = aParagraph ?? throw new ArgumentNullException(nameof(aParagraph));
            _spContext = elSetting ?? throw new ArgumentNullException(nameof(elSetting));
            _lvl = new Lazy<int>(GetInnerLevel(_aParagraph));
            _portions = new Lazy<List<Portion>>(GetPortions);
        }

        #endregion Constructors

        #region Private Methods

        private static int GetInnerLevel(A.Paragraph aPr)
        {
            var outerLvl = aPr.ParagraphProperties?.Level ?? 0;
            var interLvl = outerLvl + 1;

            return interLvl;
        }

        private void InitText()
        {
            _text = Portions.Select(p => p.Text).Aggregate((t1, t2) => t1 + t2);
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
        private List<Portion> GetPortions()
        {
            var runs = _aParagraph.Elements<A.Run>();
            var portions = new List<Portion>(runs.Count());
            foreach (var run in runs)
            {
                var runFh = GetRunFontHeight(run);
                portions.Add(new Portion(runFh, run.Text.Text));
            }

            return portions;
        }

        private int GetRunFontHeight(A.Run run)
        {
            // first, tries parse font height from current run (portion)
            var runFs = run.RunProperties?.FontSize;
            if (runFs != null)
            {
                return runFs.Value;
            }

            // if element is placeholder, tries to get from placeholder data
            var xmlElement = _spContext.XmlElement;
            if (xmlElement.IsPlaceholder())
            {
                var prFontHeight = _spContext.PlaceholderFontService.TryGetHeight(xmlElement, _lvl.Value);
                if (prFontHeight != null)
                {
                    return (int)prFontHeight;
                }
            }

            if (_spContext.PreSettings.LlvFontHeights.ContainsKey(_lvl.Value))
            {
                return _spContext.PreSettings.LlvFontHeights[_lvl.Value];
            }

            var exist = _spContext.TryFromMasterOther(_lvl.Value, out int fh);
            if (exist)
            {
                return fh;
            }

            return FormatConstants.DefaultFontHeight;
        }

        #endregion Private Methods
    }
}
