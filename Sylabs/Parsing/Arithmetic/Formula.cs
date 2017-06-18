using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class Formula : Symbol
    {        
        public Formula(params Object[] symbols) : base(symbols) { }

        public static Formula Produce(IEnumerable<Symbol> symbols)
        {
            // formula = expression

            Expression e = Expression.Produce(symbols);
            return e == null ? null : new Formula(e);
        }
    }
}
