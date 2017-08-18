using System;

namespace MarkdownWeb
{
    /// <summary>
    /// 
    /// </summary>
    public class UrlConverter : IUrlPathConverter
    {
        private readonly string _rootAbsolutePath;

        public UrlConverter(string rootAbsolutePath)
        {
            if (rootAbsolutePath == null) throw new ArgumentNullException("rootAbsolutePath");
            _rootAbsolutePath = rootAbsolutePath;
            if (!_rootAbsolutePath.EndsWith("/"))
                _rootAbsolutePath += "/";
            if (!_rootAbsolutePath.StartsWith("/"))
                _rootAbsolutePath = "/" + _rootAbsolutePath;
        }

        public string ToWebPath(string wikiPath)
        {
            if (wikiPath == null) throw new ArgumentNullException("wikiPath");
            if (wikiPath.StartsWith("/"))
                wikiPath = wikiPath.Substring(1);

            return _rootAbsolutePath + wikiPath;
        }

        public string ToAbsolutePath(string virtualPath)
        {
            throw new NotImplementedException();
        }

        public string MapUrlToWikiPath(string websiteAbsolutePath)
        {
            if (!websiteAbsolutePath.StartsWith(("/")))
                websiteAbsolutePath = "/" + websiteAbsolutePath;
            if (!websiteAbsolutePath.StartsWith(_rootAbsolutePath))
                throw new InvalidOperationException("That is not a wiki path: " + websiteAbsolutePath);

            //-1 to preserve the last slash
            return websiteAbsolutePath.Remove(0, _rootAbsolutePath.Length - 1);

        }

        public string ToWikiPath(string currentWikiPath, string websiteAbsolutePath)
        {
            if (websiteAbsolutePath == null) throw new ArgumentNullException("websiteAbsolutePath");
            if (!websiteAbsolutePath.StartsWith("/"))
                websiteAbsolutePath = "/" + websiteAbsolutePath;

            if (!websiteAbsolutePath.StartsWith(_rootAbsolutePath))
                throw new InvalidOperationException(
                    "Path doesn't start with the root path. It cannot be a wiki page: " + websiteAbsolutePath);

            return websiteAbsolutePath.Remove(0, _rootAbsolutePath.Length);
        }

        public string MapWikiPaths(string currentWikiPath, string linkedWikiPath)
        {
            if (!currentWikiPath.StartsWith("/"))
                throw new InvalidOperationException("Current page path must be absolute.");

            if (linkedWikiPath.StartsWith("/"))
                return ToWebPath(linkedWikiPath);

            var uri = new Uri(new Uri("http://localhost" + currentWikiPath), linkedWikiPath);
            var path = uri.AbsolutePath;
            return path;
        }


        public string ToWebPath(string currentWikiPath, string linkedPath)
        {
            throw new NotImplementedException();
        }
    }
}