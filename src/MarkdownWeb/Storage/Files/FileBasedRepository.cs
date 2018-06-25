using System;
using System.Collections.Generic;
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
    public class FileBasedRepository : IPageRepository, IPageSource
    {
        private readonly string _rootFilePath;

        /// <summary>
        ///     Create a new instance of <see cref="FileBasedRepository" />.
        /// </summary>
        /// <param name="rootFilePath">Root folder where all markdown files are located.</param>
        public FileBasedRepository(string rootFilePath)
        {
            _rootFilePath = rootFilePath ?? throw new ArgumentNullException("rootFilePath");
            Encoding = Encoding.UTF8;
        }

        public Encoding Encoding { get; set; }

        public StoredPage Get(PageReference pageUrl)
        {
            if (pageUrl == null) throw new ArgumentNullException("pageUrl");
            var fileName = GetFilePath(pageUrl);
            if (fileName == null)
                return null;

            var fileContents = File.ReadAllText(fileName, Encoding);
            return new StoredPage
            {
                Body = fileContents,
                Name = fileName,
                CreatedAtUtc = File.GetLastWriteTimeUtc(fileName)
            };
        }

        public StoredPage Get(PageReference pageUrl, int revision)
        {
            if (pageUrl == null) throw new ArgumentNullException("pageUrl");
            return Get(pageUrl);
        }

        public PageMetadata[] GetRevisions(PageReference pageReference)
        {
            if (pageReference == null) throw new ArgumentNullException("pageReference");
            var page = Get(pageReference);
            if (page == null)
                return null;

            return new[]
            {
                new PageMetadata
                {
                    CreatedAtUtc = page.CreatedAtUtc,
                    ChangeComment = "No comment"
                }
            };
        }

        public IEnumerable<string> GetAllPagesAsLinks()
        {
            var directory = _rootFilePath.EndsWith("/") ? _rootFilePath + "/" : _rootFilePath;
            foreach (var file in ScanDirectory(directory, directory)) yield return file;
        }

        public bool Exists(PageReference page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            return GetFilePath(page) != null;
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

        public bool PageExists(PageReference page)
        {
            return Exists(page);
        }

        private string GetFilePath(PageReference page)
        {
            var path = page.RealWikiPath.Trim('/');
            path = Path.Combine(_rootFilePath, path, page.Filename);
            return File.Exists(path) ? path : null;
        }

        private IEnumerable<string> ScanDirectory(string rootDirectory, string directoryToScan)
        {
            var files = Directory.GetFiles(directoryToScan, "*.md");
            foreach (var file in files)
            {
                var str = file.Remove(0, rootDirectory.Length).Replace('\\', '/');
                yield return str.StartsWith("/") ? str : "/" + str;
            }

            var dirs = Directory.GetDirectories(directoryToScan);
            foreach (var dir in dirs)
            {
                foreach (var file in ScanDirectory(rootDirectory, dir)) yield return file;
            }
        }
    }
}