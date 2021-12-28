using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace MarkdownWeb.Tree
{
    /// <summary>
    /// Used when generating a tree index for the wiki.
    /// </summary>
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
            if (nodeToCheck == this)
            {
                return false;
            }

            var pos = nodeToCheck.PageReference.WikiUrl.LastIndexOf('/');
            var otherPath = nodeToCheck.PageReference.WikiUrl.Substring(0, pos);

            pos = PageReference.WikiUrl.LastIndexOf('/');
            var ourPath = PageReference.WikiUrl.Substring(0, pos);

            if (!nodeToCheck.PageReference.IsIndex)
            {
                // documents
                return ourPath == otherPath;
            }
            
            // Directories
            if (!otherPath.StartsWith(ourPath))
                return false;

            // check that it's exactly the next depth.
            var ourCount = ourPath.Count(x => x == '/');
            var otherCount = otherPath.Count(x => x == '/');
            return otherCount == ourCount + 1;
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
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return PageReference.WikiUrl.Equals(path) || PageReference.FriendlyWikiUrl.TrimEnd('/').Equals(path.TrimEnd('/'));
        }
    }
}