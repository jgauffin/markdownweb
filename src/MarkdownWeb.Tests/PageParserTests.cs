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
            var actual = sut.ParseUrl("/withlink");

            actual.Body.Should().Contain("intranet/NoDoc/image.png");
        }

        [Fact]
        public void parses_page_link_correctly()
        {


            var sut = new PageParser(Environment.CurrentDirectory + "\\TestDocs\\", "/intranet/");
            var actual = sut.ParseUrl("/withlink");

            actual.Body.Should().Contain("intranet/FolderTest");
        }
    }
}
