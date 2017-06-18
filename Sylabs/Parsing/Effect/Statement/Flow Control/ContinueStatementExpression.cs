using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class ContinueStatementExpression : Symbol
    {
        public ContinueStatementExpression(params Object[] symbols) : base(symbols) { }

        public static ContinueStatementExpression Produce(IEnumerable<Symbol> symbols)
        {
            List<Symbol> result = new List<Symbol>();

            // Get return syntax
            string returnName = ParserExtensions.ExtractText(ref symbols);
            if (!string.IsNullOrEmpty(returnName))
            {
                // Add return syntax
                result.Add(new TextPart(returnName));
                ParserExtensions.RemoveBlankSpace(ref symbols);
            }

            // Remove semicolon from symbols
            // Add sememicolon to members
            int semicolonBefore = symbols.TakeWhile(s => !(s is Semicolon)).Count();
            symbols = symbols.Skip(semicolonBefore + 1);
            result.Add(new Semicolon());

            return symbols == null ? null : new ContinueStatementExpression(result);
        }        
    }
}
