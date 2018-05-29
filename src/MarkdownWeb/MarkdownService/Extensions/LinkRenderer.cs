using System;
using System.Collections.Generic;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;
using MarkdownWeb.MarkdownService.Extensions.Links;

namespace MarkdownWeb.MarkdownService.Extensions
{
    /// <summary>
    /// Used to render real anchor links from wiki syntax. Must transform wiki relative URLs to absolute web site URLs.
    /// </summary>
    public class LinkRenderer : HtmlObjectRenderer<LinkInline>
    {
        private readonly LinkInlineRenderer _orgRenderer;
        private readonly MarkdownParserContext _context;
        List<ILinkHandler> _linkHandlers = new List<ILinkHandler>();

        /// <summary>
        /// Creates a new instance of <see cref="LinkRenderer"/>.
        /// </summary>
        /// <param name="orgRenderer">Original renderer in the MarkdownDig package.</param>
        /// <param name="context">parser context</param>
        public LinkRenderer(LinkInlineRenderer orgRenderer, MarkdownParserContext context)
        {
            _orgRenderer = orgRenderer ?? throw new ArgumentNullException(nameof(orgRenderer));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _linkHandlers.Add(new WikiLinkHandler());
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
                _context.AddLink(new PageLink(link.Url, link.Label) { IsLocal = true, IsWikiLink = false });
                _orgRenderer.Write(renderer, link);
                return;
            }

            var ctx = new LinkHandlerContext(_context, renderer, link);
            foreach (var linkHandler in _linkHandlers)
            {
                if (linkHandler.CanHandle(ctx))
                {
                    linkHandler.Process(ctx);
                    return;
                }
            }


            _context.AddLink(new PageLink(link.Url, link.Label) { IsLocal = link.Url.StartsWith("/") });
            _orgRenderer.Write(renderer, link);
        }


    }
}