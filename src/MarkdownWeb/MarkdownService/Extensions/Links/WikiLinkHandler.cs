using Markdig.Syntax.Inlines;
using MarkdownWeb.MarkdownService.Extensions.Links;

namespace MarkdownWeb.MarkdownService.Extensions
{
    public class WikiLinkHandler : ILinkHandler
    {
        public bool CanHandle(LinkHandlerContext context)
        {
            return IsWikiPage(context.Link);
        }

        public void Process(LinkHandlerContext context)
        {
            var link = context.Link;
            var renderer = context.HtmlRenderer;

            var wikiPath = context.ParserContext.RequestedPage.MapReferencedDocument(link.Url);
            var pageReference = context.ParserContext.UrlPathConverter.ToReference(wikiPath);
            bool pageIsMissing = pageReference == null;

            if (renderer.EnableHtmlForInline)
            {
                renderer.Write(link.IsImage ? "<img src=\"" : "<a href=\"");

                if (pageIsMissing)
                {
                    link.Url = context.ParserContext.UrlPathConverter.ToWebUrl(wikiPath);
                }
                else if (link.IsImage)
                {
                    link.Url = context.ParserContext.UrlPathConverter.ToWebUrl("/") + "?image=" + pageReference;
                }
                else
                {
                    link.Url = context.ParserContext.UrlPathConverter.ToWebUrl(pageReference.ToWikiPath());
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

                context.ParserContext.AddLink(new PageLink(link.Url, link.Title)
                {
                    IsMissing = pageIsMissing,
                    IsWikiLink = true,
                    IsLocal = true
                });
            }
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