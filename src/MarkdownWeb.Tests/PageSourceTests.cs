using System;
using FluentAssertions;
using Xunit;

namespace MarkdownWeb.Tests
{
    public class PageSourceTests
    {
        [Fact]
        public void getting_root_should_give_us_the_index_document_from_the_root()
        {

            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetContent("/");

            actual.Should().Be("index");
        }

        [Fact]
        public void larger_path()
        {


            var sut = new PageSource("/trafikinfo/Intranet/doc/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetContent("/FolderTest/");

            actual.Should().Be("indexfolder");
        }

        [Fact]
        public void drop_root_url_from_url()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetContent("/intranet/");

            actual.Should().Be("index");
        }

        [Fact]
        public void two_documents_on_the_same_level()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetAbsoluteUrl("withlink", "page");

            actual.Should().Be("page");
        }

        [Fact]
        public void empty_string_should_give_us_the_index_document_from_the_root()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetContent("");

            actual.Should().Be("index");
        }

        [Fact]
        public void folerName_should_give_us_the_index_document_in_that_folder()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetContent("FolderTest");

            actual.Should().Be("indexfolder");
        }

        [Fact]
        public void folderPath_should_give_us_the_index_document_in_that_folder()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetContent("/FolderTest");

            actual.Should().Be("indexfolder");
        }

        [Fact]
        public void folderPath_with_Trailing_slash_should_give_us_the_index_document_in_that_folder()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetContent("/FolderTest/");

            actual.Should().Be("indexfolder");
        }

        [Fact]
        public void linking_root_should_work_from_sub_folder()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetAbsoluteUrl("FolderTest", "..");

            actual.Should().Be("/");
        }

        [Fact]
        public void linking_sub_folder_from_root()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetAbsoluteUrl("", "FolderTest");

            actual.Should().Be("FolderTest");
        }

        [Fact]
        public void linking_same_level()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetAbsoluteUrl("", "page");

            actual.Should().Be("page");
        }

        [Fact]
        public void linking_document_at_parent_level()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetAbsoluteUrl("FolderTest", "../page");

            actual.Should().Be("page");
        }

        [Fact]
        public void linking_sub_image_from_root_level()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetAbsoluteUrl("", "NoDoc/image.png");

            actual.Should().Be("NoDoc/image.png");
        }

        [Fact]
        public void linking_image_from_sibling_folder()
        {


            var sut = new PageSource("/intranet/", Environment.CurrentDirectory + "\\TestDocs\\");
            var actual = sut.GetAbsoluteUrl("FolderTest", "../NoDoc/image.png");

            actual.Should().Be("NoDoc/image.png");
        }



    }
}
