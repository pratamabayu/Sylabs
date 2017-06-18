using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class TextPart : Symbol
    {
        public string TextValue;
        public override string ToString() { return TextValue; }
        public TextPart(string c) { TextValue = c; }
    }
}
