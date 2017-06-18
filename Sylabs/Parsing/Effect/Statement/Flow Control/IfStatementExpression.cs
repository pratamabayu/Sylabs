using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class IfStatementExpression : Symbol
    {
        public IfStatementExpression(params Object[] symbols) : base(symbols) { }

        public static IfStatementExpression Produce(IEnumerable<Symbol> symbols)
        {
            return symbols == null ? null : new IfStatementExpression(symbols);
        }        
    }
}
