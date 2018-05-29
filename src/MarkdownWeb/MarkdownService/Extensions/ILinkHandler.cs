using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using MarkdownWeb.MarkdownService.Extensions.Links;

namespace MarkdownWeb.MarkdownService.Extensions
{
    public interface ILinkHandler
    {
        bool CanHandle(LinkHandlerContext context);
        void Process(LinkHandlerContext context);
    }
}