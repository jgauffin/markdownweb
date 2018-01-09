namespace MarkdownWeb
{
    /// <summary>
    ///     Something that we can access wiki pages from, like a database or a directory.
    /// </summary>
    public interface IPageSource
    {
        /// <summary>
        ///     Get absolute (web path) from a wiki page URL.
        /// </summary>
        /// <param name="currentPageUrl"></param>
        /// <param name="linkedUrl"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         All wiki pages are relative to the wiki root, although the wiki itself can exist under a sub directory in the
        ///         website.
        ///         This method transforms a wiki URL <c>"/tutorial/index.md"</c> to a web site absolute URL
        ///         <c>"/help/wiki/tutorial/index"</c>.
        ///     </para>
        /// </remarks>
        string GetAbsoluteUrl(string currentPageUrl, string linkedUrl);

        /// <summary>
        ///     Get wiki page for the given URL
        /// </summary>
        /// <param name="url">Page that links to the given page</param>
        /// <returns><c>true</c> if page exists; otherwise <c>false</c>.</returns>
        string GetContent(string url);

        /// <summary>
        ///     Checks if the specified page exists
        /// </summary>
        /// <param name="currentPageUrl">Page that links to the given page</param>
        /// <param name="linkedUrl">Page to check, can be a page relative URL</param>
        /// <returns><c>true</c> if page exists; otherwise <c>false</c>.</returns>
        bool PageExists(string currentPageUrl, string linkedUrl);

        /// <summary>
        ///     Checks if the specified page exists
        /// </summary>
        /// <param name="url">Page that links to the given page</param>
        /// <returns><c>true</c> if page exists; otherwise <c>false</c>.</returns>
        bool PageExists(string url);
    }
}