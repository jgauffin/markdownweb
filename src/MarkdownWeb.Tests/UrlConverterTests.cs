using FluentAssertions;
using Xunit;

namespace MarkdownWeb.Tests
{
    public class UrlConverterTests
    {
        [Fact]
        public void getting_root_should_give_us_the_index_document_from_the_root()
        {

            var sut = new UrlConverter("/intranet/");
            var actual = sut.ToWebPath("/");

            actual.Should().Be("/intranet/");
        }

        [Fact]
        public void wiki_path_should_be_appended_to_the_root_url()
        {


            var sut = new UrlConverter("/trafikinfo/Intranet/doc/");
            var actual = sut.ToWebPath("/FolderTest/");

            actual.Should().Be("/trafikinfo/Intranet/doc/FolderTest/");
        }

        [Fact]
        public void drop_root_url_from_wiki_url()
        {


            var sut = new UrlConverter("/intranet/");
            var actual = sut.MapUrlToWikiPath("/intranet/");

            actual.Should().Be("/");
        }

        [Fact]
        public void page_links_should_work_when_both_are_on_start_level()
        {

            var sut = new UrlConverter("/intranet/");
            var actual = sut.MapWikiPaths("/withlink", "page");

            actual.Should().Be("/page");
        }

        [Fact]
        public void page_links_should_work_linking_one_level_up()
        {

            var sut = new UrlConverter("/intranet/");
            var actual = sut.MapWikiPaths("/withlink/is", "../page");

            actual.Should().Be("/page");
        }

        [Fact]
        public void page_links_when_relative_to_same_level()
        {

            var sut = new UrlConverter("/intranet/");
            var actual = sut.MapWikiPaths("/withlink/is", "../more/page");

            actual.Should().Be("/more/page");
        }

        [Fact]
        public void page_links_when_linking_deeper()
        {

            var sut = new UrlConverter("/intranet/");
            var actual = sut.MapWikiPaths("/is", "../more/page");

            actual.Should().Be("/more/page");
        }


        [Fact]
        public void empty_string_should_give_us_the_index_document_from_the_root()
        {

            var sut = new UrlConverter("/");
            var actual = sut.MapUrlToWikiPath("");

            actual.Should().Be("/");
        }


        [Fact]
        public void linking_root_should_work_from_sub_folder()
        {


            var sut = new UrlConverter("/intranet/");
            var actual = sut.MapWikiPaths("/FolderTest/", "..");

            actual.Should().Be("/");
        }

        [Fact]
        public void linking_sub_folder_from_root()
        {


            var sut = new UrlConverter("/intranet/");
            var actual = sut.MapWikiPaths("/", "FolderTest");

            actual.Should().Be("/FolderTest");
        }


        [Fact]
        public void linking_sub_image_from_root_level()
        {


            var sut = new UrlConverter("/intranet/");
            var actual = sut.MapWikiPaths("/", "NoDoc/image.png");

            actual.Should().Be("/NoDoc/image.png");
        }

        [Fact]
        public void linking_image_from_sibling_folder()
        {


            var sut = new UrlConverter("/intranet/");
            var actual = sut.MapWikiPaths("/FolderTest", "../NoDoc/image.png");

            actual.Should().Be("/NoDoc/image.png");
        }


        public void linking_image_from_same_lavel_should_make_it_an_absolute_path()
        {
            var sut = new UrlConverter("/intranet/");
            var actual = sut.MapWikiPaths("withlink", "NoDoc/image.png");
        }

    }
}
