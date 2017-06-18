using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class Script : Symbol
    {
        public Script(params Object[] symbols) : base(symbols) { }

        public static Script Produce(IEnumerable<Symbol> symbols)
        {
            ScriptExpression expression = ScriptExpression.Produce(symbols);
            return expression == null ? null : new Script(expression);
        }
    }
}
