using Sylabs.Parsing;
using Sylabs.Translating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sylabs
{
    public abstract class ShaderTranslator
    {
        #region Fields
        static List<string> _defaultProcedureNames = null;

        Dictionary<string, string> _vertexSources = new Dictionary<string, string>();
        Dictionary<string, string> _pixelSources = new Dictionary<string, string>();

        List<ConstantDescription> _constantDescriptions = new List<ConstantDescription>();
        List<ResourceDescription> _resourceDescriptions = new List<ResourceDescription>();
        List<SamplerDescription> _samplerDescriptions = new List<SamplerDescription>();
        List<TechniqueDescription> _techniqueDescriptions = new List<TechniqueDescription>();
        #endregion

        #region Properties
        protected ShaderParser Parser { get; private set; }
        #endregion

        #region Constructors
        public ShaderTranslator(ShaderParser parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            this.Parser = parser;
        }
        static ShaderTranslator()
        {
            string[] defaultProcedureName = new string[] 
            { 
                "max", "min", 
                "clamp", "saturate", "mul", "normalize", "cross", "dot", "lerp", "frac", "distance", "pow", "reflect", "clip",
                "cos", "sin", "tan", "smoothstep", "length", "fmod", "abs",
                "float2", "float3", "float4", "float2x2", "float3x3", "float4x4",  
                "int2", "int3", "int4",
                "tex2D", "texCube"
            };

            _defaultProcedureNames = defaultProcedureName.ToList();
        }
        #endregion

        #region Methods
        public void Process()
        {
            // Clear all
            _vertexSources.Clear();
            _pixelSources.Clear();
            _constantDescriptions.Clear();
            _resourceDescriptions.Clear();
            _samplerDescriptions.Clear();
            _techniqueDescriptions.Clear();

            // Element pool
            List<ConstantExpression> constantSymbols = new List<ConstantExpression>();
            List<ResourceExpression> resourceSymbols = new List<ResourceExpression>();
            List<SamplerExpression> samplerSymbols = new List<SamplerExpression>();
            List<StructExpression> structSymbols = new List<StructExpression>();
            List<FunctionExpression> functionSymbols = new List<FunctionExpression>();

            if (this.Parser.Script == null)
                throw new ArgumentNullException("this.Parser.Script");

            // Populate symbols by type
            foreach (Symbol s in this.Parser.Script.ConstituentSymbols.OfType<ScriptExpression>().FirstOrDefault().ConstituentSymbols)
            {
                if (s is ConstantExpression)
                    constantSymbols.Add(s as ConstantExpression);
                else if (s is ResourceExpression)
                    resourceSymbols.Add(s as ResourceExpression);
                else if (s is SamplerExpression)
                    samplerSymbols.Add(s as SamplerExpression);
                else if (s is StructExpression)
                    structSymbols.Add(s as StructExpression);
                else if (s is FunctionExpression)
                    functionSymbols.Add(s as FunctionExpression);
                else if (s is TechniqueExpression)
                    _techniqueDescriptions.Add(new TechniqueDescription(s as TechniqueExpression));
            }

            // Do align constants to sixteen bytes packed rule
            if (AlignSixteenBytes())
                constantSymbols = ArrangeConstants(constantSymbols);

            // Create descriptions
            // For constants
            foreach(var result in constantSymbols)
                _constantDescriptions.Add(new ConstantDescription(result));
            // For resources
            foreach(var result in resourceSymbols)
                _resourceDescriptions.Add(new ResourceDescription(result));
            // For samplers
            foreach(var result in samplerSymbols)
                _samplerDescriptions.Add(new SamplerDescription(result));

            // Split script to each vertex and pixel collection symbol by technique and pass
            List<VertexShaderMetadata> vertexCollection = new List<VertexShaderMetadata>();
            List<PixelShaderMetadata> pixelCollection = new List<PixelShaderMetadata>();
            foreach (TechniqueDescription td in _techniqueDescriptions)
            {
                foreach (PassDescription pd in td.Passes)
                {
                    // Vertex
                    if (vertexCollection.Find(m => m.Name == pd.VertexShader) == null)
                    {
                        FunctionExpression mainFunction = functionSymbols.Find(f => f.Name == pd.VertexShader);
                        if (mainFunction == null)
                            throw new ArgumentNullException("\"" + pd.VertexShader + "\" VertexShader is not found");

                        // Get available semantics
                        List<StructExpression> semantics = null;
                        GetSemanticsFrom(mainFunction, structSymbols, ref semantics);
                        // Get extended functions
                        List<FunctionExpression> extendedFunctions = null;
                        List<string> usageProcedures = new List<string>();
                        GetExtendedFunctionsFrom(mainFunction, functionSymbols, ref extendedFunctions, ref usageProcedures);
                        if (extendedFunctions != null)
                            foreach (var functionSymbol in extendedFunctions)
                                GetExtendedFunctionsFrom(functionSymbol, functionSymbols, ref extendedFunctions, ref usageProcedures);

                        // Set to
                        vertexCollection.Add(
                            new VertexShaderMetadata(pd.VertexShader, mainFunction, 
                                constantSymbols, semantics, extendedFunctions, usageProcedures));
                    }

                    // Pixel
                    if (pixelCollection.Find(m => m.Name == pd.PixelShader) == null)
                    {
                        FunctionExpression mainFunction = functionSymbols.Find(f => f.Name == pd.PixelShader);
                        if (mainFunction == null)
                            throw new ArgumentNullException("\"" + pd.PixelShader + "\" PixelShader is not found");

                        List<Symbol> elementExpression = new List<Symbol>();

                        // Get available semantics
                        List<StructExpression> semantics = null;
                        GetSemanticsFrom(mainFunction, structSymbols, ref semantics);
                        // Get extended functions
                        List<FunctionExpression> extendedFunctions = null;
                        List<string> usageProcedures = new List<string>();
                        GetExtendedFunctionsFrom(mainFunction, functionSymbols, ref extendedFunctions, ref usageProcedures);
                        if(extendedFunctions != null)
                            foreach (var functionSymbol in extendedFunctions)
                                GetExtendedFunctionsFrom(functionSymbol, functionSymbols, ref extendedFunctions, ref usageProcedures);

                        // Set to
                        pixelCollection.Add(
                            new PixelShaderMetadata(pd.PixelShader, mainFunction,
                                constantSymbols, resourceSymbols, samplerSymbols, semantics, extendedFunctions, usageProcedures));
                    }
                }
            }

            // Internal process, depend on target platform
            foreach (var result in vertexCollection)
            {
                string output = string.Empty;
                OnVertexProcess(result, out output);

                _vertexSources.Add(result.Name, output);
            }
            foreach (var result in pixelCollection)
            {
                string output = string.Empty;
                OnPixelProcess(result, out output);

                _pixelSources.Add(result.Name, output);
            }
        }
        protected abstract void OnVertexProcess(VertexShaderMetadata metadata, out string output);
        protected abstract void OnPixelProcess(PixelShaderMetadata metadata, out string output);

		public byte[] Build(bool optimize)
        {
            byte[] output = null;

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // Write lang
                    writer.Write(GetLang());

                    // Write constant count
                    writer.Write(_constantDescriptions.Count);
                    // Write constants
                    foreach (var item in _constantDescriptions)
                    {
                        writer.Write(item.Type);
                        writer.Write(item.Dimension[0]); // Cols
                        writer.Write(item.Dimension[1]); // Rows
                        writer.Write(item.Name);
                        writer.Write(item.ArraySize);
                        writer.Write(item.Value);
                    }

                    // Write resource count
                    writer.Write(_resourceDescriptions.Count);
                    // Write resources
                    foreach (var item in _resourceDescriptions)
                    {
                        writer.Write(item.Type);
                        writer.Write(item.Name);
                    }

                    // Write sampler count
                    writer.Write(_samplerDescriptions.Count);
                    // Write samplers
                    foreach (var item in _samplerDescriptions)
                    {
                        writer.Write(item.Type);
                        writer.Write(item.Name);
                        writer.Write(item.Resource);
                        writer.Write(item.Filter);
                        writer.Write(item.AddressU);
                        writer.Write(item.AddressV);
                        writer.Write(item.AddressW);
                    }

                    // Populate compiled shader
                    Dictionary<string, byte[]> compiledVertexCollection = new Dictionary<string, byte[]>();
                    Dictionary<string, byte[]> compiledPixelCollection = new Dictionary<string, byte[]>();
                    foreach (var input in _vertexSources)
                    {
                        byte[] result = null;
						OnVertexBuild(input.Key, input.Value, optimize, out result);
                        compiledVertexCollection.Add(input.Key, result);
                    }
                    foreach (var input in _pixelSources)
                    {
                        byte[] result = null;
						OnPixelBuild(input.Key, input.Value, optimize, out result);
                        compiledPixelCollection.Add(input.Key, result);
                    }
                    // Write compiled vertex shader count
                    writer.Write(compiledVertexCollection.Count);
                    // Write compiled vertex shaders
                    foreach (var item in compiledVertexCollection)
                    {
                        writer.Write(item.Key);
                        writer.Write(item.Value.Length);
                        writer.Write(item.Value);
                    }
                    // Write compiled vertex shader count
                    writer.Write(compiledPixelCollection.Count);
                    // Write compiled vertex shaders
                    foreach (var item in compiledPixelCollection)
                    {
                        writer.Write(item.Key);
                        writer.Write(item.Value.Length);
                        writer.Write(item.Value);
                    }

                    // Write technique count
                    writer.Write(_techniqueDescriptions.Count);
                    // Write techniques
                    foreach (var item in _techniqueDescriptions)
                    {
                        writer.Write(item.Name);
                        // Write pass count
                        writer.Write(item.Passes.Count);
                        // Write passes
                        foreach (var pass in item.Passes)
                        {
                            writer.Write(pass.Name);
                            writer.Write(pass.VertexShader);
                            writer.Write(pass.PixelShader);
                        }
                    }
                }

                output = stream.ToArray();
            }

            return output;
        }

		protected abstract void OnVertexBuild(string entryPoint, string source, bool optimize, out byte[] output);
		protected abstract void OnPixelBuild(string entryPoint, string source, bool optimize, out byte[] output);

        public string ShowOutput()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("## Lang : " + GetLang() + " ##");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("## Begin Vertex Collection ##");
            stringBuilder.Append(Environment.NewLine);
            int i = 0;
            foreach (var result in _vertexSources)
            {
                stringBuilder.Append("# Name : " + result.Key + " #");
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(result.Value);
                if(i < _vertexSources.Count - 1)
                    stringBuilder.Append(Environment.NewLine);

                i++;
            }
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("## End Vertex Collection ##");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("## Begin Pixel Collection ##");
            stringBuilder.Append(Environment.NewLine);
            i = 0;
            foreach (var result in _pixelSources)
            {
                stringBuilder.Append("# Name : " + result.Key + " #");
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(result.Value);
                if (i < _pixelSources.Count - 1)
                    stringBuilder.Append(Environment.NewLine);

                i++;
            }
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("## End Pixel Collection ##");

            return stringBuilder.ToString();
        }

        protected abstract string GetLang();
        protected virtual bool AlignSixteenBytes()
        {
            return true;
        }

        #region Helpers
        static void GetSemanticsFrom(FunctionExpression mainFunction, List<StructExpression> semanticCollection, ref List<StructExpression> semantics)
        {
            // Input
            ArgumentExpression functionArgument = mainFunction.ConstituentSymbols[3] as ArgumentExpression;
            if (functionArgument.ConstituentSymbols.Count != 2)
                throw new ArgumentOutOfRangeException("Argument expression for " + mainFunction.Name);
            TextPart inputSemanticType = functionArgument.ConstituentSymbols[0] as TextPart;
            if (inputSemanticType != null)
            {
                var result = semanticCollection.Find(s => s.Name == inputSemanticType.TextValue);
                if (result == null)
                    throw new ArgumentNullException("\"" + inputSemanticType.TextValue + "\" semantic");

                semantics = new List<StructExpression>();
                semantics.Add(result);
            }
            else
                throw new ArgumentNullException("Input semantic for " + mainFunction.Name);

            // Output
            TextPart outputSemanticType = mainFunction.ConstituentSymbols[0] as TextPart;
            if (outputSemanticType != null && outputSemanticType.TextValue != "float4")
            {
                var result = semanticCollection.Find(s => s.Name == outputSemanticType.TextValue);
                if (result == null)
                    throw new ArgumentNullException("\"" + outputSemanticType.TextValue + "\" semantic");

                semantics.Add(result);
            }
        }
        static void GetExtendedFunctionsFrom(FunctionExpression function, List<FunctionExpression> functionCollection, ref List<FunctionExpression> extendedFunctions, ref List<string> usageProcedures)
        {
            StatementBlockExpression result = function.ConstituentSymbols.Find(s => s is StatementBlockExpression) as StatementBlockExpression;
            if (result != null)
                GetExtendedFunctionsFrom(result, functionCollection, ref extendedFunctions, ref usageProcedures);
        }
        static void GetExtendedFunctionsFrom(StatementBlockExpression statementBlock, List<FunctionExpression> functionCollection, ref List<FunctionExpression> extendedFunctions, ref List<string> usageProcedures)
        {
            foreach (var element in statementBlock.ConstituentSymbols)
            {
                if (element is StatementExpression)
                {
                    GetExtendedFunctionsFrom(element as Symbol, functionCollection, ref extendedFunctions, ref usageProcedures);
                }
                else if (element is StatementBlockExpression)
                    GetExtendedFunctionsFrom(element as StatementBlockExpression, functionCollection, ref extendedFunctions, ref usageProcedures);
                else if (element is IfStatementExpression ||element is ForStatementExpression )
                {
                    var statementBlocks = element.ConstituentSymbols.FindAll(e => e is StatementBlockExpression);
                    foreach (var s in statementBlocks)
                    {
                        GetExtendedFunctionsFrom(s as StatementBlockExpression, functionCollection, ref extendedFunctions, ref usageProcedures);
                    }
                }
            }
        }
        static void GetExtendedFunctionsFrom(Symbol element, List<FunctionExpression> functionCollection, ref List<FunctionExpression> extendedFunctions, ref List<string> usageProcedures)
        {
            int count = element.ConstituentSymbols.Count;
            for (int i = 0; i < count; i++)
            {
                Symbol s = element.ConstituentSymbols[i];

                if (s is ProcedureExpression)
                {
                    var procedurePart = s as ProcedureExpression;
                    if (!usageProcedures.Contains(procedurePart.Name))
                        usageProcedures.Add(procedurePart.Name);

                    if (_defaultProcedureNames.Find(e => e == procedurePart.Name) == null)
                    {
                        if (extendedFunctions == null)
                            extendedFunctions = new List<FunctionExpression>();

                        if (extendedFunctions.Find(e => e.Name == procedurePart.Name) == null)
                        {
                            FunctionExpression result = functionCollection.Find(f => f.Name == procedurePart.Name);
                            if (result == null)
                                throw new ArgumentNullException(procedurePart.Name + " function");

                            extendedFunctions.Add(result);
                        }
                    }

                    // Deep search
                    GetExtendedFunctionsFrom(procedurePart as Symbol, functionCollection, ref extendedFunctions, ref usageProcedures);
                }
            }
        }

        static List<ConstantExpression> ArrangeConstants(List<ConstantExpression> input)
        {
            List<ConstantExpression> output = new List<ConstantExpression>();

            #region Linear Arrange
            List<ConstantExpression> part1 = null;
            try
            {
                part1 = input.FindAll(c => c.Dimension[0] == 4 && c.ArraySize == 0);
            }
            catch { }
            List<ConstantExpression> part2 = null;
            try
            {
                part2 = input.FindAll(c => c.Dimension[0] == 4 && c.ArraySize > 0);
            }
            catch { }
            List<ConstantExpression> part3 = null;
            try
            {
                part3 = input.FindAll(c => c.Dimension[0] == 3 && c.ArraySize == 0);
            }
            catch { }
            List<ConstantExpression> part4 = null;
            try
            {
                part4 = input.FindAll(c => c.Dimension[0] == 3 && c.ArraySize > 0);
            }
            catch { }
            List<ConstantExpression> part5 = null;
            try
            {
                part5 = input.FindAll(c => c.Dimension[0] == 2 && c.ArraySize == 0);
            }
            catch { }
            List<ConstantExpression> part6 = null;
            try
            {
                part6 = input.FindAll(c => c.Dimension[0] == 2 && c.ArraySize > 0);
            }
            catch { }
            List<ConstantExpression> part7 = null;
            try
            {
                part7 = input.FindAll(c => c.Dimension[0] == 1 && c.ArraySize == 0);
            }
            catch { }
            List<ConstantExpression> part8 = null;
            try
            {
                part8 = input.FindAll(c => c.Dimension[0] == 1 && c.ArraySize > 0);
            }
            catch { }

            input.Clear();
            if (part1 != null)
                input.AddRange(part1);
            if (part2 != null)
                input.AddRange(part2);
            if (part3 != null)
                input.AddRange(part3);
            if (part4 != null)
                input.AddRange(part4);
            if (part1 != null)
                input.AddRange(part5);
            if (part2 != null)
                input.AddRange(part6);
            if (part3 != null)
                input.AddRange(part7);
            if (part4 != null)
                input.AddRange(part8);
            #endregion

            #region Cross Arrange
            while (input.Count > 0)
            {
                // Get first element
                var result1 = input[0];
                var result2 = input.Count > 1 ? input[1] : null;

                if (result1.ArraySize > 0)
                {
                    // Add to output
                    output.Add(result1);
                    // Remove from input
                    input.Remove(result1);
                }
                else
                {
                    if (result1.Dimension[0] == 4)
                    {
                        // Add to output
                        output.Add(result1);
                        // Remove from input
                        input.Remove(result1);
                    }
                    else if (result1.Dimension[0] == 3)
                    {
                        // Add to output
                        output.Add(result1);
                        // Remove from input
                        input.Remove(result1);

                        if (result2 != null && result2.Dimension[0] == 1 && result2.ArraySize == 0)
                        {
                            // Add to output
                            output.Add(result2);
                            // Remove from input
                            input.Remove(result2);
                        }
                        else if (result2 != null)
                        {
                            // Find next result with have one dimension
                            for (int i = 1; i < input.Count; i++)
                            {
                                if (input[i].Dimension[0] == 1 &&
                                    input[i].ArraySize == 0)
                                {
                                    result2 = input[i];
                                    break;
                                }
                            }

                            // Add and remove
                            if (result2 != null && result2.Dimension[0] == 1 && result2.ArraySize == 0)
                            {
                                // Add to output
                                output.Add(result2);
                                // Remove from input
                                input.Remove(result2);
                            }
                        }
                    }
                    else if (result1.Dimension[0] == 2)
                    {
                        // Add to output
                        output.Add(result1);
                        // Remove from input
                        input.Remove(result1);

                        if (result2 != null && result2.Dimension[0] == 2 && result2.ArraySize == 0)
                        {
                            // Add to output
                            output.Add(result2);
                            // Remove from input
                            input.Remove(result2);
                        }
                        else if (result2 != null)
                        {
                            // Find next result with have two dimension
                            for (int i = 1; i < input.Count; i++)
                            {
                                if (input[i].Dimension[0] == 2 && input[i].ArraySize == 0)
                                {
                                    result2 = input[i];
                                    break;
                                }
                            }

                            // Add and remove
                            if (result2 != null && result2.Dimension[0] == 2 && result2.ArraySize == 0)
                            {
                                // Add to output
                                output.Add(result2);
                                // Remove from input
                                input.Remove(result2);
                            }
                            else
                            {
                                ConstantExpression result3 = null;
                                // Find next result with have one dimension twice
                                for (int i = 1; i < input.Count; i++)
                                {
                                    if (input[i].Dimension[0] == 1 && input[i].ArraySize == 0)
                                    {
                                        if (input[i].Dimension[0] == 1 && input[i].ArraySize == 0 &&
                                            result2.Dimension[0] == 1 && result2.ArraySize == 0)
                                        {
                                            result3 = input[i];
                                            break;
                                        }
                                        else
                                            result2 = input[i];
                                    }
                                }

                                if (result3 != null && result3.Dimension[0] == 1 && result3.ArraySize == 0)
                                {
                                    // Add to output
                                    output.Add(result2);
                                    output.Add(result3);
                                    // Remove from input
                                    input.Remove(result2);
                                    input.Remove(result3);
                                }
                            }
                        }
                    }
                    else if (result1.Dimension[0] == 1)
                    {
                        // Add to output
                        output.Add(result1);
                        // Remove from input
                        input.Remove(result1);

                        if (result2 != null && result2.Dimension[0] == 3 && result2.ArraySize == 0)
                        {
                            // Add to output
                            output.Add(result2);
                            // Remove from input
                            input.Remove(result2);
                        }
                        else if (result2 != null)
                        {
                            // Find next result with have three dimension
                            for (int i = 1; i < input.Count; i++)
                            {
                                if (input[i].Dimension[0] == 3 && input[i].ArraySize == 0)
                                {
                                    result2 = input[i];
                                    break;
                                }
                            }

                            // Add and remove
                            if (result2 != null && result2.Dimension[0] == 3 && result2.ArraySize == 0)
                            {
                                // Add to output
                                output.Add(result2);
                                // Remove from input
                                input.Remove(result2);
                            }
                        }
                    }
                }
            }
            #endregion

            return output;
        }
        #endregion
        #endregion
    }
}
