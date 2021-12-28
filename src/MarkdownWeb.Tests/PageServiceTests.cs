using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using MarkdownWeb.PostFilters;
using MarkdownWeb.Storage.Files;
using NSubstitute;
using Xunit;

namespace MarkdownWeb.Tests
{
    public class PageServiceTests
    {
        [Fact]
        public void parses_image_link_correctly()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseUrl("/intranet/withlink");

            actual.Body.Should().Contain("/intranet/?image=/NoDoc/image.png");
        }
        
        [Fact]
        public void parses_image_link_correctly_when_using_path_alias_for_document()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseUrl("/intranet/foldertest/other/");

            actual.Body.Should().Contain("/intranet/?image=/NoDoc/image.png");
        }

        [Fact]
        public void parses_page_link_correctly()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseUrl("/intranet/withlink");

            actual.Body.Should()
                .Contain("/intranet/FolderTest", "because links are clickable and not wiki relative in the HTML pages.");
            actual.Body.Should()
                .Contain("/intranet/FolderTest/other/", "because path links to other folders should be supported");
            actual.Body.Should()
                .Contain("/intranet/page/", "because links on same level should also work.");
        }

        [Fact]
        public void parses_page_link_correctly_when_using_path_alias()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseUrl("/intranet/FolderTest/subdocWithLinks/");

            actual.Body.Should()
                .Contain("/intranet/page", "because links are clickable and not wiki relative in the HTML pages.");
            actual.Body.Should()
                .Contain("/intranet/FolderTest/other", "because path links to other folders should be supported");
            actual.Body.Should()
                .Contain("/intranet/FolderTest/subfolder/twodoc", "because links on same level should also work.");
        }

        [Fact]
        public void headings_should_get_ids_for_anchors()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            sut.PostFilters.Add(new AnchorHeadings());
            var actual = sut.ParseUrl("/intranet/FolderTest/other.md");

            actual.Body.Should()
                .Contain("id=\"this-is-id\"");
        }


        [Fact]
        public void finds_http_link_correctly()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseString("/intranet/", @"

http://this.link

");

            actual.Body.Should().Contain(@"href=""http://this.link""");
        }

        [Fact]
        public void finds_www_link_correctly()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");
            var reference = new PageReference("/" , "/index.md");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseString("/intranet/", @"
hello world

this www.onetrueerror.com is a

link
");

            actual.Body.Should().Contain(@"href=""http://www.onetrueerror.com""");
        }

        [Fact]
        public void do_not_destroy_regular_anchor_links()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseString("/intranet/", @"
hello world

this <a href=""http://www.onetrueerror.com"">regular</a> is a

link
");

            actual.Body.Should().Contain(@"href=""http://www.onetrueerror.com""");
        }

        [Fact]
        public void finds_unc_path_correctly()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseString("/intranet/", @"

\\unc_path\to\folder

");

            actual.Body.Should().Contain(@"href=""file://\\unc_path\to\folder""");
        }

        [Fact]
        public void markdown_parsing_does_not_destroy_tables()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseString("/intranet/", @"

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
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseString("/intranet/", @"

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
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseString("/intranet/", @"

<h1>Parse specific</h1>

");

            actual.Body.Should().Contain(@"id=""Parsespecific""");
        }

        [Fact]
        public void should_work_with_relative_link_to_subfolder_from_non_root_folder()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseUrl("/intranet/FolderTest/index.md");

            actual.Body.Should().Contain(@"/FolderTest/subfolder/twodoc/");
        }

        [Fact]
        public void Should_not_mark_existing_links_as_missing()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseUrl("/intranet/withlink.md");

            actual.Links.All(x => x.IsMissing != true).Should().BeTrue();
        }

        [Fact]
        public void Should_work_with_index_pages()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseUrl("/intranet/foldertest/");

            
        }


        [Fact]
        public void Should_mark_missing_links_as_missing()
        {
            var pageSource = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseUrl("/intranet/page.md");

            actual.Links.Any(x => x.IsMissing == true).Should().BeTrue();
        }

        [Fact]
        public void parse_code_blocks()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseString("/intranet/", @"

```csharp
for (int i = 0; i < 10; ++i)
{
}
```

");

            actual.Body.Should().Contain(@"code class=""language-csharp""");
        }

        
        [Fact]
        public void ListAllPages()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.GetPages();

            actual.First().Url.Should().Be("/intranet/");
            actual[1].Url.Should().Be("/intranet/page/");
        }

        [Fact]
        public void Generate_list_when_page_is_missing_and_there_are_multiple_matches()
        {
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");
            var pathConverter = new UrlConverter("/intranet/", repository);

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseUrl("/intranet/some/twodoc/");

            actual.Body.Should().Contain("/FolderTest/subfolder/twodoc");
            actual.Body.Should().Contain("/NoDoc/TwoDoc");
        }

        [Fact] 
        public void Return_contents_if_only_one_missing_page()
        {
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");
            var pathConverter = new UrlConverter("/intranet/", repository);

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseUrl("/intranet/some/other/");

            actual.Body.Should().Contain("This is other");
        }

        [Fact]
        public void Return_contents_if_only_one_missing_page_and_using_segments()
        {
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");
            var pathConverter = new UrlConverter("/intranet/", repository);

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseUrl("/intranet/some/subfolder/twodoc");

            actual.Body.Should().Contain("Testing nesting");
        }

        [Fact]
        public void GenerateIndex()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.GenerateIndex("/intranet/");

            Debug.WriteLine(actual);
        }

        [Fact]
        public void ListMissingPages()
        {
            var pageSource = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");

            var sut = new PageService(repository, pathConverter);
            var actual = sut.GetMissingPages();

            actual[0].Url.Should().Be("/intranet/some/missing/");
            actual[0].References[0].Should().Be("/index.md");
        }
    }
}