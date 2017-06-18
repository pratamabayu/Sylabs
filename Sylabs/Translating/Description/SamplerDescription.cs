using Sylabs.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Translating
{
    public sealed class SamplerDescription : ElementDescription<SamplerExpression>
    {
        #region Fields
        static string[] _allType = new string[] { "sampler2D", "samplerCube" };
        #endregion

        #region Properties
        public string Type { get; set; }
        public string Name { get; set; }

        public string Resource { get; set; }
        public string Filter { get; set; }
        public string AddressU { get; set; }
        public string AddressV { get; set; }
        public string AddressW { get; set; }
        #endregion

        #region Constructors
        public SamplerDescription(SamplerExpression e)
            : base(e)
        {
        }
        #endregion

        #region Methods
        protected override void Initialize(SamplerExpression e)
        {
            // Set default data
            this.Resource = string.Empty;
            this.Filter = "Linear";
            this.AddressU = "Clamp";
            this.AddressV = "Clamp";
            this.AddressW = "Clamp";

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
                throw new ArgumentException("Invalid sampler type \"" + type + "\"");

            // Get name part
            TextPart namePart = symbols.ElementAt(1) as TextPart;
            this.Name = namePart.ToString();

            // Get members
            IEnumerable<Symbol> memberExpr = symbols.Where(s => s is TextPart);
            if (memberExpr != null && memberExpr.Count() > 1)
            {
                int count = memberExpr.Count();
                for (int i = 0; i < count; i += 2)
                {
                    TextPart key = memberExpr.ElementAt(i) as TextPart;
                    TextPart value = memberExpr.ElementAt(i + 1) as TextPart;

                    if (!(key != null && value != null))
                        throw new ArgumentException("Invalid get sampler member");

                    if (key.TextValue == "Texture")
                        this.Resource = value.TextValue;
                    else if (key.TextValue == "Filter")
                        this.Filter = value.TextValue;
                    else if (key.TextValue == "AddressU")
                        this.AddressU = value.TextValue;
                    else if (key.TextValue == "AddressV")
                        this.AddressV = value.TextValue;
                    else if (key.TextValue == "AddressW")
                        this.AddressW = value.TextValue;
                }
            }
            else
                throw new ArgumentException("Invalid sampler members");
        }
        #endregion
    }
}
