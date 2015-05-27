using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarkdownWeb.PostFilters
{
    /// <summary>
    ///     Adds IDs to headings so that we can create links to them
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Will remove everything but letters and digits from the title and use that as the ID. i.e.
    ///         <code>Welcome to this page</code> becomes <c>Welcometothispage</c>.
    ///     </para>
    /// </remarks>
    //credit: http://stackoverflow.com/questions/22693604/c-sharp-regex-to-parse-html-string-and-add-ids-into-each-header-tag
    public class AnchorHeadings : IPostFilter
    {
        public string Parse(PostFilterContext context)
        {
            return Regex.Replace(context.HtmlToParse, @"<h(?<number>[1-6])>(?<innerText>[^<]*)</h[1-6]>", FormatHeadings);
        }

        private static string FormatHeadings(Match x)
        {
            return string.Format("<h{2} id=\"{0}\">{1}</h{2}>",
                new String(x.Groups["innerText"].Value.Where(char.IsLetterOrDigit).ToArray()),
                x.Groups["innerText"],
                x.Groups["number"]);
        }
    }
}