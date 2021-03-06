using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class PrefixOperator : Symbol
    {
        public static PrefixOperator Produce(Symbol symbol)
        {
            // prefix-operator = plus / minus

            if (symbol is Plus || symbol is Minus)
                return new PrefixOperator(symbol);
            return null;
        }
        public PrefixOperator(params Object[] symbols) : base(symbols) { }
    }
}
