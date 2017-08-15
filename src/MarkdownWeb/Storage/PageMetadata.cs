using System;

namespace MarkdownWeb.Storage
{
    /// <summary>
    /// Information about a page
    /// </summary>
    public class PageMetadata
    {
        /// <summary>
        /// Author of the current revision.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// When this revision was created
        /// </summary>
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        /// Revision identifier.
        /// </summary>
        public int Revision { get; set; }

        /// <summary>
        /// Why this revision was created.
        /// </summary>
        public string ChangeComment { get; set; }
    }
}