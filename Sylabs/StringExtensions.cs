using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sylabs
{
    public static class StringExtensions
    {
        public static string StripComments(this string input)
        {
            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            return System.Text.RegularExpressions.Regex.Replace(input,
                            blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                            me =>
                            {
                                if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                                    return me.Value.StartsWith("//") ? Environment.NewLine : "";
                                // Keep the literal strings
                                return me.Value;
                            },
                            System.Text.RegularExpressions.RegexOptions.Singleline);
        }

        public static string CleanUnwanted(this string input)
        {
            return Regex.Replace(input, @"[\r\n]", "");
        }

        /// <summary>
        /// Get string value between [first] a and [last] b.
        /// </summary>
        public static string Between(this string value, string a, string b)
        {
            int posA = value.IndexOf(a);
            int posB = value.LastIndexOf(b);
            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        }

        /// <summary>
        /// Get string value after [first] a.
        /// </summary>
        public static string Before(this string value, string a)
        {
            int posA = value.IndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        }

        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(this string value, string a)
        {
            int posA = value.LastIndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        }

        public static IEnumerable<string> GetSubStrings(this string input, string start, string end)
        {
            Regex r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
            MatchCollection matches = r.Matches(input);
            foreach (Match match in matches)
                yield return match.Groups[1].Value;
        }

        /// Like linq take - takes the first x characters
        public static string Take(this string theString, int count, bool ellipsis = false)
        {
            int lengthToTake = Math.Min(count, theString.Length);
            var cutDownString = theString.Substring(0, lengthToTake);

            if (ellipsis && lengthToTake < theString.Length)
                cutDownString += "...";

            return cutDownString;
        }

        //like linq skip - skips the first x characters and returns the remaining string
        public static string Skip(this string theString, int count)
        {
            int startIndex = Math.Min(count, theString.Length);
            var cutDownString = theString.Substring(startIndex - 1);

            return cutDownString;
        }

        //reverses the string... pretty obvious really
        public static string Reverse(this string input)
        {
            char[] chars = input.ToCharArray();
            Array.Reverse(chars);
            return new String(chars);
        }

        // "a string".IsNullOrEmpty() beats string.IsNullOrEmpty("a string")
        public static bool IsNullOrEmpty(this string theString)
        {
            return string.IsNullOrEmpty(theString);
        }

        //not so sure about this one -
        //"a string {0}".Format("blah") vs string.Format("a string {0}", "blah")
        public static string With(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        //ditches html tags - note it doesnt get rid of things like &nbsp;
        public static string StripHtml(this string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return Regex.Replace(html, @"<[^>]*>", string.Empty);
        }

        public static bool Match(this string value, string pattern)
        {
            return Regex.IsMatch(value, pattern);
        }

        //splits string into array with chunks of given size. not really that useful..
        public static string[] SplitIntoChunks(this string toSplit, int chunkSize)
        {
            if (string.IsNullOrEmpty(toSplit))
                return new string[] { "" };

            int stringLength = toSplit.Length;

            int chunksRequired = (int)Math.Ceiling((decimal)stringLength / (decimal)chunkSize);
            var stringArray = new string[chunksRequired];

            int lengthRemaining = stringLength;

            for (int i = 0; i < chunksRequired; i++)
            {
                int lengthToUse = Math.Min(lengthRemaining, chunkSize);
                int startIndex = chunkSize * i;
                stringArray[i] = toSplit.Substring(startIndex, lengthToUse);

                lengthRemaining = lengthRemaining - lengthToUse;
            }

            return stringArray;
        }

        public static string Join(this IEnumerable<object> array, string seperator)
        {
            if (array == null)
                return "";

            return string.Join(seperator, array.ToArray());
        }

        public static string Join(this object[] array, string seperator)
        {
            if (array == null)
                return "";

            return string.Join(seperator, array);
        }
    }
}
