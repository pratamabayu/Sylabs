using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class FunctionExpression : Symbol
    {
        public string Name { get; private set; }

        public FunctionExpression(params Object[] symbols) : base(symbols) 
        {
            TextPart namePart = this.ConstituentSymbols[1] as TextPart;
            this.Name = namePart.TextValue;
        }

        public static FunctionExpression Produce(IEnumerable<Symbol> symbols)
        {
            return symbols == null ? null : new FunctionExpression(symbols);
        }        
    }
}
