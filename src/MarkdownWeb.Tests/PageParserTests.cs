using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace MarkdownWeb.Tests
{
    public class PageParserTests
    {
        [Fact]
        public void parses_image_link_correctly()
        {


            var sut = new PageParser(Environment.CurrentDirectory + "\\TestDocs\\", "/intranet/");
            sut.VirtualPathHandler = OnResolvePath;
            var actual = sut.ParseUrl("/withlink");

            actual.Body.Should().Contain("/intranet?image=NoDoc/image.png");
        }

        private string OnResolvePath(string arg)
        {
            return arg.TrimStart('~');
        }

        [Fact]
        public void parses_page_link_correctly()
        {


            var sut = new PageParser(Environment.CurrentDirectory + "\\TestDocs\\", "/intranet/");
            sut.VirtualPathHandler = OnResolvePath;
            var actual = sut.ParseUrl("/withlink");

            actual.Body.Should().Contain("intranet/FolderTest");
        }

        [Fact]
        public void finds_http_link_correctly()
        {
            var sut = new PageParser(Environment.CurrentDirectory + "\\TestDocs\\", "/intranet/");
            var actual = sut.ParseString("/", @"

http://this.link

");

            actual.Body.Should().Contain(@"href=""http://this.link""");
        }

        [Fact]
        public void finds_www_link_correctly()
        {
            var sut = new PageParser(Environment.CurrentDirectory + "\\TestDocs\\", "/intranet/");
            sut.VirtualPathHandler = OnResolvePath;
            var actual = sut.ParseString("/", @"

www.onetrueerror.com

");

            actual.Body.Should().Contain(@"href=""http://www.onetrueerror.com""");
        }

        [Fact]
        public void finds_unc_path_correctly()
        {
            var sut = new PageParser(Environment.CurrentDirectory + "\\TestDocs\\", "/intranet/");
            sut.VirtualPathHandler = OnResolvePath;
            var actual = sut.ParseString("/", @"

\\unc_path\to\folder

");

            actual.Body.Should().Contain(@"href=""file://\\unc_path\to\folder""");
        }

        [Fact]
        public void headings_should_be_anchored()
        {
            var sut = new PageParser(Environment.CurrentDirectory + "\\TestDocs\\", "/intranet/");
            sut.VirtualPathHandler = OnResolvePath;
            var actual = sut.ParseString("/", @"

<h1>Parse specific</h1>

");

            actual.Body.Should().Contain(@"id=""Parsespecific""");
        }
    }
}
