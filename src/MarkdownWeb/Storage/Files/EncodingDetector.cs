﻿using System;
using System.IO;
using System.Text;

namespace MarkdownWeb.Storage.Files
{
    public class FileHelper
    {
        // Function to detect the encoding for UTF-7, UTF-8/16/32 (bom, no bom, little
        // & big endian), and local default codepage, and potentially other codepages.
        // 'taster' = number of bytes to check of the file (to save processing). Higher
        // value is slower, but more reliable (especially UTF-8 with special characters
        // later on may appear to be ASCII initially). If taster = 0, then taster
        // becomes the length of the file (for maximum reliability). 'text' is simply
        // the string with the discovered encoding applied to the file.
        //
        // Credits: http://stackoverflow.com/questions/1025332/determine-a-strings-encoding-in-c-sharp
        public static string ReadEncoded(string filename, out Encoding encoding, int taster = 1000)
        {
            var b = File.ReadAllBytes(filename);

            //////////////// First check the low hanging fruit by checking if a
            //////////////// BOM/signature exists (sourced from http://www.unicode.org/faq/utf_bom.html#bom4)
            if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF)
            {
                encoding= Encoding.GetEncoding("utf-32BE");
                return Encoding.GetEncoding("utf-32BE").GetString(b, 4, b.Length - 4);
            } // UTF-32, big-endian 
            if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00)
            {
                encoding= Encoding.UTF32;
                return Encoding.UTF32.GetString(b, 4, b.Length - 4);
            } // UTF-32, little-endian
            if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF)
            {
                encoding= Encoding.BigEndianUnicode;
                return Encoding.BigEndianUnicode.GetString(b, 2, b.Length - 2);
            } // UTF-16, big-endian
            if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE)
            {
                encoding= Encoding.Unicode;
                return Encoding.Unicode.GetString(b, 2, b.Length - 2);
            } // UTF-16, little-endian
            if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF)
            {
                encoding = Encoding.UTF8;
                return Encoding.UTF8.GetString(b, 3, b.Length - 3);
            } // UTF-8
            if (b.Length >= 3 && b[0] == 0x2b && b[1] == 0x2f && b[2] == 0x76)
            {
                encoding = Encoding.UTF7;
                return Encoding.UTF7.GetString(b, 3, b.Length - 3);
            } // UTF-7


            //////////// If the code reaches here, no BOM/signature was found, so now
            //////////// we need to 'taste' the file to see if can manually discover
            //////////// the encoding. A high taster value is desired for UTF-8
            if (taster == 0 || taster > b.Length)
                taster = b.Length; // Taster size can't be bigger than the filesize obviously.


            // Some text files are encoded in UTF8, but have no BOM/signature. Hence
            // the below manually checks for a UTF8 pattern. This code is based off
            // the top answer at: http://stackoverflow.com/questions/6555015/check-for-invalid-utf8
            // For our purposes, an unnecessarily strict (and terser/slower)
            // implementation is shown at: http://stackoverflow.com/questions/1031645/how-to-detect-utf-8-in-plain-c
            // For the below, false positives should be exceedingly rare (and would
            // be either slightly malformed UTF-8 (which would suit our purposes
            // anyway) or 8-bit extended ASCII/UTF-16/32 at a vanishingly long shot).
            var i = 0;
            var utf8 = false;
            while (i < taster - 4)
            {
                if (b[i] <= 0x7F)
                {
                    i += 1;
                    continue;
                }
                    // If all characters are below 0x80, then it is valid UTF8, but UTF8 is not 'required' (and therefore the text is more desirable to be treated as the default codepage of the computer). Hence, there's no "utf8 = true;" code unlike the next three checks.
                if (b[i] >= 0xC2 && b[i] <= 0xDF && b[i + 1] >= 0x80 && b[i + 1] < 0xC0)
                {
                    i += 2;
                    utf8 = true;
                    continue;
                }
                if (b[i] >= 0xE0 && b[i] <= 0xF0 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 &&
                    b[i + 2] < 0xC0)
                {
                    i += 3;
                    utf8 = true;
                    continue;
                }
                if (b[i] >= 0xF0 && b[i] <= 0xF4 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 &&
                    b[i + 2] < 0xC0 && b[i + 3] >= 0x80 && b[i + 3] < 0xC0)
                {
                    i += 4;
                    utf8 = true;
                    continue;
                }
                utf8 = false;
                break;
            }
            if (utf8 == true)
            {
                encoding= Encoding.UTF8;
                return Encoding.UTF8.GetString(b);
            }


            // The next check is a heuristic attempt to detect UTF-16 without a BOM.
            // We simply look for zeroes in odd or even byte places, and if a certain
            // threshold is reached, the code is 'probably' UF-16.          
            var threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
            var count = 0;
            for (var n = 0; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double) count)/taster > threshold)
            {
                encoding = Encoding.BigEndianUnicode;
                return Encoding.BigEndianUnicode.GetString(b);
            }
            count = 0;
            for (var n = 1; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double) count)/taster > threshold)
            {
                encoding= Encoding.Unicode;
                return Encoding.Unicode.GetString(b);
            } // (little-endian)


            // Finally, a long shot - let's see if we can find "charset=xyz" or
            // "encoding=xyz" to identify the encoding:
            for (var n = 0; n < taster - 9; n++)
            {
                if (
                    ((b[n + 0] == 'c' || b[n + 0] == 'C') && (b[n + 1] == 'h' || b[n + 1] == 'H') &&
                     (b[n + 2] == 'a' || b[n + 2] == 'A') && (b[n + 3] == 'r' || b[n + 3] == 'R') &&
                     (b[n + 4] == 's' || b[n + 4] == 'S') && (b[n + 5] == 'e' || b[n + 5] == 'E') &&
                     (b[n + 6] == 't' || b[n + 6] == 'T') && (b[n + 7] == '=')) ||
                    ((b[n + 0] == 'e' || b[n + 0] == 'E') && (b[n + 1] == 'n' || b[n + 1] == 'N') &&
                     (b[n + 2] == 'c' || b[n + 2] == 'C') && (b[n + 3] == 'o' || b[n + 3] == 'O') &&
                     (b[n + 4] == 'd' || b[n + 4] == 'D') && (b[n + 5] == 'i' || b[n + 5] == 'I') &&
                     (b[n + 6] == 'n' || b[n + 6] == 'N') && (b[n + 7] == 'g' || b[n + 7] == 'G') && (b[n + 8] == '='))
                    )
                {
                    if (b[n + 0] == 'c' || b[n + 0] == 'C') n += 8;
                    else n += 9;
                    if (b[n] == '"' || b[n] == '\'') n++;
                    var oldn = n;
                    while (n < taster &&
                           (b[n] == '_' || b[n] == '-' || (b[n] >= '0' && b[n] <= '9') || (b[n] >= 'a' && b[n] <= 'z') ||
                            (b[n] >= 'A' && b[n] <= 'Z')))
                    {
                        n++;
                    }
                    var nb = new byte[n - oldn];
                    Array.Copy(b, oldn, nb, 0, n - oldn);
                    try
                    {
                        var internalEnc = Encoding.ASCII.GetString(nb);
                        encoding= Encoding.GetEncoding(internalEnc);
                        return Encoding.GetEncoding(internalEnc).GetString(b);
                    }
                    catch
                    {
                        break;
                    } // If C# doesn't recognize the name of the encoding, break.
                }
            }


            // If all else fails, the encoding is probably (though certainly not
            // definitely) the user's local codepage! One might present to the user a
            // list of alternative encodings as shown here: http://stackoverflow.com/questions/8509339/what-is-the-most-common-encoding-of-each-language
            // A full list can be found using Encoding.GetEncodings();
            encoding= Encoding.Default;
            return Encoding.Default.GetString(b);
        }
    }
}