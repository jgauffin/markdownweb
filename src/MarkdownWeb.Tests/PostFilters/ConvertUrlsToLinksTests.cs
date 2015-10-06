using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownWeb.PostFilters;
using Xunit;

namespace MarkdownWeb.Tests.PostFilters
{
    public class ConvertUrlsToLinksTests
    {
        [Fact]
        public void Run()
        {
            var ctx = new PostFilterContext(@"<a href=""/new/image.png"">test</a>");

            var sut = new ConvertUrlsToLinks();
            var actual = sut.Parse(ctx);

           
        }
    }
}
