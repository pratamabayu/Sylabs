using Sylabs.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Translating
{
    public sealed class ResourceDescription : ElementDescription<ResourceExpression>
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

        #region Constructors
        public ResourceDescription(ResourceExpression e)
            : base(e)
        {
        }
        #endregion

        #region Methods
        protected override void Initialize(ResourceExpression e)
        {
            // Get data
            IEnumerable<Symbol> symbols = e.ConstituentSymbols;

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
        #endregion
    }
}
