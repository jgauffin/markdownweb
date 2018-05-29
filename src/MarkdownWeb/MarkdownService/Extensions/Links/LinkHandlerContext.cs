using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace MarkdownWeb.MarkdownService.Extensions.Links
{
    public class LinkHandlerContext
    {
        public LinkHandlerContext(MarkdownParserContext parserContext, HtmlRenderer htmlRenderer, LinkInline link)
        {
            ParserContext = parserContext;
            HtmlRenderer = htmlRenderer;
            Link = link;
        }

        public MarkdownParserContext ParserContext { get; private set; }
        public HtmlRenderer HtmlRenderer { get; private set; }
        public LinkInline Link { get; private set; }
    }
}