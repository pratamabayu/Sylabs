using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class PassExpression : Symbol
    {
        public PassExpression(params Object[] symbols) : base(symbols) { }

        public static PassExpression Produce(IEnumerable<Symbol> symbols)
        {
            // Take identifier name openbracket
            IEnumerable<Symbol> firsts = symbols.Take(3);

            // Take close bracket
            Symbol last = symbols.ElementAt(symbols.Count() - 1);

            // Skip by first
            symbols = symbols.Skip(firsts.Count()).SkipLast(1);

            List<Symbol> compiles = new List<Symbol>();
            // Populate compiles
            RecursiveProduce(symbols, compiles);

            // Combine
            List<Symbol> compilesSymbol = new List<Symbol>();
            compilesSymbol.AddRange(firsts);
            compilesSymbol.AddRange(compiles);
            compilesSymbol.Add(last);

            return symbols == null ? null : new PassExpression(compilesSymbol);
        }

        static void RecursiveProduce(IEnumerable<Symbol> symbols, List<Symbol> compiles)
        {           
            // Empty blank space before keyword
            ParserExtensions.RemoveBlankSpace(ref symbols);

            // Return if symbols <= 0
            if (symbols.Count() <= 0)
                return;

            // Get compile shader
            List<Symbol> compileSymbols = symbols.TakeWhile(s => !(s is Semicolon)).ToList();
            symbols = symbols.Skip(compileSymbols.Count() + 1);

            // Return if symbols <= 0
            if (symbols.Count() <= 0)
                return;

            compileSymbols.Add(new Semicolon());

            // Add to compiles
            compiles.Add(CompileExpression.Produce(compileSymbols));

            RecursiveProduce(symbols, compiles);
        }
    }
}
