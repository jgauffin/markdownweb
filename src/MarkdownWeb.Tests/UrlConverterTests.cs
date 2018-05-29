using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MarkdownWeb.Tests
{
    public class UrlConverterTests
    {
        public UrlConverterTests()
        {
        }
        [Fact]
        public void getting_root_should_give_us_the_index_document_from_the_root()
        {
            var pageSource = Substitute.For<IPageSource>();

            var sut = new UrlConverter("/intranet/", pageSource);
            var actual = sut.ToWebUrl("/");

            actual.Should().Be("/intranet/");
        }

        [Fact]
        public void wiki_path_should_be_appended_to_the_root_url()
        {
            var pageSource = Substitute.For<IPageSource>();

            var sut = new UrlConverter("/trafikinfo/Intranet/doc/", pageSource);
            var actual = sut.ToWebUrl("/FolderTest/");

            actual.Should().Be("/trafikinfo/Intranet/doc/FolderTest/");
        }

        [Fact]
        public void drop_root_url_from_wiki_url()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);

            var sut = new UrlConverter("/intranet/", pageSource);
            var actual = sut.MapUrlToWikiPath("/intranet/");

            actual.RealWikiPath.Should().Be("/");
        }

        [Fact]
        public void drop_root_url_from_wiki_url_when_trailing_slash_is_missing()
        {
            var pageSource = Substitute.For<IPageSource>();

            var sut = new UrlConverter("/intranet/", pageSource);
            var actual = sut.MapUrlToWikiPath("/intranet");

            actual.RealWikiPath.Should().Be("/");
        }


        [Fact]
        public void empty_string_should_give_us_the_index_document_from_the_root()
        {
            var pageSource = Substitute.For<IPageSource>();

            var sut = new UrlConverter("/", pageSource);
            var actual = sut.MapUrlToWikiPath("");

            actual.RealWikiPath.Should().Be("/");
        }

    }
}
