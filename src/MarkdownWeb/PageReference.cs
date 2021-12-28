using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AngleSharp.Io;

namespace MarkdownWeb
{
    /// <summary>
    ///     A page reference
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         As we can use different types of web paths to address a document we need to parse that path to be able to
    ///     </para>
    /// </remarks>
    public class PageReference
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="friendlyUrl">Path that can be used in links.</param>
        /// <param name="wikiUrl">Path used to access the document from the repository.</param>
        public PageReference(string friendlyUrl, string wikiUrl)
        {
            if (wikiUrl == "//")
            {
                Debugger.Break();
            }

            WikiUrl = wikiUrl ?? throw new ArgumentNullException(nameof(wikiUrl));
            if (!wikiUrl.Contains("."))
                throw new ArgumentException("Real path must contain a file name.");

            FriendlyWikiUrl = friendlyUrl ?? throw new ArgumentNullException(nameof(friendlyUrl));
            if (!FriendlyWikiUrl.EndsWith("/") && !FriendlyWikiUrl.Contains("."))
            {
                FriendlyWikiUrl += "/";
            }
        }

        /// <summary>
        ///     A friendly relative url (from the wiki root) to this document.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Can be <c>/users/delete/</c> while the real with path is <c>/users/delete.md</c>. We need to store both to be
        ///         able to adjust all wiki links and image urls
        ///     </para>
        ///     <para>Used to find the URL given by the user.</para>
        /// </remarks>
        public string FriendlyWikiUrl { get; set; }

        public bool IsIndex => WikiUrl.EndsWith(IndexFile);

        /// <summary>
        ///     This represents a (real) directory. May or may not have an "index.md".
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Directories without an index document should generate a page index for all child pages.
        ///     </para>
        /// </remarks>
        public bool IsDirectory { get; set; }

        /// <summary>
        ///     Path to the document (including filename for non index.md). Always a trailing slash
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Used to load a document from disk (and to match the url given by the user).
        ///     </para>
        /// </remarks>
        public string WikiUrl { get; private set; }

        /// <summary>
        ///     Take an URL specified in a wiki document and convert the relative path to an absolute wiki path.
        /// </summary>
        /// <param name="referencedDocument">Document linked from our document.</param>
        /// <returns></returns>
        public string MapReferencedDocument(string referencedDocument)
        {
            if (referencedDocument == null)
            {
                throw new ArgumentNullException(nameof(referencedDocument));
            }

            if (referencedDocument.StartsWith("/"))
            {
                return referencedDocument;
            }

            if (referencedDocument.StartsWith("~/"))
            {
                return referencedDocument;
            }

            // Remove the document as we traverse folder parts
            var pos = WikiUrl.LastIndexOf('/');
            var myUrl = WikiUrl.Substring(0, pos);

            var myParts = myUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var referencedParts = referencedDocument.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            while (referencedParts.Count > 0 && referencedParts[0] == ".." && myParts.Count > 0)
            {
                myParts.RemoveAt(myParts.Count - 1);
                referencedParts.RemoveAt(0);
            }

            myParts.AddRange(referencedParts);

            var myPath = string.Join("/", myParts);
            var path = myPath.StartsWith("/") ? myPath : "/" + myPath;
            return path;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return FriendlyWikiUrl;
        }

        public const string IndexFile = "index.md";
    }
}