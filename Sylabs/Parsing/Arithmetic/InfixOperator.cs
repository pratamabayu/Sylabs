using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class InfixOperator : Symbol
    {
        public static InfixOperator Produce(Symbol symbol)
        {
            // infix-operator = caret / asterisk / forward-slash / plus / minus

            if (symbol is Plus || symbol is Minus || symbol is Asterisk || symbol is ForwardSlash
                || symbol is Caret)
                return new InfixOperator(symbol);
            return null;
        }
        public InfixOperator(params Object[] symbols) : base(symbols) { }
    }
}
