using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownWeb.Storage;

namespace MarkdownWeb.MarkdownService
{
    public class MarkdownParserContext
    {
        public string CurrentWikiPath { get; set; }
        public IUrlPathConverter UrlPathConverter { get; set; }
        public IPageRepository PageRepository { get; set; }
    }
}
