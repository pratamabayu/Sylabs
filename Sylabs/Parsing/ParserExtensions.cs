using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public static class ParserExtensions
    {
        public static void RemoveBlankSpace(ref IEnumerable<Symbol> symbols)
        {
            int blankSpaceBefore = symbols.TakeWhile(s => s is NewLine || s is Tabulation || s is WhiteSpace).Count();
            symbols = symbols.Skip(blankSpaceBefore);
        }

        public static string GetFirstText(ref IEnumerable<Symbol> symbols)
        {
            string keyword = string.Empty;
            foreach (Symbol s in symbols)
            {
                if (s is NewLine || s is Tabulation || s is WhiteSpace ||
                    s is OpenCurlyBracket || s is CloseCurlyBracket ||
                    s is OpenRoundBracket || s is CloseRoundBracket ||
                    s is OpenSquareBracket || s is CloseSquareBracket ||
                    s is Equals || s is Semicolon || s is Colon || s is Comma || 
                    s is Asterisk || s is Procenttecken || s is ForwardSlash || 
                    s is Plus || s is Minus || 
                    s is GreaterThan || s is LessThan ||
                    s is VerticalBar || s is Ampersand || s is ExclamationMark)
                    break;

                keyword += s.ToString();
            }

            return keyword;
        }

        public static string ExtractText(ref IEnumerable<Symbol> symbols)
        {
            // Get first text
            string keyword = GetFirstText(ref symbols);
            // Normalize
            symbols = symbols.Skip(keyword.Length);

            return keyword;
        }

        public static IEnumerable<T> TakeSymbolInCurlyBrackets<T>(ref IEnumerable<T> symbols) where T : Symbol
        {
            // Begin parse parenthesis
            IEnumerable<T> bodySymbols;
            int openParenthesisIndex = 0;
            int closeParenthesisIndex = 0;

            if (symbols.RollUpCurlyBrackets(out openParenthesisIndex, out closeParenthesisIndex))
            {
                // Get body
                bodySymbols = symbols.Take(closeParenthesisIndex + 1).ToList();

                // Remove open and close parenthesis
                bodySymbols = bodySymbols.Skip(1).SkipLast(1);

                // Remove body
                symbols = symbols.Skip(closeParenthesisIndex + 1).ToList();
            }
            else
                throw new ParserException("Error bracket parsed");

            return bodySymbols;
        }

        public static IEnumerable<T> TakeSymbolInRoundBrackets<T>(ref IEnumerable<T> symbols) where T : Symbol
        {
            // Begin parse parenthesis
            IEnumerable<T> bodySymbols;
            int openParenthesisIndex = 0;
            int closeParenthesisIndex = 0;

            if (symbols.RollUpRoundBrackets(out openParenthesisIndex, out closeParenthesisIndex))
            {
                // Get body
                bodySymbols = symbols.Take(closeParenthesisIndex + 1).ToList();

                // Remove open and close parenthesis
                bodySymbols = bodySymbols.Skip(1).SkipLast(1);

                // Remove body
                symbols = symbols.Skip(closeParenthesisIndex + 1).ToList();
            }
            else
                throw new ParserException("Error parenthesis parsed");

            return bodySymbols;
        }

        public static IEnumerable<T> TakeSymbolInSquareBrackets<T>(ref IEnumerable<T> symbols) where T : Symbol
        {
            // Begin parse parenthesis
            IEnumerable<T> bodySymbols;
            int openParenthesisIndex = 0;
            int closeParenthesisIndex = 0;

            if (symbols.RollUpSquareBrackets(out openParenthesisIndex, out closeParenthesisIndex))
            {
                // Get body
                bodySymbols = symbols.Take(closeParenthesisIndex + 1).ToList();

                // Remove open and close parenthesis
                bodySymbols = bodySymbols.Skip(1).SkipLast(1);

                // Remove body
                symbols = symbols.Skip(closeParenthesisIndex + 1).ToList();
            }
            else
                throw new ParserException("Error parenthesis parsed");

            return bodySymbols;
        }

        public static bool RollUpCurlyBrackets<T>(this IEnumerable<T> symbols, out int openIndex, out int closeIndex) where T : Symbol
        {
            // Remove before blank spaces
            int blankSpaceBefore = symbols.TakeWhile(s => s is NewLine || s is Tabulation || s is WhiteSpace).Count();
            symbols = symbols.Skip(blankSpaceBefore);

            // Roll Up
            openIndex = 0;
            closeIndex = 0;
            int index = 0; 
            int z = -1;
            bool isValidated = false;

            foreach (Symbol s in symbols)
            {
                if (s is OpenCurlyBracket)
                {
                    z++;

                    if (z == 0)
                        openIndex = index;
                }

                if (s is CloseCurlyBracket)
                {
                    if (z == 0)
                    {
                        closeIndex = index;
                        isValidated = true;
                    }
                    else
                        z--;
                }

                index++;

                if (isValidated)
                    break;
            }

            return isValidated;
        }

        public static bool RollUpRoundBrackets<T>(this IEnumerable<T> symbols, out int openIndex, out int closeIndex) where T : Symbol
        {
            // Remove before blank spaces
            int blankSpaceBefore = symbols.TakeWhile(s => s is NewLine || s is Tabulation || s is WhiteSpace).Count();
            symbols = symbols.Skip(blankSpaceBefore);

            // Roll Up
            openIndex = 0;
            closeIndex = 0;
            int index = 0;
            int z = -1;
            bool isValidated = false;

            foreach (Symbol s in symbols)
            {
                if (s is OpenRoundBracket)
                {
                    z++;

                    if (z == 0)
                        openIndex = index;                        
                }

                if (s is CloseRoundBracket)
                {
                    if (z == 0)
                    {
                        closeIndex = index;
                        isValidated = true;
                    }
                    else
                        z--;
                }

                index++;

                if (isValidated)
                    break;
            }

            return isValidated;
        }

        public static bool RollUpSquareBrackets<T>(this IEnumerable<T> symbols, out int openIndex, out int closeIndex) where T : Symbol
        {
            // Remove before blank spaces
            int blankSpaceBefore = symbols.TakeWhile(s => s is NewLine || s is Tabulation || s is WhiteSpace).Count();
            symbols = symbols.Skip(blankSpaceBefore);

            // Roll Up
            openIndex = 0;
            closeIndex = 0;
            int index = 0;
            int z = -1;
            bool isValidated = false;

            foreach (Symbol s in symbols)
            {
                if (s is OpenSquareBracket)
                {
                    z++;

                    if (z == 0)
                        openIndex = index;
                }

                if (s is CloseSquareBracket)
                {
                    if (z == 0)
                    {
                        closeIndex = index;
                        isValidated = true;
                    }
                    else
                        z--;
                }

                index++;

                if (isValidated)
                    break;
            }

            return isValidated;
        }

        public static IEnumerable<Symbol> PopulateSymbols(string text)
        {
            IEnumerable<Symbol> symbols = text.Select(c =>
            {
                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        return new DecimalDigit(c);
                    case ' ':
                        return new WhiteSpace();
                    case '\n':
                        return new NewLine();
                    case '\t':
                        return new Tabulation();
                    case '=':
                        return new Equals();
                    case '>':
                        return new GreaterThan();
                    case '<':
                        return new LessThan();
                    case ':':
                        return new Colon();
                    case '+':
                        return new Plus();
                    case '-':
                        return new Minus();
                    case '*':
                        return new Asterisk();
                    case '/':
                        return new ForwardSlash();
                    case '^':
                        return new Caret();
                    case '.':
                        return new FullStop();
                    case ',':
                        return new Comma();
                    case '&':
                        return new Ampersand();
                    case '|':
                        return new VerticalBar();
                    case '!':
                        return new ExclamationMark();
                    case '(':
                        return new OpenRoundBracket();
                    case ')':
                        return new CloseRoundBracket();
                    case ';':
                        return new Semicolon();
                    case '{':
                        return new OpenCurlyBracket();
                    case '}':
                        return new CloseCurlyBracket();
                    /*case '[':
                        return new OpenSquareBracket();
                    case ']':
                        return new CloseSquareBracket();*/
                    default:
                        return (Symbol)(new Character(c));
                }
            });

            return symbols;
        }

        public static void DumpSymbol(StringBuilder sb, Symbol symbol, int depth)
        {
            sb.Append(string.Format("{0}{1} >{2}<",
                "".PadRight(depth * 2),
                symbol.GetType().Name.ToString(),
                symbol.ToString())).Append(Environment.NewLine);
            if (symbol.ConstituentSymbols != null)
                foreach (var childSymbol in symbol.ConstituentSymbols)
                    DumpSymbol(sb, childSymbol, depth + 1);
        }

        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source,
            int count)
        {
            Queue<T> saveList = new Queue<T>();
            int saved = 0;
            foreach (T item in source)
            {
                if (saved < count)
                {
                    saveList.Enqueue(item);
                    ++saved;
                    continue;
                }
                saveList.Enqueue(item);
                yield return saveList.Dequeue();
            }
            yield break;
        }

        public static string StringConcatenate(this IEnumerable<string> source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in source)
                sb.Append(s);
            return sb.ToString();
        }

        public static string StringConcatenate<T>(
            this IEnumerable<T> source,
            Func<T, string> projectionFunc)
        {
            return source.Aggregate(new StringBuilder(),
                (s, i) => s.Append(projectionFunc(i)),
                s => s.ToString());
        }

        public static IEnumerable<TResult> Rollup<TSource, TResult>(
            this IEnumerable<TSource> source,
            TResult seed,
            Func<TSource, TResult, TResult> projection)
        {
            TResult nextSeed = seed;
            foreach (TSource src in source)
            {
                TResult projectedValue = projection(src, nextSeed);
                nextSeed = projectedValue;
                yield return projectedValue;
            }
        }
    }
}
