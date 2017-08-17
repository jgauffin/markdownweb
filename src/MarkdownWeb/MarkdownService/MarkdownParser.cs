using Markdig;
using MarkdownWeb.MarkdownService.Extensions;
using MarkdownWeb.Storage;

namespace MarkdownWeb.MarkdownService
{
    public class MarkdownParser
    {
        private readonly IUrlPathConverter _pathConverter;
        private readonly IPageRepository _repository;
        private string _currentWikiPath;

        public MarkdownParser(IUrlPathConverter pathConverter, IPageRepository repository)
        {
            _pathConverter = pathConverter;
            _repository = repository;
            DefaultCodeLanguage = "csharp";
        }

        public string DefaultCodeLanguage { get; set; }
        public string MissingPageStyle { get; set; }

        public string Parse(string currentWikiPath, string markdownDocument)
        {
            _currentWikiPath = currentWikiPath;
            var context = new MarkdownParserContext
            {
                CurrentWikiPath = currentWikiPath,
                UrlPathConverter = _pathConverter,
                PageRepository = _repository
            };
            var builder = new MarkdownPipelineBuilder();
            builder.UseAdvancedExtensions();
            builder.Extensions.AddIfNotAlready(new WikiLinkExtension(context));
            var pipeline= builder.Build();
            return Markdown.ToHtml(markdownDocument, pipeline);
        }

    }
}
