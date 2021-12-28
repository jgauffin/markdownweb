using System;
using System.Collections.Generic;
using FluentAssertions;
using Markdig;
using MarkdownWeb.MarkdownService;
using MarkdownWeb.MarkdownService.Extensions;
using MarkdownWeb.Storage;
using MarkdownWeb.Storage.Files;
using NSubstitute;
using Xunit;
using MarkdownParserContext = MarkdownWeb.MarkdownService.MarkdownParserContext;

namespace MarkdownWeb.Tests.MarkdownService.Extensions
{
    public class WikiLinkRendererTests
    {
        private FileBasedRepository _repository;

        public WikiLinkRendererTests()
        {
            _repository = new FileBasedRepository(Environment.CurrentDirectory + "\\TestDocs\\");
        }

        [Fact]
        public void ignore_bbCode_or_language_tags_for_syntax_highlighting()
        {
            var text = "some text [codeblock] flflf";
            var pageSource = Substitute.For<IPageSource>();
            var links = new List<PageLink>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var context = new MarkdownParserContext(Substitute.For<IPageRepository>(), links)
            {
                RequestedPage = new PageReference("/client/libraries/aspnet/", "/client/libraries/aspnet/index.md"),
                UrlPathConverter = new UrlConverter("/documentation/", pageSource),
            };


            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new LinkRendererExtension(context));
            var pipeline = builder.Build();
            var actual= Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(" [codeblock] ");
        }

        [Fact]
        public void ignore_end_block_for_bbcode_and_language_tags_for_syntax_highlighting()
        {
            var text = "some text [/codeblock] flflf";
            var pageSource = Substitute.For<IPageSource>();
            var links = new List<PageLink>();
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            var context = new MarkdownParserContext(Substitute.For<IPageRepository>(), links)
            {
                RequestedPage = new PageReference("/client/libraries/aspnet/", "/client/libraries/aspnet/index.md"),
                UrlPathConverter = new UrlConverter("/documentation/", pageSource),
                
            };


            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new LinkRendererExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(" [/codeblock] ");
        }

        [Fact]
        public void show_work_for_image_links()
        {
            var pageSource = Substitute.For<IPageSource>();
            var links = new List<PageLink>();
            var text = "some text ![majs](https://book/image.png) flflf";
            var context = new MarkdownParserContext(Substitute.For<IPageRepository>(), links)
            {
                RequestedPage = new PageReference("/client/libraries/aspnet/", "/client/libraries/aspnet/index.md"),
                UrlPathConverter = new UrlConverter("/documentation/", pageSource),
                
            };
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);


            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new LinkRendererExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <img src=""https://book/image.png"" alt=""majs"" /> ");
        }

        [Fact]
        public void Should_handle_absolute_path_image_links()
        {
            var pageSource = Substitute.For<IPageSource>();
            var links = new List<PageLink>();
            var text = "some text ![majs](/book/image.png) flflf";
            var repos = Substitute.For<IPageRepository>();
            pageSource.PageExists(Arg.Is<PageReference>(x => x.WikiUrl == "/book/image.png")).Returns(true);
            var context = new MarkdownParserContext(repos, links)
            {
                RequestedPage = new PageReference("/client/libraries/aspnet/", "/client/libraries/aspnet/index.md"),
                UrlPathConverter = new UrlConverter("/documentation/", pageSource),

            };

            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new LinkRendererExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <img src=""/documentation/?image=/book/image.png"" alt=""majs"" /> ");
        }

        [Fact]
        public void show_work_for_normal_links()
        {
            var pageSource = Substitute.For<IPageSource>();
            var links = new List<PageLink>();
            var text = "some text [majs](https://book/image.png) flflf";
            var context = new MarkdownParserContext(Substitute.For<IPageRepository>(), links)
            {
                RequestedPage = new PageReference("/client/libraries/aspnet/", "/client/libraries/aspnet/index.md"),
                UrlPathConverter = new UrlConverter("/documentation/", pageSource),
                
            };
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);


            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new LinkRendererExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <a href=""https://book/image.png"">majs</a> ");
        }

        [Fact]
        public void should_work_for_wiki_links()
        {
            var pageSource = Substitute.For<IPageSource>();
            var links = new List<PageLink>();
            var text = "some text [majs](../image) flflf";
            var repos = Substitute.For<IPageRepository>();
            var context = new MarkdownParserContext(repos, links)
            {
                RequestedPage = new PageReference("/client/libraries/aspnet/", "/client/libraries/aspnet/index.md"),
                UrlPathConverter = new UrlConverter("/documentation/", pageSource),
            };
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);
            repos.Exists(Arg.Any<PageReference>()).Returns(true);

            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new LinkRendererExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <a href=""/documentation/client/libraries/image/"">majs</a> ");
        }

        [Fact]
        public void should_work_for_wiki_links_when_using_virtual_path()
        {
            var pageSource = Substitute.For<IPageSource>();
            var links = new List<PageLink>();
            var text = "some text [majs](../image) flflf";
            var repos = Substitute.For<IPageRepository>();
            var context = new MarkdownParserContext(repos, links)
            {
                RequestedPage = new PageReference("/client/libraries/aspnet/", "/client/libraries/aspnet.md"),
                UrlPathConverter = new UrlConverter("/documentation/", pageSource),
            };
            repos.Exists(Arg.Any<PageReference>()).Returns(true);
            pageSource.PageExists(Arg.Any<PageReference>()).Returns(true);

            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new LinkRendererExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <a href=""/documentation/client/image/"">majs</a> ");
        }

        [Fact]
        public void show_work_for_missing_wiki_links()
        {
            var links = new List<PageLink>();
            var text = "some text [majs](../page) flflf";
            var context = new MarkdownParserContext(Substitute.For<IPageRepository>(), links)
            {
                RequestedPage = new PageReference("/client/libraries/aspnet/", "/client/libraries/aspnet/index.md"),
                UrlPathConverter = new UrlConverter("/documentation/", _repository),
                
            };

            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new LinkRendererExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <a href=""/documentation/client/libraries/page"" class=""missing-page"">majs</a> ");
        }

        [Fact]
        public void show_work_for_missing_wiki_links_that_have_a_page_name()
        {
            var repos = Substitute.For<IPageRepository>();
            var links = new List<PageLink>();
            var text = "some text [majs](anotherpage.md) flflf";
            var context = new MarkdownParserContext(repos, links)
            {
                RequestedPage = new PageReference("/client/libraries/aspnet/", "/client/libraries/aspnet/index.md"),
                UrlPathConverter = new UrlConverter("/documentation/", _repository),
                
            };
            repos.Exists(Arg.Any<PageReference>()).Returns(false);

            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new LinkRendererExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <a href=""/documentation/client/libraries/aspnet/anotherpage.md"" class=""missing-page"">majs</a> ");
        }
    }
}
