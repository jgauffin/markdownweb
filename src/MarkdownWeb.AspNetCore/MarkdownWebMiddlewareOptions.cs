using System;
using MarkdownWeb.Git;
using MarkdownWeb.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace MarkdownWeb.AspNetCore
{
    /// <summary>
    ///     Options for <see cref="MarkdownWebMiddleware" />.
    /// </summary>
    public class MarkdownWebMiddlewareOptions
    {
        /// <summary>
        ///     Physical location of files, relative to the web site root.
        /// </summary>
        public string DocumentationDirectory { get; set; }

        /// <summary>
        ///     Callback to get error logs if something fails.
        /// </summary>
        public Action<string, Exception> ErrorLog { get; set; }

        /// <summary>
        ///     When you want to use a GIT repository as documentation source (will automatically be pulled and updated
        ///     periodically).
        /// </summary>
        public string GitRepositoryUrl { get; set; }

        /// <summary>
        ///     Sub directory in the repository where the documentation is located.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <see cref="DocumentationDirectory" /> points on where the git repository is checked out, while this can point
        ///         on a sub dir (like "docs\").
        ///     </para>
        /// </remarks>
        public string GitSubFolder { get; set; }

        /// <summary>
        ///     Page to use as layout for the wiki pages.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         It looks for "Views\Shared\Wiki.cshtml" per default, but can be any view supported page or a plain HTML page.
        ///     </para>
        ///     <para>
        ///         For plain HTML pages, it must contain a <c>{Body}</c> tag, optional tags are "{Title}", "{Abstract}" and
        ///         "{TableOfContents}".
        ///     </para>
        ///     <para>
        ///         For HTML, the path is relative to <see cref="IHostEnvironment.ContentRootPath" /> (which typically is the
        ///         wwwroot folder), for view pages,
        ///         the file must be placed where the view engine normally looks for files.
        ///     </para>
        /// </remarks>
        public string LayoutPage { get; set; } = "/Views/Shared/Wiki.cshtml";

        /// <summary>
        ///     Repository used to load the wiki files.
        /// </summary>
        public IPageRepository Repository { get; set; }

        /// <summary>
        ///     Path (url path) to serve the wiki from.
        /// </summary>
        /// <example>
        ///     "/documentation/"
        /// </example>
        public PathString WebPath
        {
            get => _webPath;
            set => _webPath = value.Value?.EndsWith("/") == true ? new PathString(value.Value + "/") : value;
        }

        /// <summary>
        /// Used to customize the git configuration.
        /// </summary>
        public void GitOptions(Action<GitSettings> settings)
        {
            GitSettings = settings;
        }

        internal Action<GitSettings> GitSettings;

        /// <summary>
        /// Use to customize how pages are generated.
        /// </summary>
        public void PageServiceOptions(Action<PageServiceConfiguration> settings)
        {
            PageServiceSettings = settings;
        }

        internal Action<PageServiceConfiguration> PageServiceSettings;
        private PathString _webPath = "/documentation/";
    }
}