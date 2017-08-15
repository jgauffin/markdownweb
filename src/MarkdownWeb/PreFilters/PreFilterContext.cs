using System;

namespace MarkdownWeb.PreFilters
{
    /// <summary>
    ///     Information provided to all pre processors.
    /// </summary>
    public class PreFilterContext
    {
        private readonly IMarkdownParser _pageParser;


        /// <summary>
        ///     Create a new instance of <see cref="PreFilterContext" />.
        /// </summary>
        /// <param name="pageParser">Parser used to treat parse requests</param>
        public PreFilterContext(IMarkdownParser pageParser)
        {
            if (pageParser == null) throw new ArgumentNullException("pageParser");
            _pageParser = pageParser;
        }

        /// <summary>
        ///     Text that pre processors should process.
        /// </summary>
        public string TextToParse { get; set; }

        /// <summary>
        ///     Path to the current markdown document
        /// </summary>
        public string CurrentPagePath { get; set; }

        /// <summary>
        ///     Parse a markdown text
        /// </summary>
        /// <param name="text">markdown</param>
        /// <returns>html</returns>
        public string Parse(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            return _pageParser.Parse(CurrentPagePath, text);
        }
    }
}