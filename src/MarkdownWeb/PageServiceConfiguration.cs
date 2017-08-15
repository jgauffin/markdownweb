namespace MarkdownWeb
{
    /// <summary>
    ///     Configuration for <see cref="PageService" />.
    /// </summary>
    public class PageServiceConfiguration
    {
        public PageServiceConfiguration()
        {
            MissingLinkStyle = "color:red";
            DefaultCodeLanguage = "csharp";
        }

        /// <summary>
        ///     Style to set on missing links
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Default is <c>color: red;</c>
        ///     </para>
        /// </remarks>
        public string MissingLinkStyle { get; set; }

        /// <summary>
        ///     Language tag to use per default for code blocks (when no lang has been specified)
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Default is <c>text</c>
        ///     </para>
        /// </remarks>
        public string DefaultCodeLanguage { get; set; }
    }
}