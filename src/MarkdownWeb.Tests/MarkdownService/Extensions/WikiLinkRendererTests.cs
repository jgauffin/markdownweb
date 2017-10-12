using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Markdig;
using MarkdownWeb.MarkdownService;
using MarkdownWeb.MarkdownService.Extensions;
using MarkdownWeb.Storage;
using MarkdownWeb.Storage.Files;
using NSubstitute;
using Xunit;
using MarkdownParser = Markdig.Parsers.MarkdownParser;

namespace MarkdownWeb.Tests.MarkdownService.Extensions
{
    public class WikiLinkRendererTests
    {
        [Fact]
        public void ignore_bbcode_or_language_tags_for_syntax_highlighting()
        {
            var text = "some text [codeblock] flflf";
            var context = new MarkdownParserContext
            {
                CurrentWikiPath = "/client/libraries/aspnet/",
                UrlPathConverter = new UrlConverter("/documentation/"),
                PageRepository = Substitute.For<IPageRepository>()
            };


            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new WikiLinkExtension(context));
            var pipeline = builder.Build();
            var actual= Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(" [codeblock] ");
        }

        [Fact]
        public void ignore_end_block_for_bbcode_and_language_tags_for_syntax_highlighting()
        {
            var text = "some text [/codeblock] flflf";
            var context = new MarkdownParserContext
            {
                CurrentWikiPath = "/client/libraries/aspnet/",
                UrlPathConverter = new UrlConverter("/documentation/"),
                PageRepository = Substitute.For<IPageRepository>()
            };


            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new WikiLinkExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(" [/codeblock] ");
        }

        [Fact]
        public void show_work_for_image_links()
        {
            var text = "some text ![majs](https://book/image.png) flflf";
            var context = new MarkdownParserContext
            {
                CurrentWikiPath = "/client/libraries/aspnet/",
                UrlPathConverter = new UrlConverter("/documentation/"),
                PageRepository = Substitute.For<IPageRepository>()
            };


            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new WikiLinkExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <img src=""https://book/image.png"" alt=""majs"" /> ");
        }

        [Fact]
        public void show_work_for_normal_links()
        {
            var text = "some text [majs](https://book/image.png) flflf";
            var context = new MarkdownParserContext
            {
                CurrentWikiPath = "/client/libraries/aspnet/",
                UrlPathConverter = new UrlConverter("/documentation/"),
                PageRepository = Substitute.For<IPageRepository>()
            };


            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new WikiLinkExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <a href=""https://book/image.png"">majs</a> ");
        }

        [Fact]
        public void should_work_for_wiki_links()
        {
            var text = "some text [majs](../image) flflf";
            var repos = Substitute.For<IPageRepository>();
            var context = new MarkdownParserContext
            {
                CurrentWikiPath = "/client/libraries/aspnet/",
                UrlPathConverter = new UrlConverter("/documentation/"),
                PageRepository = repos
            };
            repos.Exists(Arg.Any<string>()).Returns(true);

            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new WikiLinkExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <a href=""/documentation/client/libraries/image"">majs</a> ");
        }

        [Fact]
        public void show_work_for_missing_wiki_links()
        {
            var text = "some text [majs](../page) flflf";
            var context = new MarkdownParserContext
            {
                CurrentWikiPath = "/client/libraries/aspnet/",
                UrlPathConverter = new UrlConverter("/documentation/"),
                PageRepository = Substitute.For<IPageRepository>()
            };


            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new WikiLinkExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <a href=""/documentation/client/libraries/page"" class=""missing-page"">majs</a> ");
        }

        [Fact]
        public void show_work_for_missing_wiki_links_that_have_a_page_name()
        {
            var text = "some text [majs](anotherpage.md) flflf";
            var context = new MarkdownParserContext
            {
                CurrentWikiPath = "/client/libraries/aspnet/",
                UrlPathConverter = new UrlConverter("/documentation/"),
                PageRepository = Substitute.For<IPageRepository>()
            };


            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new WikiLinkExtension(context));
            var pipeline = builder.Build();
            var actual = Markdown.ToHtml(text, pipeline);

            actual.Should().Contain(@" <a href=""/documentation/client/libraries/aspnet/anotherpage.md"" class=""missing-page"">majs</a> ");
        }
    }
}
