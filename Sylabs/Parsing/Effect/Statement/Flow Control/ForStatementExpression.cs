using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class ForStatementExpression : Symbol
    {
        public ForStatementExpression(params Object[] symbols) : base(symbols) { }

        public static ForStatementExpression Produce(IEnumerable<Symbol> symbols)
        {
            return symbols == null ? null : new ForStatementExpression(symbols);
        }        
    }
}
