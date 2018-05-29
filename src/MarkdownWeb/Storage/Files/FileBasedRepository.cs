using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MarkdownWeb.Helpers;

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

        public StoredPage Get(string wikiPagePath)
        {
            if (wikiPagePath == null) throw new ArgumentNullException("wikiPagePath");
            var fileName = GetFilePath(wikiPagePath);
            if (fileName == null)
                return null;

            var fileContents = File.ReadAllText(fileName, Encoding);
            return new StoredPage
            {
                Body = fileContents,
                Name = fileName,
                CreatedAtUtc = File.GetLastWriteTimeUtc(fileName),
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
                    ChangeComment = "No comment"
                }
            };
        }

        public IEnumerable<string> GetAllPagesAsLinks()
        {
            var directory = _rootFilePath.EndsWith("/") ? _rootFilePath + "/" : _rootFilePath;
            foreach (var file in ScanDirectory(directory, directory))
            {
                yield return file;
            }
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
                foreach (var file in ScanDirectory(rootDirectory, dir))
                {
                    yield return file;
                }
            }
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

            if (wikiPath.Contains(".md"))
            {
                return File.Exists(path) ? path : null;
            }


            if (File.Exists(path + "\\index.md"))
                return path + "\\index.md";

            if (File.Exists(path + ".md"))
                return path + ".md";

            return null;
        }

        public bool PageExists(PageReference page)
        {
            return Exists(page.RealWikiPath + "/" + page.Filename);
        }
    }
}