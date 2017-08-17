using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MarkdownWeb.PostFilters
{
    /// <summary>
    ///     Allows filters to process a
    /// </summary>
    public class PostFilterCollection
    {
        private readonly List<IPostFilter> _filters = new List<IPostFilter>();

        public PostFilterCollection()
        {
            AddAllBuiltIn();
        }

        public void Add(IPostFilter filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            _filters.Add(filter);
        }

        public void Clear()
        {
            _filters.Clear();
        }

        public void Execute(PostFilterContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            foreach (var filter in _filters)
            {
                context.HtmlToParse = filter.Parse(context);
            }

        }

        public bool Remove(Type filterType)
        {
            return _filters.RemoveAll(x => x.GetType() == filterType) > 0;
        }

        private void AddAllBuiltIn()
        {
            _filters.Add(new AnchorHeadings());
            _filters.Add(new TableOfContents());
        }
    }
}