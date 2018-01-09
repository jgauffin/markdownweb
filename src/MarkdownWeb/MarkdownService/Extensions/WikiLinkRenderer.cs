﻿using System;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace MarkdownWeb.MarkdownService.Extensions
{
    /// <summary>
    /// Used to render real anchor links from wiki syntax. Must transform wiki relative URLs to absolute web site URLs.
    /// </summary>
    public class WikiLinkRenderer : HtmlObjectRenderer<LinkInline>
    {
        private readonly LinkInlineRenderer _orgRenderer;
        private readonly MarkdownParserContext _context;

        /// <summary>
        /// Creates a new instance of <see cref="WikiLinkRenderer"/>.
        /// </summary>
        /// <param name="orgRenderer">Original renderer in the MarkdownDig package.</param>
        /// <param name="context">parser context</param>
        public WikiLinkRenderer(LinkInlineRenderer orgRenderer, MarkdownParserContext context)
        {
            _orgRenderer = orgRenderer ?? throw new ArgumentNullException(nameof(orgRenderer));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        protected override void Write(HtmlRenderer renderer, LinkInline link)
        {
            // just a [block]
            if (link.Url == null)
            {
                renderer.Write($"[{link.Title}]");
                return;
            }

            if (link.Url.StartsWith("~"))
            {
                link.Url = _context.UrlPathConverter.ToAbsolutePath(link.Url);
                _orgRenderer.Write(renderer, link);
                return;
            }

            // relative page
            if (!IsWikiPage(link))
            {
                _orgRenderer.Write(renderer, link);
                return;
            }

            if (link.IsImage && !IsWikiPage(link))
            {
                _orgRenderer.Write(renderer, link);
                return;
            }

            link.Url = _context.UrlPathConverter.MapWikiPaths(_context.CurrentWikiPath, link.Url);
            bool pageIsMissing = IsPageMissing(link.Url) && !link.IsImage;

            if (renderer.EnableHtmlForInline)
            {
                renderer.Write(link.IsImage ? "<img src=\"" : "<a href=\"");

                if (link.IsImage)
                {
                    link.Url = _context.UrlPathConverter.ToWebPath("/") + "?image=" + link.Url;
                }
                else
                {
                    link.Url = _context.UrlPathConverter.ToWebPath(link.Url);
                }

                renderer.WriteEscapeUrl(link.GetDynamicUrl != null ? link.GetDynamicUrl() ?? link.Url : link.Url);
                renderer.Write("\"");
                renderer.WriteAttributes(link);
            }
            if (link.IsImage)
            {
                if (renderer.EnableHtmlForInline)
                    renderer.Write(" alt=\"");
                var wasEnableHtmlForInline = renderer.EnableHtmlForInline;
                renderer.EnableHtmlForInline = false;
                renderer.WriteChildren(link);
                renderer.EnableHtmlForInline = wasEnableHtmlForInline;
                if (renderer.EnableHtmlForInline)
                    renderer.Write("\"");
            }
            else if (pageIsMissing)
                renderer.Write(" class=\"missing-page\"");


            if (renderer.EnableHtmlForInline && !string.IsNullOrEmpty(link.Title))
            {
                renderer.Write(" title=\"");
                renderer.WriteEscape(link.Title);
                renderer.Write("\"");
            }


            if (link.IsImage)
            {
                if (renderer.EnableHtmlForInline)
                    renderer.Write(" />");
            }
            else
            {
                if (renderer.EnableHtmlForInline)
                    renderer.Write(">");
                renderer.WriteChildren(link);
                if (renderer.EnableHtmlForInline)
                    renderer.Write("</a>");
            }
        }

        private bool IsPageMissing(string linkUrl)
        {
            return !_context.PageRepository.Exists(linkUrl);
        }

        private static bool IsWikiPage(LinkInline obj)
        {
            if (obj.Url.StartsWith("~"))
            {
                return true;
            }

            if (obj.Url.StartsWith("/") || obj.Url.StartsWith("../"))
                return true;

            // page at same level should not contain ":"
            return !obj.Url.Contains(":");
        }
    }
}