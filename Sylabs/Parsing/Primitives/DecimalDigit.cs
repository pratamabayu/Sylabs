using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class DecimalDigit : Symbol
    {
        private string CharacterValue;
        public override string ToString() { return CharacterValue; }
        public DecimalDigit(char c) { CharacterValue = c.ToString(); }
    }
}
