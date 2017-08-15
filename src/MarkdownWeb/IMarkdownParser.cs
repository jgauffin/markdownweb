namespace MarkdownWeb
{
    /// <summary>
    /// Definition of a markdown parser.
    /// </summary>
    public interface IMarkdownParser
    {
        /// <summary>
        /// Parse a parkdown document
        /// </summary>
        /// <param name="currentPagePath">Path to the page which is passed as <c>text</c>.</param>
        /// <param name="text">Markdown document</param>
        /// <returns>HTML</returns>
        string Parse(string currentPagePath, string text);
    }
}