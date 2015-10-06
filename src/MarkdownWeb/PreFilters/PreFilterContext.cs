namespace MarkdownWeb.PreFilters
{
    public class PreFilterContext
    {
        private readonly IMarkdownParser _pageParser;

        public PreFilterContext(IMarkdownParser pageParser)
        {
            _pageParser = pageParser;
        }

        public string TextToParse { get; set; }

        /// <summary>
        ///     Path to the current markdown document
        /// </summary>
        public string CurrentPagePath { get; set; }

        public IMarkdownParser Parser { get; set; }

        public string Parse(string text)
        {
            return _pageParser.Parse(CurrentPagePath, text);
        }
    }
}