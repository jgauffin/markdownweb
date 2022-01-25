using System;
using LibGit2Sharp;

namespace MarkdownWeb.Git
{
    /// <summary>
    ///     Configuration for <see cref="GitPageRepository" />
    /// </summary>
    public class GitStorageConfiguration
    {
        /// <summary>
        ///     Creates a new instance of <see cref="GitStorageConfiguration" />
        /// </summary>
        public GitStorageConfiguration()
        {
            DocumentationDirectory = @"doc\";
        }

        /// <summary>
        ///     Credentials for the repository.
        /// </summary>
        public Credentials Credentials { get; set; }

        /// <summary>
        ///     Directory within <see cref="FetchDirectory" />, typically ends with "src\" or "doc\"
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If the <see cref="FetchDirectory" /> is <c>c:\src\yourProject\</c> then the documentationDirectory should be
        ///         <c>c:\src\yourProject\docs\</c> or similar.
        ///     </para>
        /// </remarks>
        public string DocumentationDirectory { get; set; }

        /// <summary>
        ///     Directory used for git fetch
        /// </summary>
        public string FetchDirectory { get; set; }

        /// <summary>
        ///     HTTPS or SSH url for your git repository
        /// </summary>
        public Uri RepositoryUri { get; set; }

        /// <summary>
        ///     How often we should check for updates (when someone requests the documentation)
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Per default 30 seconds.
        ///     </para>
        /// </remarks>
        public TimeSpan UpdateInterval { get; set; }
    }
}