namespace MarkdownWeb.PreFilters
{
    public class PreFilterContext
    {
        private readonly PageParser _pageParser;

        public PreFilterContext(PageParser pageParser)
        {
            _pageParser = pageParser;
        }

        public string TextToParse { get; set; }

        /// <summary>
        ///     Path to the current markdown document
        /// </summary>
        public string CurrentUrlPath { get; set; }

        /// <summary>
        ///     Path to the root document for the markdown web
        /// </summary>
        public string RootUrlPath { get; set; }

        public IMarkdownParser Parser { get; set; }

        public string Parse(string text)
        {
            return _pageParser.ParseString(CurrentUrlPath, text).Body;
        }
    }
}