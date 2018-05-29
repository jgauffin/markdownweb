using System;
using System.Collections.Generic;
using System.IO;

namespace MarkdownWeb.Tree
{
    public class PageTreeNode
    {
        private readonly List<PageTreeNode> _children = new List<PageTreeNode>();

        public PageTreeNode()
        {
            Title = "Root";
        }

        public PageTreeNode(PageSummary summary, PageTreeNode parent)
        {
            Parent = parent;
            PageReference = summary.PageReference;
            Title = summary.Title;
            Abstract = summary.Abstract;
            Url = summary.Url;
        }

        public string Abstract { get; set; }

        public IReadOnlyList<PageTreeNode> Children => _children;


        public PageReference PageReference { get; set; }

        public PageTreeNode Parent { get; private set; }
        public string Title { get; set; }
        public string Url { get; set; }

        public void AddChild(PageTreeNode page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            _children.Add(page);
            page.Parent = this;
        }

        public bool IsParent(PageTreeNode nodeToCheck)
        {
            // Cannot be parent if we are not index.
            if (PageReference.Filename != "index.md")
                return false;

            // other node is index, so we can be parent if they are one level below us.
            if (nodeToCheck.PageReference.Filename == "index.md")
            {
                var nodeParts =
                    nodeToCheck.PageReference.RealWikiPath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                var ourParts = PageReference.RealWikiPath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                if (ourParts.Length != nodeParts.Length - 1)
                    return false;

                for (var i = 0; i < ourParts.Length; i++)
                    if (ourParts[i] != nodeParts[i])
                        return false;

                return true;
            }

            // Child is not index.md, it is only a child to index.md on the same level.
            return PageReference.RealWikiPath == nodeToCheck.PageReference.RealWikiPath;
        }

        public override string ToString()
        {
            return $"[{PageReference}] {Title}";
        }

        public void Visit(Action<PageTreeNode, PageTreeNodeVisitorState> visitor)
        {
            if (_children.Count == 0)
            {
                visitor(this, PageTreeNodeVisitorState.SingleNode);
                return;
            }

            visitor(this, PageTreeNodeVisitorState.StartNodeWithChildren);
            foreach (var child in Children)
            {
                child.Visit(visitor);
            }
            visitor(this, PageTreeNodeVisitorState.EndNodeWithChildren);
        }

        public bool IsForPath(string path)
        {
            return PageReference.RealWikiPath == path &&
                   (PageReference.Filename == "index.md" || PageReference.Filename == "");
        }
    }
}