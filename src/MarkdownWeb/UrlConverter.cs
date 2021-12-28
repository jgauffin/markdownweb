using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarkdownWeb
{
    /// <summary>
    /// 
    /// </summary>
    public class UrlConverter : IUrlPathConverter
    {
        private readonly IPageSource _pageSource;
        private readonly Func<string, string> _virtualPathLookup;
        private readonly string _rootAbsolutePath;

        public UrlConverter(IPageSource pageSource, Func<string, string> virtualPathLookup = null)
        {
            _pageSource = pageSource ?? throw new ArgumentNullException(nameof(pageSource));
            _virtualPathLookup = virtualPathLookup;
        }

        public UrlConverter(string rootAbsolutePath, IPageSource pageSource, Func<string, string> virtualPathLookup = null)
        {
            _rootAbsolutePath = rootAbsolutePath ?? throw new ArgumentNullException(nameof(rootAbsolutePath));
            _pageSource = pageSource ?? throw new ArgumentNullException(nameof(pageSource));
            _virtualPathLookup = virtualPathLookup;

            if (!_rootAbsolutePath.EndsWith("/"))
                _rootAbsolutePath += "/";
            if (!_rootAbsolutePath.StartsWith("/"))
                _rootAbsolutePath = "/" + _rootAbsolutePath;
        }

        public string RemoveWebRoot(string url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            if (!url.StartsWith("/"))
                url = $"/{url}";

            if (!url.StartsWith(_rootAbsolutePath))
                throw new ArgumentException("Url do not start with the web root: " + url);

            return "/" + url.Remove(0, _rootAbsolutePath.Length);
        }

        public string ToWebUrl(string wikiPath)
        {
            if (wikiPath == null) throw new ArgumentNullException(nameof(wikiPath));
            if (wikiPath.StartsWith("~/"))
            {
                return _virtualPathLookup(wikiPath);
            }

            return _rootAbsolutePath + wikiPath.TrimStart('/');
        }

        public string ToAbsolutePath(string virtualPath)
        {
            return _virtualPathLookup?.Invoke(virtualPath) ?? virtualPath;
        }

        public PageReference MapUrlToWikiPath(string websiteAbsolutePath)
        {
            if (websiteAbsolutePath == null) throw new ArgumentNullException(nameof(websiteAbsolutePath));

            if (!websiteAbsolutePath.StartsWith("/"))
                websiteAbsolutePath = $"/{websiteAbsolutePath}";
            if (!websiteAbsolutePath.TrimEnd('/').StartsWith(_rootAbsolutePath.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("That is not a wiki path: " + websiteAbsolutePath);

            var givenWikiPath = websiteAbsolutePath.TrimEnd('/') == _rootAbsolutePath.TrimEnd('/')
                ? "/"
                : websiteAbsolutePath.Remove(0, _rootAbsolutePath.Length - 1);
            var path = givenWikiPath.TrimEnd('/');
            if (path == "" || path == "/")
            {
                return new PageReference("/", "/index.md");
            }

            return ToReference(givenWikiPath);
        }

        public PageReference ToReference(string givenWikiPath)
        {
            if (givenWikiPath == "/")
            {
                return new PageReference("/", $"/{PageReference.IndexFile}");
            }

            // we must always try the last segment as the documentName since friendly Urls are rewritten.
            var withoutLastSlash = givenWikiPath.TrimEnd('/');
            var lastSlashPos = withoutLastSlash.LastIndexOf('/');
            var possibleDocumentName = withoutLastSlash.Substring(lastSlashPos + 1);
            var withoutDocument = withoutLastSlash.Substring(0, lastSlashPos + 1);

            // possible paths:
            //
            // For page documents
            // /some/path.md
            // /some/path
            //
            // For index (same url as above, but index document)
            // /some/path/
            // /some/path/index.md

            var possiblePageReferences = new List<PageReference>();

            if (possibleDocumentName.Contains("."))
            {
                if (possibleDocumentName.Equals(PageReference.IndexFile, StringComparison.OrdinalIgnoreCase))
                {
                    // /some/path/index.md
                    possiblePageReferences.Add(new PageReference(withoutDocument, givenWikiPath));
                }
                else
                {
                    // /some/path.md
                    possiblePageReferences.Add(new PageReference(withoutDocument + Path.GetFileNameWithoutExtension(possibleDocumentName) + "/", givenWikiPath));
                    possiblePageReferences.Add(new PageReference(givenWikiPath, givenWikiPath));
                }
            }
            else
            {
                // /some/path/ (path.md)
                possiblePageReferences.Add(new PageReference(givenWikiPath, withoutLastSlash + ".md"));

                // /some/path/ (index.md)
                possiblePageReferences.Add(new PageReference(givenWikiPath, withoutLastSlash + "/index.md"));
            }

            return possiblePageReferences.FirstOrDefault(reference => _pageSource.PageExists(reference));
        }
    }
}