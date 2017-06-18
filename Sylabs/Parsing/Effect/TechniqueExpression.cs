using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class TechniqueExpression : Symbol
    {
        public TechniqueExpression(params Object[] symbols) : base(symbols) { }

        public static TechniqueExpression Produce(IEnumerable<Symbol> symbols)
        {
            // Take identifier name openbracket
            IEnumerable<Symbol> firsts = symbols.Take(3);

            // Take close bracket
            Symbol last = symbols.ElementAt(symbols.Count() - 1);

            // Skip by first
            symbols = symbols.Skip(firsts.Count()).SkipLast(1);            

            List<Symbol> passes = new List<Symbol>();
            // Populate pass
            RecursiveProduce(symbols, passes);

            // Combine
            List<Symbol> techniqueSymbol = new List<Symbol>();
            techniqueSymbol.AddRange(firsts);
            techniqueSymbol.AddRange(passes);
            techniqueSymbol.Add(last);

            return symbols == null ? null : new TechniqueExpression(techniqueSymbol);
        }

        static void RecursiveProduce(IEnumerable<Symbol> symbols, List<Symbol> passes)
        {
            ParserExtensions.RemoveBlankSpace(ref symbols);

            if (symbols.Count() <= 0)
                return;           

            // Get keyword
            string keyword = ParserExtensions.ExtractText(ref symbols);

            // Empty blank space after keyword
            ParserExtensions.RemoveBlankSpace(ref symbols);

            // Get name
            string name = ParserExtensions.ExtractText(ref symbols);

            // Empty blank space after name
            ParserExtensions.RemoveBlankSpace(ref symbols);            

            if (!string.IsNullOrEmpty(keyword) && !string.IsNullOrEmpty(name))
            {
                if (keyword == "pass")
                {
                    int openBracketIndex = 0;
                    int closeBracketIndex = 0;
                    if (symbols.RollUpCurlyBrackets(out openBracketIndex, out closeBracketIndex))
                    {
                        // Get body
                        List<Symbol> bodySymbols = symbols.Take(closeBracketIndex + 1).ToList();

                        // Remove body
                        symbols = symbols.Skip(closeBracketIndex + 1).ToList();

                        // Create pass symbols
                        List<Symbol> passSymbols = new List<Symbol>();
                        passSymbols.Add(new TextPart(keyword));
                        passSymbols.Add(new TextPart(name));
                        passSymbols.AddRange(bodySymbols);

                        // Parse technique
                        passes.Add(PassExpression.Produce(passSymbols));
                    }
                    else
                        throw new InvalidOperationException("Roll Up Bracket");
                }
            }

            RecursiveProduce(symbols, passes);
        }
    }
}
