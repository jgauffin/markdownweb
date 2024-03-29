﻿using System;
using System.Collections.Generic;

namespace MarkdownWeb.Tree
{
    /// <inheritdoc />
    public class PagePathComparer : IComparer<PageSummary>
    {
        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the
        ///     other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        ///     A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in
        ///     the following table.Value Meaning Less than zero
        ///     <paramref name="x" /> is less than <paramref name="y" />.Zero
        ///     <paramref name="x" /> equals <paramref name="y" />.Greater than zero
        ///     <paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        public int Compare(PageSummary x, PageSummary y)
        {
            if (x?.PageReference.Equals(y?.PageReference) == true)
            {
                return 0;
            }

            var partsx = x.PageReference.WikiUrl.Split(new []{'/'}, StringSplitOptions.RemoveEmptyEntries);
            var partsy = y.PageReference.WikiUrl.Split(new []{'/'}, StringSplitOptions.RemoveEmptyEntries);
            var min = Math.Min(partsx.Length, partsy.Length);
            for (int i = 0; i < min; i++)
            {
                var result = partsx[i].CompareTo(partsy[i]);
                if (result != 0)
                    return result;
            }

            if (partsx.Length < partsy.Length)
                return -1;
            if (partsx.Length > partsy.Length)
                return 1;

            if (x.PageReference.IsIndex)
                return -1;
            if (y.PageReference.IsIndex)
                return 1;

            return x.PageReference.WikiUrl.CompareTo(y.PageReference.WikiUrl);
        }
    }
}
