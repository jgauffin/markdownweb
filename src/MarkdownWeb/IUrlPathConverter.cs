namespace MarkdownWeb
{
    /// <summary>
    ///     The purpose of this class is to be able to convert wiki url paths to website url paths.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Wiki paths are relative to the wiki root, while the wiki might be in a sub path on the web site. So when a path
    ///         in the wiki is <c>/patterns/srp</c> the complete
    ///         website path might be <c>/doc/wiki/patterns/srp</c>.
    ///     </para>
    /// </remarks>
    public interface IUrlPathConverter
    {
        /// <summary>
        ///     Convert an wiki path to an absolute web site path.
        /// </summary>
        /// <param name="wikiPath">Path in the wiki</param>
        /// <returns>Path in the web site</returns>
        string ToWebPath(string wikiPath);

        /// <summary>
        ///     The path is a virtual path (i.e. starts with "~").
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <returns>Path in the web site</returns>
        string ToAbsolutePath(string virtualPath);


        /// <summary>
        ///     Convert a web path to a wiki path
        /// </summary>
        /// <param name="websiteAbsolutePath">Path in the web site</param>
        /// <returns>Path for the wiki document.</returns>
        string MapUrlToWikiPath(string websiteAbsolutePath);

        /// <summary>
        ///     Convert a relative path path to a absolute page path.
        /// </summary>
        /// <param name="currentWikiPath">"Wiki path for the page being parsed</param>
        /// <param name="linkedPath">Link to another document which may or may not be relative to the current page.</param>
        /// <returns>Path for the wiki document.</returns>
        string ToWikiPath(string currentWikiPath, string linkedPath);

        /// <summary>
        /// A page contains a link to another page. 
        /// </summary>
        /// <param name="currentWikiPath">Wiki path to the page that is being parsed</param>
        /// <param name="linkedPath">Path for a wiki link in the current page. Absolute or relative wiki path. i.e. it can contain <c>".."</c></param>
        /// <returns>Absolute path in the web site.</returns>
        string ToWebPath(string currentWikiPath, string linkedPath);

        /// <summary>
        /// Map a relative page path
        /// </summary>
        /// <param name="currentWikiPath">Page that got the link</param>
        /// <param name="linkedWikiPath">The link</param>
        /// <returns>A complete web site URL.</returns>
        string MapWikiPaths(string currentWikiPath, string linkedWikiPath);
    }
}