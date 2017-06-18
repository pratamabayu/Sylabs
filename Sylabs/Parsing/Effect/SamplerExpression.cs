using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class SamplerExpression : Symbol
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

        public SamplerExpression(params Object[] inputSymbols) : base(inputSymbols) 
        {
            // Set default data
            this.Resource = string.Empty;
            this.Filter = "Linear";
            this.AddressU = "Clamp";
            this.AddressV = "Clamp";
            this.AddressV = "Clamp";

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

        public static SamplerExpression Produce(IEnumerable<Symbol> symbols)
        {
            // Take identifier name openbracket
            IEnumerable<Symbol> firsts = symbols.Take(3);

            // Take close bracket
            IEnumerable<Symbol> lasts = symbols.Reverse().Take(2).Reverse();

            // Skip by first
            symbols = symbols.Skip(firsts.Count()).SkipLast(2);

            List<Symbol> members = new List<Symbol>();
            RecursiveProduce(symbols, members);

            // Combine
            List<Symbol> combinedSymbol = new List<Symbol>();
            combinedSymbol.AddRange(firsts);
            combinedSymbol.AddRange(members);
            combinedSymbol.AddRange(lasts);

            return symbols == null ? null : new SamplerExpression(combinedSymbol);
        }

        static void RecursiveProduce(IEnumerable<Symbol> symbols, List<Symbol> members)
        {
            ParserExtensions.RemoveBlankSpace(ref symbols);

            if (symbols.Count() <= 0)
                return;

            string keyword = ParserExtensions.ExtractText(ref symbols);

            if (!string.IsNullOrEmpty(keyword))
            {
                members.Add(new TextPart(keyword));

                ParserExtensions.RemoveBlankSpace(ref symbols);

                int assignBefore = symbols.TakeWhile(s => !(s is Equals)).Count();
                symbols = symbols.Skip(assignBefore + 1);
                members.Add(new Equals());

                ParserExtensions.RemoveBlankSpace(ref symbols);

                string inputType = ParserExtensions.ExtractText(ref symbols);
                members.Add(new TextPart(inputType));

                if (string.IsNullOrEmpty(inputType))
                    throw new ParserException("Error sampler parsed");

                int semicolonBefore = symbols.TakeWhile(s => !(s is Semicolon)).Count();
                symbols = symbols.Skip(semicolonBefore + 1);
                members.Add(new Semicolon());
            }

            RecursiveProduce(symbols, members);
        }
    }
}
