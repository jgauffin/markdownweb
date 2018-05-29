using System;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;

namespace MarkdownWeb.MarkdownService.Extensions
{
    
    internal class LinkRendererExtension : IMarkdownExtension
    {
        private readonly MarkdownParserContext _context;

        public LinkRendererExtension(MarkdownParserContext context)
        {
            _context = context;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            var htmlRenderer = renderer as TextRendererBase<HtmlRenderer>;
            if (htmlRenderer == null)
                return;

            var orgRenderer = htmlRenderer.ObjectRenderers.FindExact<LinkInlineRenderer>();
            if (orgRenderer != null)
                htmlRenderer.ObjectRenderers.Remove(orgRenderer);

            htmlRenderer.ObjectRenderers.AddIfNotAlready(new LinkRenderer(orgRenderer, _context));
        }
    }
}