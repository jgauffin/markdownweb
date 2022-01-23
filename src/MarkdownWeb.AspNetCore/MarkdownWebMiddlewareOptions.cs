using System;
using MarkdownWeb.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace MarkdownWeb.AspNetCore
{
    /// <summary>
    /// Options for <see cref="MarkdownWebMiddleware"/>.
    /// </summary>
    public class MarkdownWebMiddlewareOptions
    {
        private PathString _path = "/documentation/";

        /// <summary>
        ///     Physical location of files, relative to the web site root.
        /// </summary>
        public string DocumentationDirectory { get; set; }

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
        ///     HTML page to use as layout for the wiki pages.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Must contain a <c>{Wiki}</c> tag, optional tags are "{Title}", "{Abstract}" and "{TableOfContents}".
        ///     </para>
        ///     <para>
        ///         Relative path from <see cref="IHostEnvironment.ContentRootPath" /> (which typically is the wwwroot folder).
        ///     </para>
        /// </remarks>
        public string LayoutPage { get; set; } = "/Shared/MarkdownWeb.html";

        /// <summary>
        ///     Path (url path) to serve the wiki from.
        /// </summary>
        /// <example>
        ///     "/documentation/"
        /// </example>
        public PathString Path
        {
            get => _path;
            set
            {
                _path = value;
            }
        }

        /// <summary>
        /// Repository used to load the wiki files.
        /// </summary>
        public IPageRepository Repository { get; set; }
    }
}