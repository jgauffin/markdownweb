using System;
using System.Collections.Generic;
using System.Text;

namespace MarkdownWeb.PreFilters
{
    /// <summary>
    ///     Generates sequence diagrams using https://bramp.github.io/js-sequence-diagrams/
    /// </summary>
    public class SequenceDiagramsFilter : IPreFilter
    {
        private const string StartBlock = "\r\n```sequence";
        private const string EndBlock = "\r\n```";

        public SequenceDiagramsFilter()
        {
            ThemeName = "Simple";
            Ids = new List<string>();
        }

        /// <summary>
        /// Theme that js-sequence-diagrams should use for the generation
        /// </summary>
        public string ThemeName { get; set; }

        /// <summary>
        /// For tests
        /// </summary>
        internal List<string> Ids { get; set; }


        /// <summary>
        /// Parse text
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns>Text with the modifications done by this script</returns>
        public string Parse(PreFilterContext filterContext)
        {
            var endPos = 0;
            var generated = new StringBuilder();
            var startPos = filterContext.TextToParse.IndexOf(StartBlock, StringComparison.OrdinalIgnoreCase);

            // check if doc starts with a sequence
            if (startPos == -1)
            {
                if (!filterContext.TextToParse.StartsWith(StartBlock.TrimStart()))
                    return filterContext.TextToParse;

                // less complex than changing the block texts depending on if we
                // are the start of the document or not.
                filterContext.TextToParse = "\r\n" + filterContext.TextToParse;
                startPos = 0;
            }

            while (startPos != -1)
            {
                //append the document part that was from last position to this sequence
                //+2 = crlf
                if (startPos != 0)
                {
                    // reverse here since we want to get document from old endPos up to our startPos
                    var middleHtml = filterContext.TextToParse.Substring(endPos, startPos - endPos + 2);
                    generated.Append(middleHtml);
                }

                //find where this sequence ends
                endPos = filterContext.TextToParse.IndexOf(EndBlock, startPos + StartBlock.Length, StringComparison.Ordinal);

                //failed to find end, assume whole document since then the block doesn't end with crlf
                var diagram = endPos == -1
                    ? filterContext.TextToParse.Substring(startPos + StartBlock.Length)
                    : filterContext.TextToParse.Substring(startPos + StartBlock.Length, endPos - startPos - StartBlock.Length);
                diagram = diagram.Trim();

                var id = $"seq{Guid.NewGuid():N}";
                Ids.Add(id);
                generated.AppendLine($"<div class=\"sequencediagram\" id=\"{id}\"></div>");
                generated.Append($@"<script> 
    var d = Diagram.parse('{diagram.Replace("\"", "\\\"")}');
    var options = {{ theme: '{ThemeName}'}};
    d.drawSVG('{id}', options);
</script>");

                if (endPos == -1)
                    break;
                endPos += EndBlock.Length;

                startPos = filterContext.TextToParse.IndexOf("\r\n```sequence", endPos, StringComparison.OrdinalIgnoreCase);
            }

            if (endPos != -1 && endPos != filterContext.TextToParse.Length)
            {
                var leftOver = filterContext.TextToParse.Substring(endPos);
                generated.Append(leftOver);
            }


            return generated.ToString();
        }
    }
}