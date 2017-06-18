using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class ScriptExpression : Symbol
    {
        public ScriptExpression(params Object[] symbols) : base(symbols) { }

        public static ScriptExpression Produce(IEnumerable<Symbol> symbols)
        {
            List<Symbol> scriptExpressions = new List<Symbol>();
            
            // Recursive parse script
            RecursiveProduce(symbols, scriptExpressions);

            return scriptExpressions == null ? null : new ScriptExpression(scriptExpressions);
        }

        static void RecursiveProduce(IEnumerable<Symbol> symbols, List<Symbol> scriptExpressions)
        {
            // Empty blank space before keyword
            ParserExtensions.RemoveBlankSpace(ref symbols);
            
            if (symbols.Count() <= 0)
                return;            

            // Get keyword
            string keyword = ParserExtensions.ExtractText(ref symbols);

            // Empty blank space after keyword
            ParserExtensions.RemoveBlankSpace(ref symbols);

            // Get name
            string name = ParserExtensions.ExtractText(ref symbols);

            // Empty blank space after name
            ParserExtensions.RemoveBlankSpace(ref symbols);            

            #region Parsing sub expression
            if (!string.IsNullOrEmpty(keyword) && !string.IsNullOrEmpty(name))
            {
                // Create text symbol for identifier
                TextPart keywordTextPart = new TextPart(keyword);
                // Create text symbol for name
                TextPart nameTextPart = new TextPart(name);

                if (keyword.StartsWith("sampler") || keyword == "struct")
                {
                    int openBracketIndex = 0;
                    int closeBracketIndex = 0;
                    if (symbols.RollUpCurlyBrackets(out openBracketIndex, out closeBracketIndex))
                    {
                        // Get body
                        List<Symbol> bodySymbols = symbols.Take(closeBracketIndex + 1).ToList();

                        // Remove body
                        symbols = symbols.Skip(closeBracketIndex + 1).ToList();

                        // Count until next semicolon
                        int beforeSemicolon = symbols.TakeWhile(s => !(s is Semicolon)).Count();
                        // Normalize
                        symbols = symbols.Skip(beforeSemicolon + 1);

                        // Combined final symbols
                        List<Symbol> samplerOrStructSymbols = new List<Symbol>();
                        samplerOrStructSymbols.Add(keywordTextPart);
                        samplerOrStructSymbols.Add(nameTextPart);
                        samplerOrStructSymbols.AddRange(bodySymbols);
                        samplerOrStructSymbols.Add(new Semicolon());

                        // Add sampler expression
                        if (keyword.StartsWith("sampler"))
                        {
                            string keywordType = keyword.Replace("sampler", "");
                            //if (keywordType != "2D" || keywordType != "Cube")
                            	//new ParserException("Unsupported " + keyword + " syntax");

                            scriptExpressions.Add(SamplerExpression.Produce(samplerOrStructSymbols));
                        }
                        else
                            // Add struct expression
                            scriptExpressions.Add(StructExpression.Produce(samplerOrStructSymbols));
                    }
                    else
                        throw new ParserException("Error roll up bracket");
                }
                else if (keyword == "technique")
                {
                    int openBracketIndex = 0;
                    int closeBracketIndex = 0;
                    if(symbols.RollUpCurlyBrackets(out openBracketIndex, out closeBracketIndex))
                    {
                        // Get body
                        List<Symbol> bodySymbols = symbols.Take(closeBracketIndex + 1).ToList();

                        // Remove body
                        symbols = symbols.Skip(closeBracketIndex + 1).ToList();

                        // Combined final symbols
                        List<Symbol> techniqueSymbols = new List<Symbol>();
                        techniqueSymbols.Add(keywordTextPart);
                        techniqueSymbols.Add(nameTextPart);
                        techniqueSymbols.AddRange(bodySymbols);

                        // Add technique expression
                        scriptExpressions.Add(TechniqueExpression.Produce(techniqueSymbols));
                    }
                    else
                        throw new ParserException("Error roll up bracket");
                }
                else
                {
                    bool isFunction = symbols.FirstOrDefault() is OpenRoundBracket;

                    if (isFunction)
                    {
                        List<Symbol> functionExpression = new List<Symbol>();

                        // Add header
                        functionExpression.Add(keywordTextPart);
                        functionExpression.Add(nameTextPart);

                        // Begin parse arguments
                        IEnumerable<Symbol> argumentSymbols = ParserExtensions.TakeSymbolInRoundBrackets(ref symbols);
                        // Add arguments
                        functionExpression.Add(new OpenRoundBracket());
                        functionExpression.Add(ArgumentExpression.Produce(argumentSymbols));
                        functionExpression.Add(new CloseRoundBracket());
                        // End parse arguments

                        // Remove blank spaces
                        ParserExtensions.RemoveBlankSpace(ref symbols);

                        // Begin parse return
                        IEnumerable<Symbol> returnSymbol = symbols.TakeWhile(s => !(s is OpenCurlyBracket));
                        int removeReturnsCount = returnSymbol.Count();
                        if (returnSymbol.FirstOrDefault() is Colon)
                        {
                            // Remove return
                            symbols = symbols.Skip(removeReturnsCount);

                            // Remove double dot
                            returnSymbol = returnSymbol.Skip(1);

                            // Remove blank spaces
                            ParserExtensions.RemoveBlankSpace(ref returnSymbol);

                            // Get return type
                            string returnType = ParserExtensions.ExtractText(ref returnSymbol);

                            // Add returns
                            functionExpression.Add(new Colon());
                            functionExpression.Add(new TextPart(returnType));

                            ParserExtensions.RemoveBlankSpace(ref returnSymbol);
                        }
                        // End parse return

                        // Remove blank spaces
                        ParserExtensions.RemoveBlankSpace(ref symbols);

                        // Begin parse body
                        IEnumerable<Symbol> memberSymbols = ParserExtensions.TakeSymbolInCurlyBrackets(ref symbols);
                        // Add function body
                        functionExpression.Add(new OpenCurlyBracket());
                        functionExpression.Add(StatementBlockExpression.Produce(memberSymbols));
                        functionExpression.Add(new CloseCurlyBracket());
                        // End parse body

                        // Produce function expression
                        scriptExpressions.Add(FunctionExpression.Produce(functionExpression));
                    }
                    else
                    {
                        // Parse fields
                        // Get symbols before semicolon
                        List<Symbol> bodySymbols = symbols.TakeWhile(s => !(s is Semicolon)).ToList();

                        // Normalize original symbols
                        symbols = symbols.Skip(bodySymbols.Count + 1);

                        // Create newSymbols
                        List<Symbol> newSymbols = new List<Symbol>();
                        // Add symbols
                        newSymbols.Add(keywordTextPart);
                        newSymbols.Add(nameTextPart);
                        // Add body symbols
                        newSymbols.AddRange(bodySymbols);
                        // Add semi colon to new Symbols
                        newSymbols.Add(new Semicolon());

                        // Parse resource
                        if (keyword.StartsWith("texture"))
                        {
                            string keywordType = keyword.Replace("texture", "");
							//if (keywordType != "2D" || keywordType != "Cube")
							    //throw new ParserException("Unsupported " + keyword + " syntax");

                            scriptExpressions.Add(ResourceExpression.Produce(newSymbols));
                        }
                        // Parse constant
                        else
                            scriptExpressions.Add(ConstantExpression.Produce(newSymbols));
                    }
                }
            }
            #endregion

            RecursiveProduce(symbols, scriptExpressions);
        }        
    }
}
