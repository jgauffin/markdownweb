namespace MarkdownWeb.PreFilters
{
    /// <summary>
    /// Used to parse the text before the markdown parser have done it's job.
    /// </summary>
    public interface IPreFilter
    {
        /// <summary>
        /// Parse text
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns>Text with the modifications done by this script</returns>
        string Parse(PreFilterContext filterContext);
    }
}
