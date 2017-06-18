using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Parsing
{
    public class StatementBlockExpression : Symbol
    {
        public StatementBlockExpression(params Object[] symbols) : base(symbols) { }

        public static StatementBlockExpression Produce(IEnumerable<Symbol> symbols)
        {
            List<Symbol> members = new List<Symbol>();
            RecursiveProduce(symbols, members);

            return symbols == null ? null : new StatementBlockExpression(members);
        }

        static void RecursiveProduce(IEnumerable<Symbol> symbols, List<Symbol> members)
        {
            ParserExtensions.RemoveBlankSpace(ref symbols);

            if (symbols.Count() <= 0)
                return;

            string identify = ParserExtensions.GetFirstText(ref symbols);

            if (!string.IsNullOrEmpty(identify))
            {               
                // Checking .. 
                // 1. If statement
                if (identify == "if")
                {
                    List<Symbol> ifExpression = new List<Symbol>();

                    IfStatementProduce(ref symbols, ifExpression, identify);

                    // Produce if expression
                    members.Add(IfStatementExpression.Produce(ifExpression));
                }
                // 2. For statement
                else if (identify == "for")
                {
                    List<Symbol> forExpression = new List<Symbol>();

                    // Normalize
                    symbols = symbols.Skip(identify.Length);

                    // Remove blank spaces
                    ParserExtensions.RemoveBlankSpace(ref symbols);

                    // Add syntax
                    forExpression.Add(new TextPart(identify));

                    // Begin parse arguments
                    IEnumerable<Symbol> argumentSymbols = ParserExtensions.TakeSymbolInRoundBrackets(ref symbols);
                    // Add arguments
                    forExpression.Add(new OpenRoundBracket());
                    forExpression.Add(ForHeaderExpression.Produce(argumentSymbols));
                    forExpression.Add(new CloseRoundBracket());
                    // End parse arguments

                    // Remove blank spaces
                    ParserExtensions.RemoveBlankSpace(ref symbols);

                    // Begin parse body
                    IEnumerable<Symbol> bodySymbols = ParserExtensions.TakeSymbolInCurlyBrackets(ref symbols);
                    // Add function body
                    forExpression.Add(new OpenCurlyBracket());
                    forExpression.Add(StatementBlockExpression.Produce(bodySymbols));
                    forExpression.Add(new CloseCurlyBracket());
                    // End parse body

                    // Produce if expression
                    members.Add(ForStatementExpression.Produce(forExpression));
                }
                // 3. Other 
                else
                {
                    int semicolonBefore = symbols.TakeWhile(s => !(s is Semicolon)).Count();
                    // Get
                    IEnumerable<Symbol> memberPartSymbols = symbols.Take(semicolonBefore + 1);
                    // Empty
                    symbols = symbols.Skip(semicolonBefore + 1);

                    switch (identify)
                    {
                        case "clip":
                            members.Add(ClipStatementExpression.Produce(memberPartSymbols));
                            break;
                        case "break":
                            members.Add(BreakStatementExpression.Produce(memberPartSymbols));
                            break;
                        case "continue":
                            members.Add(ContinueStatementExpression.Produce(memberPartSymbols));
                            break;
                        case "discard":
                            members.Add(DiscardStatementExpression.Produce(memberPartSymbols));
                            break;
                        case "return":
                            members.Add(ReturnStatementExpression.Produce(memberPartSymbols));
                            break;
                        default:
                            members.Add(StatementExpression.Produce(memberPartSymbols));
                            break;
                    }
                }
            }

            RecursiveProduce(symbols, members);
        }

        static void IfStatementProduce(ref IEnumerable<Symbol> symbols, List<Symbol> ifExpression, string identify)
        {
            // Normalize
            symbols = symbols.Skip(identify.Length);

            // Remove blank spaces
            ParserExtensions.RemoveBlankSpace(ref symbols);

            // Add syntax
            ifExpression.Add(new TextPart(identify));

            bool skipHeaderParsed = false;

            if (identify == "else")
            {
                string identify2 = ParserExtensions.GetFirstText(ref symbols);

                if (!string.IsNullOrEmpty(identify2))
                {
                    if (identify2 != "if")
                        throw new ParserException("Error parsed if statement");

                    // Normalize
                    symbols = symbols.Skip(identify2.Length);

                    // Remove blank spaces
                    ParserExtensions.RemoveBlankSpace(ref symbols);

                    // Add syntax
                    ifExpression.Add(new TextPart(identify2));
                }
                else
                    skipHeaderParsed = true;
            }

            if (!skipHeaderParsed)
            {
                // Begin parse header
                IEnumerable<Symbol> argumentSymbols = ParserExtensions.TakeSymbolInRoundBrackets(ref symbols);
                // Add arguments
                ifExpression.Add(new OpenRoundBracket());
                ifExpression.Add(IfHeaderExpression.Produce(argumentSymbols));
                ifExpression.Add(new CloseRoundBracket());
                // End parse header                
            }

            // Remove blank spaces
            ParserExtensions.RemoveBlankSpace(ref symbols);

            // Begin parse body
            IEnumerable<Symbol> bodySymbols = ParserExtensions.TakeSymbolInCurlyBrackets(ref symbols);
            // Add function body
            ifExpression.Add(new OpenCurlyBracket());
            ifExpression.Add(StatementBlockExpression.Produce(bodySymbols));
            ifExpression.Add(new CloseCurlyBracket());
            // End parse body

            // Remove blank spaces
            ParserExtensions.RemoveBlankSpace(ref symbols);

            // Find again
            if (!skipHeaderParsed)
            {
                if (identify == "if" || identify == "else")
                {
                    string nextIdentify = ParserExtensions.GetFirstText(ref symbols);

                    if(nextIdentify == "else")
                        IfStatementProduce(ref symbols, ifExpression, nextIdentify);
                }
            }
        }
    }
}
