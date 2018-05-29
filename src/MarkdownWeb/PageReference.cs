using System;
using System.Diagnostics;
using System.Linq;

namespace MarkdownWeb
{
    /// <summary>
    /// A page reference
    /// </summary>
    /// <remarks>
    ///<para>
    ///As we can use different types of web paths to address a document we need to parse that path to be able to 
    /// </para>
    /// </remarks>
    public class PageReference
    {
        public PageReference(string givenWikiPath, string realWikiPath, string fileName)
        {
            if (realWikiPath == "//")
                Debugger.Break();

            GivenWikiPath = givenWikiPath ?? throw new ArgumentNullException(nameof(givenWikiPath));
            RealWikiPath = realWikiPath ?? throw new ArgumentNullException(nameof(realWikiPath));
            Filename = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        public string Filename { get; set; }

        /// <summary>
        /// Path to the document
        /// </summary>
        public string RealWikiPath { get; private set; }

        /// <summary>
        /// Path given in the web browser
        /// </summary>
        /// <remarks>
        /// <para>Can be <c>/users/delete/</c> while the real with path is <c>/users/delete.md</c>. We need to store both to be able to adjust all wiki links and image urls </para>
        /// </remarks>
        public string GivenWikiPath { get; set; }

        public override string ToString()
        {
            return $"{RealWikiPath}{Filename}";
        }

        public string MapReferencedDocument(string wikiPath)
        {
            var myParts = GivenWikiPath.Split(new []{'/'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            var referencedParts = wikiPath.Split(new []{'/'}, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (GivenWikiPath.Length > RealWikiPath.Length)
                referencedParts.Insert(0, "..");

            while (referencedParts.Count > 0 && referencedParts[0] == ".." && myParts.Count > 0)
            {
                myParts.RemoveAt(myParts.Count-1);
                referencedParts.RemoveAt(0);
            }

            myParts.AddRange(referencedParts);

            var str = string.Join("/", myParts);
            return str.StartsWith("/") ? str : "/" + str;
        }
    }
}