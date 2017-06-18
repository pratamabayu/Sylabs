using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Translating
{
    public abstract class ElementDescription<T>
    {
        public T Symbol
        {
            get;
            private set;
        }

        public ElementDescription(T e)
        {
            this.Symbol = e;
            Initialize(e);
        }

        protected abstract void Initialize(T e);
    }
}
