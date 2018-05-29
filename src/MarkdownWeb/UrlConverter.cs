using System;

namespace MarkdownWeb
{
    /// <summary>
    /// 
    /// </summary>
    public class UrlConverter : IUrlPathConverter
    {
        private readonly IPageSource _pageSource;
        private readonly string _rootAbsolutePath;

        public UrlConverter(IPageSource pageSource)
        {
            _pageSource = pageSource ?? throw new ArgumentNullException(nameof(pageSource));
        }

        public UrlConverter(string rootAbsolutePath, IPageSource pageSource)
        {
            _rootAbsolutePath = rootAbsolutePath ?? throw new ArgumentNullException(nameof(rootAbsolutePath));
            _pageSource = pageSource ?? throw new ArgumentNullException(nameof(pageSource));

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
            if (wikiPath.StartsWith("/"))
                wikiPath = wikiPath.Substring(1);

            return _rootAbsolutePath + wikiPath;
        }

        public string ToAbsolutePath(string virtualPath)
        {
            throw new NotImplementedException();
        }

        public PageReference MapUrlToWikiPath(string websiteAbsolutePath)
        {
            if (websiteAbsolutePath == null) throw new ArgumentNullException(nameof(websiteAbsolutePath));

            if (!websiteAbsolutePath.StartsWith("/"))
                websiteAbsolutePath = $"/{websiteAbsolutePath}";
            if (!websiteAbsolutePath.TrimEnd('/').StartsWith(_rootAbsolutePath.TrimEnd('/')))
                throw new InvalidOperationException("That is not a wiki path: " + websiteAbsolutePath);

            var givenWikiPath = websiteAbsolutePath.TrimEnd('/') == _rootAbsolutePath.TrimEnd('/')
                ? "/"
                : websiteAbsolutePath.Remove(0, _rootAbsolutePath.Length - 1);
            var path = givenWikiPath.TrimEnd('/');
            if (path == "" || path == "/")
            {
                return new PageReference("/", "/", "index.md");
            }

            // try get document
            var pos = path.LastIndexOf('/');
            var document = path.Substring(pos + 1);
            path = path.Substring(0, pos + 1);

            // /some/path.md --> /some/path.md
            if (document.Contains("."))
            {
                var givenDocument = new PageReference(givenWikiPath, path, document);
                return _pageSource.PageExists(givenDocument) ? givenDocument : null;
            }

            // /some/path/  --> /some/path.md
            var reference = new PageReference(givenWikiPath, path, document + ".md");
            if (_pageSource.PageExists(reference))
                return reference;

            // /some/path --> /some/path/index.md
            reference = new PageReference(givenWikiPath, path + document, "index.md");
            if (_pageSource.PageExists(reference))
                return reference;

            return null;
        }
    }
}