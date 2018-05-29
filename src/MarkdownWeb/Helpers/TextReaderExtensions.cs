using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MarkdownWeb.Helpers
{
    public static class TextReaderExtensions
    {
        public static string ReadUntilEmptyLine(this TextReader reader)
        {
            var s = "";
            while (true)
            {
                var s2 = reader.ReadLine();
                if (string.IsNullOrEmpty(s2))
                    return s;

                s += s2 + Environment.NewLine;
            }
        }

        public static void SkipEmptyLines(this TextReader reader)
        {
            while (true)
            {
                if (reader.Peek() == '\r' || reader.Peek() == '\n')
                    reader.ReadLine();
                else
                    return;
            }
        }
    }
}
