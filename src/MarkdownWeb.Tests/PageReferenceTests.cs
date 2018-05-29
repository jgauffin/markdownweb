using FluentAssertions;
using Xunit;

namespace MarkdownWeb.Tests
{
    public class PageReferenceTests
    {
        
        [Fact]
        public void page_links_should_work_when_both_are_on_start_level()
        {

            var sut = new PageReference("/withlink", "/", "withlink.md");
            var actual = sut.MapReferencedDocument("page");

            actual.Should().Be("/page");
        }
         
        [Fact]
        public void page_links_should_work_linking_one_level_up()
        {

            var sut = new PageReference("/withlink/", "/withlink/", "index.md");
            var actual = sut.MapReferencedDocument("../page");

            actual.Should().Be("/page");
        }

        [Fact]
        public void page_links_when_relative_to_same_level()
        {

            var sut = new PageReference("/withlink", "/withlink", "index.md");
            var actual = sut.MapReferencedDocument("more/page");

            actual.Should().Be("/withlink/more/page");
        }

        [Fact]
        public void page_links_when_linking_deeper()
        {

            var sut = new PageReference("/withlink/", "/withlink/", "index.md");
            var actual = sut.MapReferencedDocument("../more/page");

            actual.Should().Be("/more/page");
        }

        

        [Fact]
        public void linking_root_should_work_from_sub_folder()
        {

            var sut = new PageReference("/FolderTest/", "/FolderTest/", "index.md");
            var actual = sut.MapReferencedDocument( "..");

            actual.Should().Be("/");
        }

        [Fact]
        public void linking_sub_folder_from_root()
        {
            
            var sut = new PageReference("/", "/", "index.md");
            var actual = sut.MapReferencedDocument( "FolderTest");

            actual.Should().Be("/FolderTest");
        }

        [Fact]
        public void linking_sub_folder_from_virtual_path()
        {
            
            var sut = new PageReference("/document/", "/", "document.md");
            var actual = sut.MapReferencedDocument( "FolderTest");

            actual.Should().Be("/FolderTest");
        }


        [Fact]
        public void linking_image_from_sibling_folder()
        {

            var sut = new PageReference("/FolderTest", "/FolderTest/","index.md");
            var actual = sut.MapReferencedDocument( "../NoDoc/image.png");

            actual.Should().Be("/NoDoc/image.png");
        }

        [Fact]
        public void linking_image_from_same_level_should_make_it_an_absolute_path()
        {
           
            var sut = new PageReference("withlink", "/", "withlink.md");
            var actual = sut.MapReferencedDocument( "NoDoc/image.png");
        }


    }
}
