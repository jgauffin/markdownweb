using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkdownWeb
{
    public class PageReferenceWithChildren : PageReference
    {
        private List<PageReferenceWithChildren> _children = new List<PageReferenceWithChildren>();
        public IReadOnlyList<PageReferenceWithChildren> Children => _children;

        public PageReferenceWithChildren(string friendlyUrl, string wikiUrl) : base(friendlyUrl, wikiUrl)
        {
        }

        public bool HasChildDocuments
        {
            get
            {
                return Children.Any(x => x.HasChildDocuments) || Children.Any(x => x.WikiUrl.Contains(".md"));
            }
        }

        public void AddChild(PageReference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            _children.Add(new PageReferenceWithChildren(reference.FriendlyWikiUrl, reference.WikiUrl));
        }

        public void AddChild(PageReferenceWithChildren reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            _children.Add(reference);
        }
    }
}