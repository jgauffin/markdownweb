using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MarkdownWeb.Storage.Files;
using MarkdownWeb.Tree;
using NSubstitute;
using Xunit;

namespace MarkdownWeb.Tests.Tree
{
    public class PageTreeGeneratorTests
    {
        [Fact]
        public void ListAllPages()
        {
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");
            var pageSource = repository;
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var service = new PageService(repository, pathConverter);
            var pages = service.GetPages();

            var sut = new PageTreeGenerator();
            var root = sut.Generate(pages, "/intranet/");

            root.Children[3].PageReference.ToString().Should().Be("/page/");
        }

        [Fact]
        public void Should_be_possible_to_build_a_HTML_tree()
        {
            var pageSource = Substitute.For<IPageSource>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var pathConverter = new UrlConverter("/intranet/", pageSource);
            var repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");
            var service = new PageService(repository, pathConverter);
            var pages = service.GetPages();

            var sb = new StringBuilder();
            sb.AppendLine("<ul>");
            var sut = new PageTreeGenerator();
            var root = sut.Generate(pages, "/intranet/");
            root.Visit((node, state) =>
            {
                if (node == root)
                    return;

                switch (state)
                {
                    case PageTreeNodeVisitorState.SingleNode:
                        sb.AppendLine("<li>" + node.Title + "</li>");
                        break;
                    case PageTreeNodeVisitorState.StartNodeWithChildren:
                        sb.AppendLine("<li>" + node.Title);
                        sb.AppendLine("<ul>");
                        break;
                    case PageTreeNodeVisitorState.EndNodeWithChildren:
                        sb.AppendLine("</ul></li>");
                        break;
                }
            });
            sb.AppendLine("</ul>");

            var actual = sb.ToString();
        }
    }
}
