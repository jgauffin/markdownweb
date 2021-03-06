﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MarkdownWeb.PreFilters
{
    /// <summary>
    ///     Allows filters to process a
    /// </summary>
    public class PreFilterCollection
    {
        private readonly List<IPreFilter> _filters = new List<IPreFilter>();

        public PreFilterCollection()
        {
            AddAllBuiltIn();
        }

        public void Add(IPreFilter filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            _filters.Add(filter);
        }

        public void Clear()
        {
            _filters.Clear();
        }

        public string Execute(IMarkdownParser parser, PageReference pageToRender, string text)
        {
            foreach (var filter in _filters)
            {
                var context = new PreFilterContext(parser)
                {
                    TextToParse = text,
                    RequestedPage = pageToRender,
                };
                text = filter.Parse(context);
            }

            return text;
        }

        public bool Remove(Type filterType)
        {
            return _filters.RemoveAll(x => x.GetType() == filterType) > 0;
        }

        private void AddAllBuiltIn()
        {
            _filters.Add(new SequenceDiagramsFilter());
            _filters.Add(new UncPathsToLinks());
        }
    }
}
