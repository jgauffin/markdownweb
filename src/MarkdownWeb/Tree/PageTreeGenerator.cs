using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkdownWeb.Tree;

namespace MarkdownWeb
{
    public class PageTreeGenerator
    {
        public PageTreeNode Generate(IList<PageSummary> allPages, string rootUrl)
        {
            rootUrl = rootUrl.Trim('/');
            var sortedNodes = allPages
                .OrderBy(x => x, new PagePathComparer())
                .ToList();

            var pages = new LinkedList<PageTreeNode>();
            foreach (var page in sortedNodes)
            {
                var newNode = new PageTreeNode(page, null);
                pages.AddLast(newNode);
            }

            var root = pages.FirstOrDefault(x=>x.PageReference.RealWikiPath == "/" && x.PageReference.Filename == "index.md");
            if (root == null)
                root = new PageTreeNode {PageReference = new PageReference("/", "/", "index.md")};

            var node = pages.First;
            while (node != null)
            {
                // No need to try to root the root.
                // and don't add parents for nodes with parents (typically for folders that do not got any documents).
                if (node.Value == root || node.Value.Parent != null)
                {
                    node = node.Next;
                    continue;
                }
                    

                var currentPage = node.Value;
                var parent = pages.FirstOrDefault(x => x.IsParent(currentPage));

                // happens for root documents.
                if (parent == null)
                {
                    if (currentPage.PageReference.RealWikiPath != "/")
                    {
                        var parts = currentPage.PageReference.RealWikiPath.Split(new[] { '/' },
                            StringSplitOptions.RemoveEmptyEntries);

                        parent = root;

                        
                        //1 since we do not want to create the path for the given node if it's the index (i.e. root segment)
                        var partsToSkip = currentPage.PageReference.Filename == "index.md" ? 1 : 0;
                        for (int i = 0; i < parts.Length - partsToSkip; i++)
                        {
                            var path = "/" + string.Join("/", parts.Take(i + 1)) + "/";
                            var page = pages.FirstOrDefault(x => x.IsForPath(path));

                            if (page == null)
                            {
                                var summary = new PageSummary
                                {
                                    PageReference = new PageReference(path, path, ""),
                                    Title = Capitalize(parts[i]),
                                    Url = $"/{rootUrl}{path}"
                                };
                                page = new PageTreeNode(summary, parent);
                                parent.AddChild(page);
                                pages.AddLast(page);
                            }

                            parent = page;
                        }
                    }
                    else
                    {
                        parent = root;
                    }

                }


                parent.AddChild(currentPage);
                node = node.Next;
            }

            return root;
        }

        private string Capitalize(string s)
        {
            if (s.Length == 0)
                return char.ToUpper(s[0]).ToString();

            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
