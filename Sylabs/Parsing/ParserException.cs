using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class ParserException : Exception
    {
        public ParserException(string message) : base(message) { }
    }
}
