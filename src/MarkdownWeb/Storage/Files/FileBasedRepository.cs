using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public IEnumerable<string> GetAllPagesAsLinks(Func<string, bool> pathFilter)
        {
            var directory = _rootFilePath.EndsWith("/") ? _rootFilePath + "/" : _rootFilePath;
            var items = new List<string>();
            ScanDirectoryForFiles(directory, directory, items, pathFilter);
            return items;
        }

        /// <summary>
        /// List all pages that exist under a specific path.
        /// </summary>
        /// <param name="startWikiPath">Start path, "" or "/" = root.</param>
        /// <param name="pathFilter"></param>
        /// <returns></returns>
        public IReadOnlyList<PageReferenceWithChildren> GetAllPages(string startWikiPath, Func<string, bool> pathFilter)
        {
            if (startWikiPath == null)
            {
                throw new ArgumentNullException(nameof(startWikiPath));
            }

            var node = new PageReferenceWithChildren("/", "/index.md");
            startWikiPath = startWikiPath.TrimStart('/');
            var path = _rootFilePath + startWikiPath.Replace('/', '\\');

            ScanDirectory(_rootFilePath, path, node, pathFilter);
            foreach (var child in node.Children)
            {
                AdjustFriendlyUrls(child);
            }
            return node.Children;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <remarks>
        ///<para>
        ///Once we have scanned documents we have to convert files to url paths.
        /// </para>
        /// </remarks>
        private void AdjustFriendlyUrls(PageReferenceWithChildren node)
        {
            foreach (var child in node.Children)
            {
                if (child.IsDirectory)
                    continue;

                var ourSuggestedPath = child.WikiUrl.Replace(".md", "/index.md");

                // find all pages that are not directories (or index.md)
                // and check if they can get a more friendly name, i.e. "/some/url/" instead of "/some/url.md"
                if (node.Children.All(x => x.WikiUrl != ourSuggestedPath))
                {
                    child.FriendlyWikiUrl = child.WikiUrl.Replace(".md", "");
                }

                AdjustFriendlyUrls(child);
            }
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
            var path = page.WikiUrl.Trim('/').Replace('/', '\\');
            path = Path.Combine(_rootFilePath, path);
            return File.Exists(path) ? path : null;
        }

        /// <summary>
        /// This method do not adjust friendly path, do that in a second pass using <see cref="AdjustFriendlyUrls"/>.
        /// </summary>
        /// <param name="rootDirectory"></param>
        /// <param name="directoryToScan"></param>
        /// <param name="parent"></param>
        private void ScanDirectory(string rootDirectory, string directoryToScan, PageReferenceWithChildren parent, Func<string, bool> pathFilter)
        {
            var ourPath = "/" + directoryToScan.Remove(0, rootDirectory.Length).Replace('\\', '/');
            if (ourPath.Length > 2)
            {
                ourPath += "/";
            }

            PageReferenceWithChildren node;
            if (ourPath == "/")
            {
                node = parent;
            }
            else
            {
                node = new PageReferenceWithChildren(ourPath, $"{ourPath}{PageReference.IndexFile}") { IsDirectory = true };
                parent.AddChild(node);
            }

            var files = Directory.GetFiles(directoryToScan, "*.md");
            foreach (var file in files)
            {
                var fullPath = "/" + file.Remove(0, rootDirectory.Length).Replace('\\', '/');
                if (Path.GetFileName(file) == PageReference.IndexFile)
                {
                    // it's always added.
                    continue;
                }

                var child = new PageReferenceWithChildren(fullPath, fullPath);
                node.AddChild(child);
            }

            var dirs = Directory.GetDirectories(directoryToScan);
            foreach (var dir in dirs)
            {
                if (!pathFilter(dir))
                {
                    continue;
                }

                ScanDirectory(rootDirectory, dir, node, pathFilter);
            }
        }

        private void ScanDirectoryForFiles(
            string rootDirectory, 
            string directoryToScan, 
            ICollection<string> items,
            Func<string, bool> pathFilter)
        {
            var files = Directory.GetFiles(directoryToScan, "*.md");
            foreach (var file in files)
            {
                var str = file.Remove(0, rootDirectory.Length).Replace('\\', '/');
                items.Add(str.StartsWith("/") ? str : "/" + str);
            }

            var dirs = Directory.GetDirectories(directoryToScan);
            foreach (var dir in dirs)
            {
                if (!pathFilter(dir))
                {
                    continue;
                }

                ScanDirectoryForFiles(rootDirectory, dir, items, pathFilter);
            }
        }
    }
}