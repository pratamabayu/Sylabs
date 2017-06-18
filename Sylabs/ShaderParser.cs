using Sylabs.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs
{
    public sealed class ShaderParser
    {
        #region Fields
        Symbol _script = null;
        #endregion

        #region Properties
        public Symbol Script
        {
            get { return _script; }
        }
        #endregion

        #region Methods
        public void Process(string source)
        {
            // Remove comments
            source = source.StripComments();

            // Remove unwanted characters
            source = source.CleanUnwanted();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(source);
#endif

            // Parsing tree
            _script = Sylabs.Parsing.Script.Produce(ParserExtensions.PopulateSymbols(source));
            if (_script == null)
                throw new ParserException("Invalid script"); 
        }  
        #endregion
    }
}
