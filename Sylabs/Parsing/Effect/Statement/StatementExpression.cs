using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class StatementExpression : Symbol
    {
        public StatementExpression(params Object[] symbols) : base(symbols) { }

        public static StatementExpression Produce(IEnumerable<Symbol> symbols)
        {
            List<Symbol> result = new List<Symbol>();

            // Get variable type
            string left1 = ParserExtensions.ExtractText(ref symbols);
            if (!string.IsNullOrEmpty(left1))
            {
                // Add variable type
                result.Add(new TextPart(left1));
                ParserExtensions.RemoveBlankSpace(ref symbols);
            }

            // Get variable name
            string left2 = ParserExtensions.ExtractText(ref symbols);
            if (!string.IsNullOrEmpty(left2))
            {
                // Add variable name
                result.Add(new TextPart(left2));
                ParserExtensions.RemoveBlankSpace(ref symbols);
            }

            // Chack it has modified assign
            Symbol checking = symbols.First();
            if (checking is Plus || checking is Minus || 
                checking is Asterisk || checking is ForwardSlash)
            {
                symbols = symbols.Skip(1);
                result.Add(checking);

                ParserExtensions.RemoveBlankSpace(ref symbols);

                checking = symbols.First();
                if (!(checking is Equals))
                {
                    string variableTypeName = left1;
                    if (!string.IsNullOrEmpty(left2))
                        variableTypeName += " " + left2;

                    throw new ParserException("Error parsed statement for " + variableTypeName + ". Missing assign");
                }
            }

            // Chack it has assign or not
            if (checking is Equals)
            {
                // If has
                // Remove assign from symbols
                // Add assign to members
                int assignBefore = symbols.TakeWhile(s => !(s is Equals)).Count();
                symbols = symbols.Skip(assignBefore + 1);
                result.Add(new Equals());

                // Parsing right side
                ParsingRightSide(ref symbols, result);

                // Remove semicolon from symbols
                // Add sememicolon to members
                int semicolonBefore = symbols.TakeWhile(s => !(s is Semicolon)).Count();
                symbols = symbols.Skip(semicolonBefore + 1);
                result.Add(new Semicolon());
            }
            // Chack it has semicolon or not
            else if (checking is Semicolon)
            {
                // If not
                // Remove semicolon from symbols
                // Add sememicolon to members
                int semicolonBefore = symbols.TakeWhile(s => !(s is Semicolon)).Count();
                symbols = symbols.Skip(semicolonBefore + 1);
                result.Add(new Semicolon());
            }
            else
            {
                string variableTypeName = left1;
                if (!string.IsNullOrEmpty(left2))
                    variableTypeName += " " + left2;

                throw new ParserException("Error parsed statement for " + variableTypeName);
            }

            return result == null ? null : new StatementExpression(result);
        }

        public static void ParsingRightSide(ref IEnumerable<Symbol> symbols, List<Symbol> result)
        {
            ParserExtensions.RemoveBlankSpace(ref symbols);
            bool searching = symbols.Count() > 0;

            while (searching)
            {
                string text = ParserExtensions.ExtractText(ref symbols);
                if (!string.IsNullOrEmpty(text))
                    ParserExtensions.RemoveBlankSpace(ref symbols);

                Symbol nextSymbol = null;
                try
                {
                    nextSymbol = symbols.First();
                    if (nextSymbol is DecimalDigit || nextSymbol is Character || nextSymbol is FullStop)
                        nextSymbol = null;
                }
                catch
                {
                    searching = false;
                }

                bool isProcedure = false;
                if (!string.IsNullOrEmpty(text) && nextSymbol is OpenRoundBracket)
                {
                    isProcedure = true;
                    result.Add(ProcedureExpression.Produce(text, ParserExtensions.TakeSymbolInRoundBrackets(ref symbols)));
                }
                else if (nextSymbol is Semicolon)
                {
                    if (!string.IsNullOrEmpty(text))
                        result.Add(new TextPart(text));
                    ParserExtensions.RemoveBlankSpace(ref symbols);
                    break;
                }
                else
                {
                    if (!string.IsNullOrEmpty(text))
                        result.Add(new TextPart(text));
                }

                
                if (!isProcedure && nextSymbol != null)
                {                   
                    result.Add(nextSymbol);
                    symbols = symbols.Skip(1);
                }

                ParserExtensions.RemoveBlankSpace(ref symbols);
                searching = symbols.Count() > 0;
            }
        }
    }
}
