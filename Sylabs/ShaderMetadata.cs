using Sylabs.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs
{
    public abstract class ShaderMetadata
    {
        public string Name
        {
            get;
            private set;
        }
        public List<ConstantExpression> Constants
        {
            get;
            private set;
        }        
        public List<StructExpression> Semantics
        {
            get;
            private set;
        }
        public List<FunctionExpression> ExtendedFunctions
        {
            get;
            private set;
        }
        
        public FunctionExpression EntryPoint
        {
            get;
            private set;
        }

        public List<string> UsageProcedures
        {
            get;
            private set;
        }

        public ShaderMetadata(string name, FunctionExpression entryPoint, List<ConstantExpression> constants, List<StructExpression> structs, List<FunctionExpression> extendedFunctions, List<string> usageProcedures)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            this.Name = name;

            if (entryPoint == null)
                throw new ArgumentNullException("entryPoint");
            else if (string.IsNullOrEmpty(entryPoint.Name))
                throw new ArgumentNullException("entryPoint.Name");
            this.EntryPoint = entryPoint;

            if (this.Name != this.EntryPoint.Name)
                throw new ArgumentException("this.Name != this.EntryPoint.Name");

            this.Constants = constants;            
            this.Semantics = structs;
            this.ExtendedFunctions = extendedFunctions;
            this.UsageProcedures = usageProcedures;
        }
    }

    public sealed class VertexShaderMetadata : ShaderMetadata
    {
        public VertexShaderMetadata(string name, FunctionExpression entryPoint, List<ConstantExpression> constants, 
            List<StructExpression> structs, List<FunctionExpression> extendedFunctions, List<string> usageProcedures)
            : base(name, entryPoint, constants, structs, extendedFunctions, usageProcedures)
        {
        }
    }

    public sealed class PixelShaderMetadata : ShaderMetadata
    {
        public List<ResourceExpression> Resources
        {
            get;
            private set;
        }
        public List<SamplerExpression> Samplers
        {
            get;
            private set;
        }

        public PixelShaderMetadata(string name, FunctionExpression entryPoint, List<ConstantExpression> constants, 
            List<ResourceExpression> resources, List<SamplerExpression> samplers, List<StructExpression> structs,
            List<FunctionExpression> extendedFunctions, List<string> usageProcedures)
            : base(name, entryPoint, constants, structs, extendedFunctions, usageProcedures)
        {
            this.Resources = resources;
            this.Samplers = samplers;
        }
    }
}
