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
hello world

this www.onetrueerror.com is a

link
");

            actual.Body.Should().Contain(@"href=""http://www.onetrueerror.com""");
        }

        [Fact]
        public void do_not_destroy_regular_anchor_links()
        {
            var sut = new PageParser(Environment.CurrentDirectory + "\\TestDocs\\", "/intranet/");
            sut.VirtualPathHandler = OnResolvePath;
            var actual = sut.ParseString("/", @"
hello world

this <a href=""http://www.onetrueerror.com"">regular</a> is a

link
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
        public void markdown_parsing_does_not_destroy_tables()
        {
            var sut = new PageParser(Environment.CurrentDirectory + "\\TestDocs\\", "/intranet/");
            sut.VirtualPathHandler = OnResolvePath;
            var actual = sut.ParseString("/", @"

this | is  a | table header
----- | ----- | ----------
*italic* | description | comment
**bold** | description | comment
***italic bold*** | description | comment
http://autolink.com | yeay | wow

");

            actual.Body.Should().Contain(@"</table>");
        }

        [Fact]
        public void markdown_parsing_does_not_destroy_more_complex_tables()
        {
            var sut = new PageParser(Environment.CurrentDirectory + "\\TestDocs\\", "/intranet/");
            sut.VirtualPathHandler = OnResolvePath;
            var actual = sut.ParseString("/", @"

System         | Alias            | Nod 1            | Nod 2 
---------------|-------------------|-----------------|-------
**Winservice**  |  \\blgn165282  |            |         
**Mellanlager** | [AnnoTest](http://AnnoTest)    | \\blgn201086    | \\blgn201087 
**AnnoCache**   |                  | \\blgn201158    | \\blgn201159
**Databas**	    |  blgn165277     |                   |     

System         | Alias            | Nod 1            | Nod 2 
---------------|-------------------|-----------------|-------
**Winservice**  |  \\blgn165282  |            |         
**Mellanlager** | [AnnoTest](http://AnnoTest)    | \\blgn201086    | \\blgn201087 
**AnnoCache**   |                  | \\blgn201158    | \\blgn201159
**Databas**	    |  blgn165277     |                   |     



");

            actual.Body.Should().Contain(@"</table>");
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


        [Fact]
        public void parse_code_blocks()
        {
            var sut = new PageParser(Environment.CurrentDirectory + "\\TestDocs\\", "/intranet/");
            sut.VirtualPathHandler = OnResolvePath;
            var actual = sut.ParseString("/", @"

```csharp
for (int i = 0; i < 10; ++i)
{
}
```

");

            actual.Body.Should().Contain(@"code data-lang=""csharp"" class=""language-csharp""");
        }

    }
}
