using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class FractionalPart : Symbol
    {
        public static FractionalPart Produce(IEnumerable<Symbol> symbols)
        {
            // fractional-part = full-stop digit-sequence

            if (!symbols.Any())
                return null;
            if (symbols.First() is FullStop)
            {
                IEnumerable<Symbol> s = null;
                DigitSequence d = DigitSequence.Produce(symbols.Skip(1), out s);
                if (d == null || s.Any())
                    return null;
                return new FractionalPart(new FullStop(), d);
            }
            return null;
        }
        public FractionalPart(params Object[] symbols) : base(symbols) { }
    }
}
