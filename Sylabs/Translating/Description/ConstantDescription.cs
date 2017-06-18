using Sylabs.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sylabs.Translating
{
    public sealed class ConstantDescription : ElementDescription<ConstantExpression>
    {
        #region Fields
        static string[] _allType = new string[] { "float", "int", "bool" };
        #endregion

        #region Properties
        public string Type
        {
            get;
            private set;
        }

        public int[] Dimension
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public int ArraySize
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        public ConstantDescription(ConstantExpression e)
            : base(e)
        {
        }
        #endregion

        #region Methods
        protected override void Initialize(ConstantExpression e)
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

            // Init dimension
            this.Dimension = new int[] { 1, 1 };
            string dimension = type.Replace(this.Type, "");
            if (!string.IsNullOrEmpty(dimension))
            {
                if (dimension.Contains('x'))
                {
                    string[] value = dimension.Split('x');
                    this.Dimension[0] = int.Parse(value[0], CultureInfo.InvariantCulture);
                    this.Dimension[1] = int.Parse(value[1], CultureInfo.InvariantCulture);
                }
                else
                    this.Dimension[0] = int.Parse(dimension, CultureInfo.InvariantCulture);
            }

            // Get name part
            TextPart namePart = symbols.ElementAt(1) as TextPart;
            string name = namePart.ToString();

            // Init name and array            
            if (name.Contains('[') && name.Contains(']'))
            {
                // Extract data
                string[] value = name.Split(new char[] { '[', ']' });

                // Set data
                this.Name = value[0];
                this.ArraySize = int.Parse(value[1], CultureInfo.InvariantCulture);
            }
            else
            {
                // Set data
                this.Name = name;
                this.ArraySize = 0;
            }

            // Set value
            this.Value = string.Empty;
        }
        #endregion
    }
}
