using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MarkdownWeb.Storage.Files;
using Xunit;

namespace MarkdownWeb.Tests.Files
{
    public class FileBasedRepositoryTests
    {
        [Fact]
        public void Test()
        {
            var sut = new FileBasedRepository($"{Environment.CurrentDirectory}\\TestDocs\\");

            var pages = sut.GetAllPagesAsLinks().ToList();

            pages.Should().Contain("/withlink.md");
            pages.Should().Contain("/FolderTest/subfolder/twodoc.md");
        }


        [Fact]
        public void Test2()
        {
            var sut = new FileBasedRepository($"{Environment.CurrentDirectory}\\TestDocs\\");
            
            var pages = sut.GetAllPages("").ToList();

            pages.Should().Contain(x => x.WikiUrl == "/withlink.md");
            //pages.Should().Contain("/FolderTest/subfolder/twodoc.md");
        }
        
    }
}
