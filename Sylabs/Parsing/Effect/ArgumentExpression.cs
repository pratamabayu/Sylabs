using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class ArgumentExpression : Symbol
    {
        public ArgumentExpression(params Object[] symbols) : base(symbols) { }

        public static ArgumentExpression Produce(IEnumerable<Symbol> symbols)
        {
            List<Symbol> arguments = new List<Symbol>();
            RecursiveProduceArgument(ref symbols, arguments);

            return symbols == null ? null : new ArgumentExpression(arguments);
        }

        static void RecursiveProduceArgument(ref IEnumerable<Symbol> symbols, List<Symbol> arguments)
        {
            ParserExtensions.RemoveBlankSpace(ref symbols);

            if (symbols.Count() <= 0)
                return;

            string keyword = ParserExtensions.ExtractText(ref symbols);
            ParserExtensions.RemoveBlankSpace(ref symbols);
            string name = ParserExtensions.ExtractText(ref symbols);

            if (!string.IsNullOrEmpty(keyword) && !string.IsNullOrEmpty(name))
            {
                arguments.Add(new TextPart(keyword));
                arguments.Add(new TextPart(name));
            }

            ParserExtensions.RemoveBlankSpace(ref symbols);
            if (symbols.FirstOrDefault() is Comma)
            {
                symbols = symbols.Skip(1);

                arguments.Add(new Comma());
                RecursiveProduceArgument(ref symbols, arguments);
            }
        }
    }
}
