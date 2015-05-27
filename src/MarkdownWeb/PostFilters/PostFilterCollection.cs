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

        public string Execute(string html)
        {
            foreach (var filter in _filters)
            {
                var context = new PostFilterContext();
                context.HtmlToParse = html;
                html = filter.Parse(context);
            }

            return html;
        }

        public bool Remove(Type filterType)
        {
            return _filters.RemoveAll(x => x.GetType() == filterType) > 0;
        }

        private void AddAllBuiltIn()
        {
            foreach (
                var type in
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(x => typeof (IPostFilter).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface))
            {
                _filters.Add((IPostFilter) Activator.CreateInstance(type));
            }
        }
    }
}