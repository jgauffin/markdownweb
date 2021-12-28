using System;
using FluentAssertions;
using MarkdownWeb.Storage.Files;
using NSubstitute;
using Xunit;

namespace MarkdownWeb.Tests
{
    public class UrlConverterTests
    {
        private FileBasedRepository _repository;

        public UrlConverterTests()
        {
            _repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");
        }

        [Fact]
        public void getting_root_should_give_us_the_index_document_from_the_root()
        {

            var sut = new UrlConverter("/intranet/", _repository);
            var actual = sut.ToWebUrl("/");

            actual.Should().Be("/intranet/");
        }

        [Fact]
        public void should_use_virtualLookuper_when_specified()
        {

            var sut = new UrlConverter("/trafikinfo/Intranet/doc/", _repository, x => $"/mofo{x.Remove(0,1)}");
            var actual = sut.ToWebUrl("~/FolderTest/");

            actual.Should().Be("/mofo/FolderTest/");
        }

        [Fact]
        public void wiki_path_should_be_appended_to_the_root_url()
        {

            var sut = new UrlConverter("/trafikinfo/Intranet/doc/", _repository);
            var actual = sut.ToWebUrl("/FolderTest/");

            actual.Should().Be("/trafikinfo/Intranet/doc/FolderTest/");
        }

        [Fact]
        public void wiki_path_should_add_index_document_for_path_only_urls()
        {

            var sut = new UrlConverter("/Intranet/", _repository);
            var actual = sut.MapUrlToWikiPath("/intranet/foldertest/");

            actual.WikiUrl.Should().Be("/foldertest/index.md");
        }

        [Fact]
        public void ToWebUrl_should_base_on_wikiRoot_for_urls_starting_with_slash()
        {

            var sut = new UrlConverter("/Intranet/", _repository);
            var actual = sut.ToWebUrl("/foldertest/");

            actual.Should().Be("/Intranet/foldertest/");
        }

        [Fact]
        public void ToWebUrl_should_base_build_on_root()
        {

            var sut = new UrlConverter("/Intranet/", _repository);
            var actual = sut.ToWebUrl("/foldertest/");

            actual.Should().Be("/Intranet/foldertest/");
        }

        [Fact]
        public void drop_root_url_from_wiki_url()
        {

            var sut = new UrlConverter("/intranet/", _repository);
            var actual = sut.MapUrlToWikiPath("/intranet/");

            actual.WikiUrl.Should().Be("/index.md");
            actual.FriendlyWikiUrl.Should().Be("/");
        }

        [Fact]
        public void drop_root_url_from_wiki_url_when_trailing_slash_is_missing()
        {

            var sut = new UrlConverter("/intranet/", _repository);
            var actual = sut.MapUrlToWikiPath("/intranet");

            actual.WikiUrl.Should().Be("/index.md");
            actual.FriendlyWikiUrl.Should().Be("/");
        }

        [Fact]
        public void Should_make_friendly_path_for_child_document()
        {
            var sut = new UrlConverter("/intranet/", _repository);

            var actual = sut.ToReference("/FolderTest/other");

            actual.WikiUrl.Should().Be("/FolderTest/other.md");
            actual.FriendlyWikiUrl.Should().Be("/FolderTest/other/");
        }

        [Fact]
        public void Should_support_image_urls()
        {

            var sut = new UrlConverter("/intranet/", _repository);
            var actual = sut.ToReference("/NoDoc/image.png");

            actual.Should().NotBeNull();
        }


        [Fact]
        public void empty_string_should_give_us_the_index_document_from_the_root()
        {

            var sut = new UrlConverter("/", _repository);
            var actual = sut.MapUrlToWikiPath("");

            actual.WikiUrl.Should().Be("/index.md");
            actual.FriendlyWikiUrl.Should().Be("/");
        }

    }
}
