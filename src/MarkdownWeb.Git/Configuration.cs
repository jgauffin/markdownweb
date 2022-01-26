using System;
using LibGit2Sharp;

namespace MarkdownWeb.Git
{
    /// <summary>
    ///     Configuration for <see cref="GitPageRepository" />
    /// </summary>
    public class GitSettings
    {
        /// <summary>
        ///     Creates a new instance of <see cref="GitSettings" />
        /// </summary>
        public GitSettings()
        {
            DocumentationDirectory = @"doc\";
        }

        /// <summary>
        ///     Credentials for the repository.
        /// </summary>
        public Credentials Credentials { get; set; }


        /// <summary>
        ///     Start from scratch if we can't get the repository and reset hard did not work.
        /// </summary>
        /// <value>
        ///     Default is <c>true</c>
        /// </value>
        public bool DeleteAndFetchOnErrors { get; set; }

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
        ///     Invoked once a pull/merge have been completed.
        /// </summary>
        public Action DownloadCompleted { get; set; }

        /// <summary>
        ///     Directory used for git fetch
        /// </summary>
        public string FetchDirectory { get; set; }

        /// <summary>
        ///     HTTPS or SSH url for your git repository
        /// </summary>
        public Uri RepositoryUri { get; set; }

        /// <summary>
        ///     Do a <c>reset --hard</c> if there are conflicts on pull. Default true;
        /// </summary>
        public bool ResetOnConflicts { get; set; } = true;

        /// <summary>
        ///     Do all GIT operations in a background task (to not slow down the website).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Default is <c>true</c>.
        ///     </para>
        /// </remarks>
        public bool UpdateInBackground { get; set; } = true;

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