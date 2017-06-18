using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class StructStatementExpression : Symbol
    {
        public StructStatementExpression(params Object[] symbols) : base(symbols) { }

        public static StructStatementExpression Produce(IEnumerable<Symbol> symbols)
        {
            List<Symbol> members = new List<Symbol>();

            string keyword = ParserExtensions.ExtractText(ref symbols);
            
            if (!string.IsNullOrEmpty(keyword))
            {
                members.Add(new TextPart(keyword));

                ParserExtensions.RemoveBlankSpace(ref symbols);

                string name = ParserExtensions.ExtractText(ref symbols);
                members.Add(new TextPart(name));

                if (string.IsNullOrEmpty(name))
                    throw new ParserException("Error struct parsed");

                int doubleDotBefore = symbols.TakeWhile(s => !(s is Colon)).Count();
                symbols = symbols.Skip(doubleDotBefore + 1);
                members.Add(new Colon());

                ParserExtensions.RemoveBlankSpace(ref symbols);

                string inputType = ParserExtensions.ExtractText(ref symbols);
                members.Add(new TextPart(inputType));

                if (string.IsNullOrEmpty(inputType))
                    throw new ParserException("Error struct parsed");

                int semicolonBefore = symbols.TakeWhile(s => !(s is Semicolon)).Count();
                symbols = symbols.Skip(semicolonBefore + 1);
                members.Add(new Semicolon());
            }

            return new StructStatementExpression(members);
        }
    }
}
