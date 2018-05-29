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
        /// Remove the root path that specifies where the wiki/markdown pages reside.
        /// </summary>
        /// <param name="url">web url</param>
        /// <returns>wiki path</returns>
        string RemoveWebRoot(string url);

        /// <summary>
        ///     Convert an wiki path to an absolute web site path.
        /// </summary>
        /// <param name="wikiPath">Path in the wiki</param>
        /// <returns>Path in the web site</returns>
        string ToWebUrl(string wikiPath);

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
        PageReference MapUrlToWikiPath(string websiteAbsolutePath);
    }
}