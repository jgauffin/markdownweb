using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownWeb.Tests
{
    public class FakeConverter : IUrlPathConverter
    {
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

        public PageReference MapUrlToWikiPath(string websiteAbsolutePath)
        {
            throw new NotImplementedException();
        }

        public PageReference ToReference(string wikiPath)
        {
            throw new NotImplementedException();
        }
    }
}
