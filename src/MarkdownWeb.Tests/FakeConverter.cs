using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownWeb.Tests
{
    public class FakeConverter : IUrlPathConverter
    {
        public string ToWebPath(string wikiPath)
        {
            throw new NotImplementedException();
        }

        public string ToWebPath(string wikiPath, PageReference referringPage)
        {
            throw new NotImplementedException();
        }

        public string RemoveWebRoot(string url)
        {
            throw new NotImplementedException();
        }

        public string ToWebUrl(string wikiPath)
        {
            throw new NotImplementedException();
        }

        public string ToAbsolutePath(string virtualPath)
        {
            throw new NotImplementedException();
        }

        PageReference IUrlPathConverter.MapUrlToWikiPath(string websiteAbsolutePath)
        {
            throw new NotImplementedException();
        }

        public string MapWikiPaths(PageReference currentWikiPath, string linkedWikiPath)
        {
            throw new NotImplementedException();
        }

        public string MapUrlToWikiPath(string websiteAbsolutePath)
        {
            throw new NotImplementedException();
        }

        public string ToWikiPath(string currentWikiPath, string linkedPath)
        {
            throw new NotImplementedException();
        }

        public string ToWebPath(string currentWikiPath, string linkedPath)
        {
            throw new NotImplementedException();
        }

        public string MapWikiPaths(string currentWikiPath, string linkedWikiPath)
        {
            throw new NotImplementedException();
        }
    }
}
