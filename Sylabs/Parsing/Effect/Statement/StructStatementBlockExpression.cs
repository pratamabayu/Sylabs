using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class StructStatementBlockExpression : Symbol
    {
        public StructStatementBlockExpression(params Object[] symbols) : base(symbols) { }

        public static StructStatementBlockExpression Produce(IEnumerable<Symbol> symbols)
        {
            List<Symbol> members = new List<Symbol>();
            RecursiveProduce(symbols, members);

            return new StructStatementBlockExpression(members);
        }

        static void RecursiveProduce(IEnumerable<Symbol> symbols, List<Symbol> members)
        {
            ParserExtensions.RemoveBlankSpace(ref symbols);

            if (symbols.Count() <= 0)
                return;

            int semicolonBefore = symbols.TakeWhile(s => !(s is Semicolon)).Count();
            // Get
            IEnumerable<Symbol> memberPartSymbols = symbols.Take(semicolonBefore + 1);
            // Empty
            symbols = symbols.Skip(semicolonBefore + 1);

            // Add to members
            members.Add(StructStatementExpression.Produce(memberPartSymbols));

            // Recursive produce
            RecursiveProduce(symbols, members);
        }
    }
}
