using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class DigitSequence : Symbol
    {
        public static DigitSequence Produce(IEnumerable<Symbol> symbols,
            out IEnumerable<Symbol> symbolsToProcess)
        {
            // digit-sequence = 1*decimal-digit

            IEnumerable<Symbol> digits = symbols.TakeWhile(s => s is DecimalDigit);
            if (digits.Any())
            {
                symbolsToProcess = symbols.Skip(digits.Count());
                return new DigitSequence(digits);
            }
            symbolsToProcess = null;
            return null;
        }
        public DigitSequence(params Object[] symbols) : base(symbols) { }
    }
}
