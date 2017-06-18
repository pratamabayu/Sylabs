using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class NegSign : Symbol
    {
        public static NegSign Produce(Symbol symbol)
        {
            // neg-sign = minus

            if (symbol is Minus)
                return new NegSign(symbol);
            return null;
        }
        public NegSign(params Object[] symbols) : base(symbols) { }
    }
}
