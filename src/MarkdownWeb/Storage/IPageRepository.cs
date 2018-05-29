using System.Collections.Generic;

namespace MarkdownWeb.Storage
{
    /// <summary>
    ///     Used to fetch information from the storage.
    /// </summary>
    public interface IPageRepository
    {
        /// <summary>
        ///     Get page.
        /// </summary>
        /// <param name="wikiPagePath">Relative to the wiki root.</param>
        /// <returns>Page if found; otherwise <c>null</c>.</returns>
        StoredPage Get(string wikiPagePath);

        /// <summary>
        /// </summary>
        /// <param name="wikiPagePath">Relative to the wiki root</param>
        /// <param name="revision"></param>
        /// <returns>Page if found; otherwise <c>null</c>.</returns>
        StoredPage Get(string wikiPagePath, int revision);

        /// <summary>
        ///     Get all saved revisions of this document
        /// </summary>
        /// <param name="wikiPagePath">Relative to the wiki root</param>
        /// <returns>Metadata if page was found; otherwise <c>null</c>.</returns>
        PageMetadata[] GetRevisions(string wikiPagePath);

        /// <summary>
        /// List all pages (as links from root)
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllPagesAsLinks();

        /// <summary>
        ///     Check if page exists
        /// </summary>
        /// <param name="wikiPagePath">Relative to the wiki root</param>
        /// <returns><c>true</c> if found; otherwise false</returns>
        bool Exists(string wikiPagePath);

        /// <summary>
        ///     Have created a new page.
        /// </summary>
        /// <param name="wikiPagePath">Relative to the wiki root</param>
        /// <param name="page">Page that was created.</param>
        void Create(string wikiPagePath, EditedPage page);

        /// <summary>
        ///     Have updated an existing page.
        /// </summary>
        /// <param name="wikiPagePath">Relative to the wiki root</param>
        /// <param name="page">Page that was edited.</param>
        void Update(string wikiPagePath, EditedPage page);
    }
}