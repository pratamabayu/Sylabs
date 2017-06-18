using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class Character : Symbol
    {
        private string CharacterValue;
        public override string ToString() { return CharacterValue; }
        public Character(char c) { CharacterValue = c.ToString(CultureInfo.InvariantCulture); }
    }
}
