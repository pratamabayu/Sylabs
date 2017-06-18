using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class StructExpression : Symbol
    {
        public string Name { get; private set; }

        public StructExpression(params Object[] symbols) : base(symbols) 
        {
            TextPart namePart = this.ConstituentSymbols[1] as TextPart;
            this.Name = namePart.TextValue;
        }

        public static StructExpression Produce(IEnumerable<Symbol> symbols)
        {
            // Take identifier name openbracket
            IEnumerable<Symbol> firsts = symbols.Take(3);

            // Take close bracket
            IEnumerable<Symbol> lasts = symbols.Reverse().Take(2).Reverse();

            // Skip by first "{" and last "};"
            symbols = symbols.Skip(firsts.Count()).SkipLast(2);

            // Combine
            List<Symbol> combinedSymbol = new List<Symbol>();
            combinedSymbol.AddRange(firsts);
            combinedSymbol.Add(StructStatementBlockExpression.Produce(symbols));
            combinedSymbol.AddRange(lasts);
            
            return new StructExpression(combinedSymbol);
        }        
    }
}
