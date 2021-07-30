﻿using System.Collections.Generic;
using DocumentFormat.OpenXml.Drawing.Charts;
using ShapeCrawler.Shared;
using X = DocumentFormat.OpenXml.Spreadsheet;

namespace ShapeCrawler.Charts
{
    /// <summary>
    ///     Represents a chart category.
    /// </summary>
    public class Category // TODO: should be internal?
    {
        private readonly int index;
        private readonly NumericValue cachedName;
        private readonly ResettableLazy<List<X.Cell>> _xCells;

        #region Constructors

        internal Category(
            ResettableLazy<List<X.Cell>> xCells,
            int index,
            NumericValue cachedName,
            Category mainCategory)
            : this(xCells, index, cachedName)
        {
            // TODO: what about creating a new separate class like MultiCategory:Category
            this.MainCategory = mainCategory;
        }

        internal Category(
            ResettableLazy<List<X.Cell>> xCells,
            int index,
            NumericValue cachedName)
        {
            this._xCells = xCells;
            this.index = index;
            this.cachedName = cachedName;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///     Gets main category. Returns <c>NULL</c> if the chart is not a multi-level.
        /// </summary>
        public Category MainCategory { get; }

        /// <summary>
        ///     Gets or sets category name.
        /// </summary>
        public string Name
        {
            get => this.cachedName.InnerText;
            set
            {
                this._xCells.Value[this.index].CellValue.Text = value;
                this.cachedName.Text = value;
                this._xCells.Reset();
            }
        }

        #endregion Properties
    }
}