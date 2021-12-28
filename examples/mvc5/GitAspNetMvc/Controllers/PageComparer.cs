using System;
using System.Collections.Generic;
using System.IO;
using MarkdownWeb;

namespace GitAspNetMvc.Controllers
{
    public class PageComparer : IComparer<PageSummary>
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
            var partsx = x.PageReference.WikiUrl.Split(new []{'/'}, StringSplitOptions.RemoveEmptyEntries);
            var partsy = y.PageReference.WikiUrl.Split(new []{'/'}, StringSplitOptions.RemoveEmptyEntries);
            var min = Math.Min(partsx.Length, partsy.Length);
            for (int i = 0; i < min; i++)
            {
                var result = partsx[i].CompareTo(partsy[i]);
                if (result == 0)
                    continue;

                return result;
            }

            if (partsx.Length < partsy.Length)
                return -1;
            if (partsx.Length > partsy.Length)
                return 1;
                

            for (var i = 0; i < partsx.Length; i++)
            {
                var result = partsx[i].CompareTo(partsy[i]);
                if (result != 0)
                    return result;
            }

            var filename1 = Path.GetFileName(x.PageReference.WikiUrl);
            var filename2 = Path.GetFileName(y.PageReference.WikiUrl);
            if (filename1 == "index.md")
                return -1;
            if (filename2 == "index.md")
                return 1;

            return filename1.CompareTo(filename2);
        }
    }
}