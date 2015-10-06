using MarkdownDeep;
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
            var markdown = new Markdown
            {
                PrepareLink = OnHtmlLink,
                PrepareImage = OnImageLink,
                FormatCodeBlock = OnCodeBlock
            };
            return markdown.Transform(markdownDocument);

        }


        private string OnCodeBlock(Markdown arg1, string arg2)
        {
            return string.Format(
                @"<pre class=""language-{0}""><code data-lang=""{0}"" class=""language-{0}"">{1}</code></pre>",
                DefaultCodeLanguage,
                arg2);
        }

        private bool OnHtmlLink(HtmlTag arg)
        {
            var src = arg.attributes["href"];
            if (src.StartsWith("http"))
                return false;
            if (src.StartsWith("~"))
            {
                src = _pathConverter.ToAbsolutePath(src);
                arg.attributes["href"] = src;
                return true;
            }


            //if (!src.StartsWith("/") && !src.StartsWith("#"))
            //{
            //    src = _pathConverter.MapWikiPaths(_currentWikiPath, src);
            //    arg.attributes["href"] = src;
            //}

            //make a wiki path to be able to check settings
            var wikiPath = _pathConverter.MapWikiPaths(_currentWikiPath, src);
            var exists = _repository.Exists(wikiPath) || src.StartsWith("#");
            if (!exists)
            {
                arg.attributes["style"] = MissingPageStyle;
                var clazz = "";
                if (arg.attributes.TryGetValue("class", out clazz))
                    clazz += " missing-page";
                else
                    clazz = "missing-page";
                arg.attributes["class"] = clazz;
            }

            //now make it clickable.
            arg.attributes["href"] = _pathConverter.ToWebPath(wikiPath);
            return true;
        }

        private bool OnImageLink(HtmlTag arg1, bool arg2)
        {
            var src = arg1.attributes["src"];
            if (src.StartsWith("http"))
                return false;

            if (src.StartsWith("~"))
            {
                src= _pathConverter.ToAbsolutePath(src);
            }
            else
            src = _pathConverter.MapWikiPaths(_currentWikiPath, src);

            var url = _pathConverter.ToWebPath("/") + "?image=" + src;
            arg1.attributes["src"] = url;
            return true;
        }

    }
}
