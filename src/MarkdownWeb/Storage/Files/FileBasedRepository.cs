using System;
using System.IO;
using System.Text;

namespace MarkdownWeb.Storage.Files
{
    /// <summary>
    ///     Uses disk to locate pages.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This implementation do currently not support revisions. i.e. all previous edits are lost (overwritten) each
    ///         time a page is saved.
    ///     </para>
    /// </remarks>
    public class FileBasedRepository : IPageRepository
    {
        private readonly string _rootFilePath;

        /// <summary>
        ///     Create a new instance of <see cref="FileBasedRepository" />.
        /// </summary>
        /// <param name="rootFilePath">Root folder where all markdown files are located.</param>
        public FileBasedRepository(string rootFilePath)
        {
            if (rootFilePath == null) throw new ArgumentNullException("rootFilePath");
            _rootFilePath = rootFilePath;
            Encoding = Encoding.UTF8;
        }

        public Encoding Encoding { get; set; }

        public StoredPage Get(string wikiPagePath)
        {
            if (wikiPagePath == null) throw new ArgumentNullException("wikiPagePath");
            var fileName = GetFilePath(wikiPagePath);
            if (fileName == null)
                return null;

            var body = File.ReadAllText(fileName, Encoding);
            var pos = body.IndexOfAny(new[] {'\r', '\n'});
            var title = pos != -1 ? body.Substring(0, pos).TrimStart('#', ' ', '\t') : "";


            return new StoredPage
            {
                Body = body,
                Title = title,
                Author = "Unknown",
                CreatedAtUtc = File.GetLastWriteTimeUtc(fileName),
                Revision = 1
            };
        }

        public StoredPage Get(string wikiPagePath, int revision)
        {
            if (wikiPagePath == null) throw new ArgumentNullException("wikiPagePath");
            return Get(wikiPagePath);
        }

        public PageMetadata[] GetRevisions(string wikiPagePath)
        {
            if (wikiPagePath == null) throw new ArgumentNullException("wikiPagePath");
            var page = Get(wikiPagePath);
            if (page == null)
                return null;

            return new[]
            {
                new PageMetadata
                {
                    CreatedAtUtc = page.CreatedAtUtc,
                    Revision = 1,
                    Author = page.Author,
                    ChangeComment = "No comment"
                }
            };
        }

        public bool Exists(string wikiPath)
        {
            if (wikiPath == null) throw new ArgumentNullException("wikiPath");
            return GetFilePath(wikiPath) != null;
        }

        public void Create(string wikiPagePath, EditedPage page)
        {
            var filePath = wikiPagePath.EndsWith("/")
                ? Path.Combine(_rootFilePath, wikiPagePath.TrimStart('/') + "index.md")
                : Path.Combine(_rootFilePath, wikiPagePath.TrimStart('/') + ".md");

            File.WriteAllText(filePath, page.Body, Encoding);
        }

        public void Update(string wikiPagePath, EditedPage page)
        {
            var filePath = wikiPagePath.EndsWith("/")
                ? Path.Combine(_rootFilePath, wikiPagePath.TrimStart('/') + "index.md")
                : Path.Combine(_rootFilePath, wikiPagePath.TrimStart('/') + ".md");

            File.WriteAllText(filePath, page.Body, Encoding);
        }

        private string GetFilePath(string wikiPath)
        {
            var path = wikiPath.Trim('/');
            path = Path.Combine(_rootFilePath, path);

            if (File.Exists(path + "\\index.md"))
                return path + "\\index.md";

            if (File.Exists(path + ".md"))
                return path + ".md";

            return null;
        }
    }
}