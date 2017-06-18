using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class CompileExpression : Symbol
    {
        public CompileExpression(params Object[] symbols) : base(symbols) { }

        public static CompileExpression Produce(IEnumerable<Symbol> symbols)
        {
            return symbols == null ? null : new CompileExpression(Extract(symbols));
        }

        static IEnumerable<Symbol> Extract(IEnumerable<Symbol> symbols)
        {
            // Empty blank space before keyword
            ParserExtensions.RemoveBlankSpace(ref symbols);

            List<Symbol> parts = new List<Symbol>();

            if (symbols.Count() <= 0)
                return parts;            

            // Get keyword
            string keyword = ParserExtensions.ExtractText(ref symbols);

            if (!string.IsNullOrEmpty(keyword) &&
                (keyword == "SetVertexShader" || keyword == "SetPixelShader"))
            {
                // Add Set{N}Shader
                parts.Add(new TextPart(keyword));

                // Empty blank space before keyword
                ParserExtensions.RemoveBlankSpace(ref symbols);

                int openParenthesisIndex = 0;
                int closeParenthesisIndex = 0;
                if (symbols.RollUpRoundBrackets(out openParenthesisIndex, out closeParenthesisIndex))
                {
                    // Get body
                    IEnumerable<Symbol> bodySymbols = symbols.Take(closeParenthesisIndex + 1).ToList();
                    bodySymbols = bodySymbols.Skip(1).SkipLast(1);

                    // Remove body
                    symbols = symbols.Skip(closeParenthesisIndex + 1).ToList();

                    parts.Add(new OpenRoundBracket());
                    ExtractSetShader(bodySymbols, parts);
                    parts.Add(new CloseRoundBracket());
                }

                parts.AddRange(symbols);
            }
            else
                throw new ParserException("SetVertexShader or SetPixelShader is not found");

            return parts;
        }

        static void ExtractSetShader(IEnumerable<Symbol> symbols, List<Symbol> parts)
        {
            // Empty blank space before keyword
            ParserExtensions.RemoveBlankSpace(ref symbols);

            if (symbols.Count() <= 0)
                return;            

            // Get keyword
            string keyword = ParserExtensions.ExtractText(ref symbols);

            if (!string.IsNullOrEmpty(keyword))
            {
                // Add Vertex or Pixel / Fragment function name
                parts.Add(new TextPart(keyword));

                // Empty blank space after keyword
                ParserExtensions.RemoveBlankSpace(ref symbols);
            }
        }        
    }
}
