using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownWeb.Tests
{
    class FakeService: IMarkdownParser
    {
        public string Parse(string currentPagePath, string text)
        {
            return text;
        }

        public string Parse(PageReference currentPage, string text)
        {
            throw new NotImplementedException();
        }
    }
}
