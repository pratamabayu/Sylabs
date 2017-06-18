using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class WholeNumberPart : Symbol
    {
        public static WholeNumberPart Produce(IEnumerable<Symbol> symbols,
            out IEnumerable<Symbol> symbolsToProcess)
        {
            // whole-number-part = digit-sequence

            IEnumerable<Symbol> s = null;
            DigitSequence d = DigitSequence.Produce(symbols, out s);
            if (d != null)
            {
                symbolsToProcess = s;
                return new WholeNumberPart(d);
            }
            symbolsToProcess = null;
            return null;
        }
        public WholeNumberPart(params Object[] symbols) : base(symbols) { }
    }
}
