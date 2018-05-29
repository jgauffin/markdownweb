using Markdig;
using MarkdownWeb.MarkdownService.Extensions;
using MarkdownWeb.Storage;

namespace MarkdownWeb.MarkdownService
{
    public class MarkdownParser
    {
        private readonly IUrlPathConverter _pathConverter;
        private readonly IPageRepository _repository;

        public MarkdownParser(IUrlPathConverter pathConverter, IPageRepository repository)
        {
            _pathConverter = pathConverter;
            _repository = repository;
            DefaultCodeLanguage = "csharp";
        }

        public string DefaultCodeLanguage { get; set; }
        public string MissingPageStyle { get; set; }

        public string Parse(MarkdownParserContext context, string markdown)
        {
            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.UseAutoLinks();
            
            builder.Extensions.AddIfNotAlready(new LinkRendererExtension(context));
            var pipeline= builder.Build();
            return Markdown.ToHtml(markdown, pipeline);
        }

    }
}
