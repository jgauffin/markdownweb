namespace MarkdownWeb
{
    /// <summary>
    ///     Something that we can access wiki pages from, like a database or a directory.
    /// </summary>
    public interface IPageSource
    {
        /// <summary>
        ///     Checks if the specified page exists
        /// </summary>
        /// <param name="page">Given page</param>
        /// <returns><c>true</c> if page exists; otherwise <c>false</c>.</returns>
        bool PageExists(PageReference page);
    }
}