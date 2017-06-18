using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class ResourceExpression : Symbol
    {
        #region Fields
        static string[] _allType = new string[] { "texture2D", "textureCube" };
        #endregion

        #region Properties
        public string Type
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }
        #endregion

        public ResourceExpression(params Object[] inputSymbols) : base(inputSymbols) 
        {
            // Get data
            IEnumerable<Symbol> symbols = this.ConstituentSymbols;

            // Get type part
            TextPart typePart = symbols.ElementAt(0) as TextPart;
            string type = typePart.ToString();

            // Init type
            foreach (string s in _allType)
            {
                if (type.StartsWith(s))
                {
                    this.Type = s;
                    break;
                }
            }

            if (string.IsNullOrEmpty(this.Type))
                throw new ArgumentException("Invalid type");

            // Get name part
            TextPart namePart = symbols.ElementAt(1) as TextPart;
            this.Name = namePart.ToString();
        }

        public static ResourceExpression Produce(IEnumerable<Symbol> symbols)
        {
            return symbols == null ? null : new ResourceExpression(symbols);
        }
    }
}
