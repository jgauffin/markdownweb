using System;
using MarkdownWeb.PostFilters;
using MarkdownWeb.PreFilters;

namespace MarkdownWeb
{
    /// <summary>
    ///     Configuration for <see cref="PageService" />.
    /// </summary>
    public class PageServiceConfiguration
    {
        private Func<string, bool> _directoryFilter = s => true;

        public PageServiceConfiguration()
        {
            MissingLinkStyle = "color:red";
            DefaultCodeLanguage = "csharp";
            PreFilters = new PreFilterCollection();
            PostFilters = new PostFilterCollection();
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

        /// <summary>
        ///     Used to process the generated HTML once it's been created by the markdown parser.
        /// </summary>
        public PostFilterCollection PostFilters { get; private set; }

        /// <summary>
        ///     Used to modify the text before it's given to the markdown parser.
        /// </summary>
        public PreFilterCollection PreFilters { get; private set; }

        /// <summary>
        /// Can be used to filter out directories from the index generation.
        /// </summary>
        public Func<string, bool> DirectoryFilter
        {
            get => _directoryFilter;
            set => _directoryFilter = value ?? (dir => true);
        }
    }
}