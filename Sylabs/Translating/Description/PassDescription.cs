using Sylabs.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Translating
{
    public sealed class PassDescription : ElementDescription<PassExpression>
    {
        #region Properties
        public string Name
        {
            get;
            private set;
        }
        
        public string VertexShader
        {
            get;
            private set;
        }
        public string PixelShader
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        public PassDescription(PassExpression e)
            : base(e)
        {
        }
        #endregion

        #region Methods
        protected override void Initialize(PassExpression e)
        {
            // Get data
            IEnumerable<Symbol> symbols = e.ConstituentSymbols;

            // Get name part
            TextPart namePart = symbols.ElementAt(1) as TextPart;
            this.Name = namePart.ToString();

            // Create compile
            IEnumerable<CompileExpression> compilesExpr = symbols.OfType<CompileExpression>();
            if (compilesExpr != null && compilesExpr.Count() == 2)
            {
                foreach (Symbol s in compilesExpr)
                {
                    // get compile expression
                    IEnumerable<Symbol> compile = s.ConstituentSymbols;

                    // Get compile shader type
                    string compileShaderType = compile.ElementAt(0).ToString();

                    if (compileShaderType.StartsWith("SetVertexShader"))
                    {
                        this.VertexShader = compile.ElementAt(2).ToString();
                    }
                    else if (compileShaderType.StartsWith("SetPixelShader"))
                    {
                        this.PixelShader = compile.ElementAt(2).ToString();
                    }
                    else
                        throw new ArgumentException("Unknown " + compileShaderType);
                }
            }
            else
                throw new ArgumentException("Compile shader types is not found");
        }
        #endregion
    }
}
