using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class ForHeaderExpression : Symbol
    {
        public ForHeaderExpression(params Object[] symbols) : base(symbols) { }

        public static ForHeaderExpression Produce(IEnumerable<Symbol> symbols)
        {
            List<Symbol> result = new List<Symbol>();

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

            return symbols == null ? null : new ForHeaderExpression(result);
        }        
    }
}
