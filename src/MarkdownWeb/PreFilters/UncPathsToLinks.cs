using System.Text.RegularExpressions;

namespace MarkdownWeb.PreFilters
{
    /// <summary>
    /// Converts UNC paths to clickable file links
    /// </summary>
    public class UncPathsToLinks : IPreFilter
    {
        /// <summary>
        /// Parse text
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns>Text with the modifications done by this script</returns>
        public string Parse(PreFilterContext filterContext)
        {
            var regex = @"(\\\\[A-Z_a-z0-9$\\.\-]+)[\s|\n]";
            var r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.Replace(filterContext.TextToParse, "<a href=\"file://$1\" target=\"_blank\">\\$1</a>");
        }
    }
}
