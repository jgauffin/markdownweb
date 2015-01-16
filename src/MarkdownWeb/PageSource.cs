using System;
using System.IO;
using System.Text;

namespace MarkdownWeb
{
    public class PageSource : IPageSource
    {
        private string _rootUri;
        private string _rootDirectory;

        public PageSource(string rootUri, string rootDirectory)
        {
            if (rootUri == null) throw new ArgumentNullException("rootUri");
            if (rootDirectory == null) throw new ArgumentNullException("rootDirectory");
            if (!Directory.Exists(rootDirectory))
                throw new DirectoryNotFoundException("The root directory must exist: " + rootDirectory);

            _rootUri = rootUri.Trim('/');
            _rootDirectory = rootDirectory;
        }

        public string GetAbsoluteUrl(string currentPageUrl, string linkedUrl)
        {
            var url = currentPageUrl.Trim('/');
            if (url.StartsWith(_rootUri))
                url = url.Remove(0, _rootUri.Length).TrimStart('/');

            if (IsFileUrl(url))
            {
                var pos = url.LastIndexOf('/');
                url = pos > -1 ? url.Remove(pos) : "";
            }

            linkedUrl = linkedUrl.Trim('/');
            while (linkedUrl.StartsWith(".."))
            {
                linkedUrl = linkedUrl.Remove(0, 2).TrimStart('/');

                var pos = url.LastIndexOf('/');
                if (pos == -1)
                {
                    url = "";
                    break;
                }
                url = url.Remove(pos);
            }

            if (url == "" && linkedUrl == "")
                return "/";

            return url == "" ? linkedUrl : url + '/' + linkedUrl;
        }

        private bool IsFileUrl(string url)
        {
            if (url.StartsWith(_rootUri))
                url = url.Remove(0, _rootUri.Length).TrimStart('/');

            var fullPath = Path.Combine(_rootDirectory, url.Replace('/', '\\'));
            return File.Exists(fullPath + ".md");
        }


        private string GetFullPath(string url)
        {
            if (url.StartsWith(_rootUri))
                url = url.Remove(0, _rootUri.Length).TrimStart('/');

            var fullPath = Path.Combine(_rootDirectory, url.Replace('/', '\\'));
            if (Directory.Exists(fullPath))
                fullPath = Path.Combine(fullPath, "index.md");
            else if (!fullPath.EndsWith(".md"))
                fullPath += ".md";

            return fullPath;
        }

        private string ParseUrl(string url)
        {
            url = url.Trim('/');

            var path = GetFullPath(url);

            ////direct link
            //if (!path.EndsWith("index.md"))
            //{
            //    var pos = url.LastIndexOf('/');
            //    if (pos == -1)
            //        _currentUrlPath = "/";
            //    else
            //        _currentUrlPath = url.Substring(0, pos);
            //}
            //else
            //    _currentUrlPath = url;

            //_currentFilePath = Path.GetDirectoryName(path);
            return path;
        }


        public bool PageExists(string currentPageUrl, string linkedUrl)
        {
            var url = GetAbsoluteUrl(currentPageUrl, linkedUrl);
            var path = GetFullPath(url);
            return File.Exists(path);
        }

        public bool PageExists(string url)
        {
            var path = GetFullPath(url);
            return File.Exists(path);
        }

        public string GetContent(string url)
        {
            var filePath = ParseUrl(url);

            //return File.ReadAllText(filePath, Encoding.UTF8);
            Encoding encoding;
            return FileHelper.ReadEncoded(filePath, out encoding);
        }
    }
}