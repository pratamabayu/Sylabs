using Sylabs.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Translating
{
    public sealed class TechniqueDescription : ElementDescription<TechniqueExpression>
    {
        #region Properties
        public string Name
        {
            get;
            private set;
        }

        public List<PassDescription> Passes
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        public TechniqueDescription(TechniqueExpression e)
            : base(e)
        {
        }
        #endregion

        #region Methods
        protected override void Initialize(TechniqueExpression e)
        {
            // Get data
            IEnumerable<Symbol> symbols = e.ConstituentSymbols;

            // Get name part
            TextPart namePart = symbols.ElementAt(1) as TextPart;
            this.Name = namePart.ToString();

            // Create passes
            IEnumerable<PassExpression> passesExpr = symbols.OfType<PassExpression>();
            if (passesExpr != null && passesExpr.Count() > 0)
            {
                this.Passes = new List<PassDescription>();
                foreach (Symbol s in passesExpr)
                    this.Passes.Add(new PassDescription((PassExpression)s));
            }
            else
                throw new ArgumentException("PassExpression is not found");
        }
        #endregion
    }
}
