using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using CsQuery.Engine.PseudoClassSelectors;
using MarkdownWeb;
using MarkdownWeb.Tree;

namespace GitAspNetMvc.Models
{
    public class PageListViewModel
    {
        private readonly PageTreeNode _root;

        public PageListViewModel(PageTreeNode root)
        {
            _root = root;
        }
        public void Write(TextWriter writer, string ulClass)
        {
            writer.WriteLine($"<ul class=\"{ulClass}\">");
           _root.Visit((node, state) =>
           {
               if (node ==_root)
                   return;

               switch (state)
               {
                   case PageTreeNodeVisitorState.SingleNode:
                       writer.WriteLine($"<li><a href=\"{node.Url}\">{node.Title}</a></li>");
                       break;
                   case PageTreeNodeVisitorState.StartNodeWithChildren:
                       writer.WriteLine($"<li><a href=\"{node.Url}\">{node.Title}</a>");
                       writer.WriteLine("<ul>");
                       break;
                   case PageTreeNodeVisitorState.EndNodeWithChildren:
                       writer.WriteLine("</ul></li>");
                       break;
               }
           });

            writer.WriteLine("</ul>");
        }

    }
}