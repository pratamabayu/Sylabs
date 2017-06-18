using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public sealed class ProcedureExpression : Symbol
    {
        public string Name { get; private set; }

        public ProcedureExpression(params Object[] symbols) : base(symbols) 
        {
            TextPart namePart = this.ConstituentSymbols[0] as TextPart;
            this.Name = namePart.TextValue;
        }

        public static ProcedureExpression Produce(string name, IEnumerable<Symbol> symbols)
        {
            List<Symbol> result = new List<Symbol>();
            RecursiveProduce(name, ref symbols, result);

            return result == null ? null : new ProcedureExpression(result);
        }

        static void RecursiveProduce(string name, ref IEnumerable<Symbol> symbols, List<Symbol> result)
        {
            ParserExtensions.RemoveBlankSpace(ref symbols);

            if (string.IsNullOrEmpty(name))
                return;

            // Write procedure name
            result.Add(new TextPart(name));

            // Begin procedure
            result.Add(new OpenRoundBracket());

            bool searching = true;
            while (searching)
            {
                string text = ParserExtensions.ExtractText(ref symbols);
                if (!string.IsNullOrEmpty(text))
                    ParserExtensions.RemoveBlankSpace(ref symbols);

                Symbol nextSymbol = null;
                try
                {
                    nextSymbol = symbols.First(s => !(s is DecimalDigit ||
                        s is Character || s is FullStop));
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

            // End procedure
            result.Add(new CloseRoundBracket());
        }
    }
}
