using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public abstract class Symbol
    {
        public List<Symbol> ConstituentSymbols { get; set; }
        
        public Symbol(params Object[] symbols)
        {
            List<Symbol> ls = new List<Symbol>();
            foreach (var item in symbols)
            {
                if (item is Symbol)
                    ls.Add((Symbol)item);
                else if (item is IEnumerable<Symbol>)
                    foreach (var item2 in (IEnumerable<Symbol>)item)
                        ls.Add(item2);
                else
                    // If this error is thrown, the parser is coded incorrectly.
                    throw new ParserException("Internal error");
            }
            ConstituentSymbols = ls;
        }
        public Symbol() { }

        public override string ToString()
        {
            string s = ConstituentSymbols.Select(ct => ct.ToString()).StringConcatenate();
            return s;
        }
    }
}
