using Sylabs.Optimizer;
using Sylabs.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sylabs.Translating
{
    public sealed class GlslEs_100_Translator : ShaderTranslator
    {
        #region Fields
        // NOTE
        // 1. Failed = reserved keyword 'output' on mobile device like android

        static List<VariableCache> _shaderVarMap = new List<VariableCache>();
        static Dictionary<string, List<VertexShaderAttribute>> _vertexShaderAttributes = new Dictionary<string, List<VertexShaderAttribute>>();

        static readonly char DotChar = '.';
		static readonly string FloatString = "f";
		static readonly string SignaturePositionString = "POSITION";
		static readonly string SignaturePosition0String = "POSITION0";
		static readonly string SignatureColorString = "COLOR";
		static readonly string SignatureDepthString = "DEPTH";
		static readonly string SignatureDepth0String = "DEPTH0";
        #endregion

        #region Constructors
        public GlslEs_100_Translator(ShaderParser parser)
            : base(parser)
        {
            _vertexShaderAttributes.Clear();
        }
        #endregion

        #region Methods
        protected override void OnVertexProcess(VertexShaderMetadata metadata, out string output)
        {
            _shaderVarMap.Clear();

            StringBuilder builder = new StringBuilder();

            builder.Append("#version 100");
            builder.Append(Environment.NewLine);

            ConstantsProcessor(builder, metadata);
            builder.Append(Environment.NewLine);
            SemanticsProcessor(builder, metadata);
            builder.Append(Environment.NewLine);
            if (metadata.ExtendedFunctions != null && metadata.ExtendedFunctions.Count > 0)
            {
                ExtendedFunctionsProcessor(builder, metadata);
                builder.Append(Environment.NewLine);
                builder.Append(Environment.NewLine);
            }
            EntryPointProcessor(builder, metadata);

            output = builder.ToString();
        }
        
        protected override void OnPixelProcess(PixelShaderMetadata metadata, out string output)
        {
            _shaderVarMap.Clear();

            StringBuilder builder = new StringBuilder();

            builder.Append("#version 100");
            builder.Append(Environment.NewLine);
            builder.Append("precision mediump float;");
            builder.Append(Environment.NewLine);

            ConstantsProcessor(builder, metadata);
            builder.Append(Environment.NewLine);
            ResourcesProcessor(builder, metadata);
            builder.Append(Environment.NewLine);
            SemanticsProcessor(builder, metadata);
            builder.Append(Environment.NewLine);
            if (metadata.ExtendedFunctions != null && metadata.ExtendedFunctions.Count > 0)
            {
                ExtendedFunctionsProcessor(builder, metadata);
                builder.Append(Environment.NewLine);
                builder.Append(Environment.NewLine);
            }
            EntryPointProcessor(builder, metadata);

            output = builder.ToString();
        }

        #region Process Helpers
        static void ConstantsProcessor(StringBuilder stringBuilder, ShaderMetadata metadata)
        {
            if (metadata.Constants == null)
                return;

            foreach (var expression in metadata.Constants)
            {
                // Set flag
                stringBuilder.Append("uniform ");

                // Write type
                string conversionType = ConvertTypeByDimension(expression.Type, expression.Dimension[0], expression.Dimension[1]);
                stringBuilder.Append(conversionType);
                // Write dimensions
                if (expression.Dimension[0] > 1)
                {
                    stringBuilder.Append(expression.Dimension[0]);
                }
                // Write name
                stringBuilder.Append(" ");
                stringBuilder.Append(expression.Name);
                // Write array
                if (expression.ArraySize > 0)
                    stringBuilder.Append("[" + expression.ArraySize + "]");

                // Write value

                // Finalize
                stringBuilder.Append(";");
                stringBuilder.Append(Environment.NewLine);

                // Add to shader var map
                AddVariable(0, CombineType(expression.Type, expression.Dimension[0], expression.Dimension[1]), expression.Name, _shaderVarMap);
            }

			// Add position fixup variable
			stringBuilder.Append("uniform vec4 PositionFixup;");
			stringBuilder.Append(Environment.NewLine);
        }
        
        static void ResourcesProcessor(StringBuilder stringBuilder, PixelShaderMetadata metadata)
        {
            if (metadata.Resources == null)
                return;

            foreach (var expression in metadata.Resources)
            {
                // Set flag
                stringBuilder.Append("uniform ");

                // Write type
                stringBuilder.Append(ConvertResource(expression.Type));

                // Write name
                stringBuilder.Append(" ");
                stringBuilder.Append(expression.Name);

                // Finalize
                stringBuilder.Append(";");
                stringBuilder.Append(Environment.NewLine);

                // Add to shader var map
                AddVariable(0, expression.Type, expression.Name, _shaderVarMap);
            }
        }
        static void SemanticsProcessor(StringBuilder stringBuilder, ShaderMetadata metadata)
        {
            if (metadata.Semantics == null)
                return;

            // Get entry point
            FunctionExpression entryPoint = metadata.EntryPoint;

            // Prepare
            string inputName = (((ArgumentExpression)entryPoint.ConstituentSymbols[3]).ConstituentSymbols[0] as TextPart).TextValue;
            string outputName = (entryPoint.ConstituentSymbols[0] as TextPart).TextValue;
            bool isFragment = metadata is PixelShaderMetadata;

            for (int i = 0; i < metadata.Semantics.Count; i++)
            {
                // Get 
                StructExpression item = metadata.Semantics[i];

                if (item.Name == inputName)
                {
                    // Get struct statement block
                    StructStatementBlockExpression block = item.ConstituentSymbols.Find(s => s is StructStatementBlockExpression) as StructStatementBlockExpression;
                    if (block == null)
                        throw new ArgumentNullException("block");

                    // Write statement
                    for (int j = 0; j < block.ConstituentSymbols.Count; j++)
                    {
                        StructStatementExpression statement = block.ConstituentSymbols[j] as StructStatementExpression;

                        string attribute = statement.ConstituentSymbols[3].ToString();

                        // Skip first position, in GLSL is "gl_Position"
                        //if (isFragment && (attribute == "POSITION" || attribute == "POSITION0"))
                            //continue;

                        // Generate vertex shader attribute
                        if (!isFragment)
                        {
                            List<VertexShaderAttribute> vertexAttributes = null;
                            if (_vertexShaderAttributes.TryGetValue(entryPoint.Name, out vertexAttributes))
                            {
                            }
                            else
                            {
                                vertexAttributes = new List<VertexShaderAttribute>();
                                _vertexShaderAttributes.Add(entryPoint.Name, vertexAttributes);
                            }

                            vertexAttributes.Add(GetVertexShaderAttribute(statement));
                        }

                        // Set flag
                        if (isFragment)
                            stringBuilder.Append("varying ");
                        else
                            stringBuilder.Append("attribute ");

                        // Write type
                        stringBuilder.Append(ConvertType(statement.ConstituentSymbols[0].ToString()));

                        // Write name
                        stringBuilder.Append(" ");
                        if (isFragment)
                            stringBuilder.Append("v_" + statement.ConstituentSymbols[1].ToString());
                        else
                            stringBuilder.Append(statement.ConstituentSymbols[1].ToString());

                        // Finalize
                        stringBuilder.Append(";");
                        stringBuilder.Append(Environment.NewLine);
                    }                    
                }
                else if (item.Name == outputName)
                {
                    // Get struct statement block
                    StructStatementBlockExpression block = item.ConstituentSymbols.Find(s => s is StructStatementBlockExpression) as StructStatementBlockExpression;
                    if (block == null)
                        throw new ArgumentNullException("StructStatementBlockExpression");

                    // If isFragment == true, To do support multiple output, for multiple render target purpose ????
                    if (isFragment)
                        continue;

                    // Write statement
                    for (int j = 0; j < block.ConstituentSymbols.Count; j++)
                    {
                        StructStatementExpression statement = block.ConstituentSymbols[j] as StructStatementExpression;

                        string attribute = statement.ConstituentSymbols[3].ToString();

                        // Skip first position, in GLSL is "gl_Position"
                        //if (attribute == "POSITION" || attribute == "POSITION0")
                            //continue;

                        // Set flag
                        stringBuilder.Append("varying ");

                        // Write type
                        stringBuilder.Append(ConvertType(statement.ConstituentSymbols[0].ToString()));

                        // Write name
                        stringBuilder.Append(" ");
                        stringBuilder.Append("v_" + statement.ConstituentSymbols[1].ToString());

                        // Finalize
                        stringBuilder.Append(";");
                        stringBuilder.Append(Environment.NewLine);
                    }                    
                }
            }
        }
        static void EntryPointProcessor(StringBuilder stringBuilder, ShaderMetadata metadata)
        {
            if (metadata.EntryPoint == null)
                throw new ArgumentNullException("metadata.EntryPoint");

            FunctionProcessor(stringBuilder, metadata.EntryPoint, metadata);
        }
        static void ExtendedFunctionsProcessor(StringBuilder stringBuilder, ShaderMetadata metadata)
        {
            if (metadata.ExtendedFunctions == null)
                return;

            foreach (var expression in metadata.ExtendedFunctions)
            {
                FunctionProcessor(stringBuilder, expression, metadata);
                if (metadata.ExtendedFunctions.IndexOf(expression) < metadata.ExtendedFunctions.Count - 1)
                    stringBuilder.Append(Environment.NewLine);
            }
        }
        static void FunctionProcessor(StringBuilder stringBuilder, FunctionExpression expression, ShaderMetadata metadata)
        {
            // Is entry point
            if (metadata.Name == expression.Name)
            {
                stringBuilder.Append("void main()");
            }// Is general function
            else
            {                
                // Write type
                stringBuilder.Append(ConvertType(expression.ConstituentSymbols[0].ToString()));

                // Write name
                stringBuilder.Append(" ");
                stringBuilder.Append(expression.ConstituentSymbols[1].ToString());

                // Begin arguments
                stringBuilder.Append("(");

                // Write arguments
                ArgumentExpression argumentExpression = expression.ConstituentSymbols[3] as ArgumentExpression;
                if (argumentExpression != null && argumentExpression.ConstituentSymbols.Count > 0)
                {
                    for (int i = 0; i < argumentExpression.ConstituentSymbols.Count; i += 3)
                    {
                        // Write type
                        stringBuilder.Append(ConvertType(argumentExpression.ConstituentSymbols[i].ToString()));

                        // Write name
                        stringBuilder.Append(" ");
                        stringBuilder.Append(argumentExpression.ConstituentSymbols[i + 1].ToString());
                        if (i + 3 < argumentExpression.ConstituentSymbols.Count)
                            stringBuilder.Append(", ");
                    }
                }

                // End arguments
                stringBuilder.Append(")");
            }

            // Create list mapping
            List<VariableCache> varMap = new List<VariableCache>();

            // Append shader vars
            varMap.AddRange(_shaderVarMap);

            // Add function arguments
            foreach (var symbol in expression.ConstituentSymbols)
            {
                if (symbol is ArgumentExpression)
                {
                    VariableCache varDescription = new VariableCache();
                    varDescription.Level = 1;
                    varDescription.Type = symbol.ConstituentSymbols[0].ToString();
                    varDescription.Name = symbol.ConstituentSymbols[1].ToString();
                    varMap.Add(varDescription);
                }
            }

            // Checking has return semantic for entry point function only
            Colon doubleDotExpression = expression.ConstituentSymbols[5] as Colon;

            // Begin statement block
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("{");

            // Write statement block
            StatementBlockExpression statementBlockExpression = doubleDotExpression == null ?
                expression.ConstituentSymbols[6] as StatementBlockExpression :
                expression.ConstituentSymbols[8] as StatementBlockExpression;
            StatementBlockProcessor(stringBuilder, statementBlockExpression, expression.Name, metadata, varMap, 1);

            // End statement block
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("}");
        }

        static void StatementBlockProcessor(StringBuilder stringBuilder, StatementBlockExpression expression, string functionName, ShaderMetadata metadata, List<VariableCache> varMap, int depth = 0)
        {
            foreach (var element in expression.ConstituentSymbols)
            {
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.AppendTab(depth);

                if (element is StatementExpression ||
                    element is ReturnStatementExpression ||
                    element is BreakStatementExpression ||
                    element is ContinueStatementExpression ||
                    element is DiscardStatementExpression)
                {
                    int index = expression.ConstituentSymbols.IndexOf(element);

                    if (element is StatementExpression)
                    {
                        TextPart firstSymbol = element.ConstituentSymbols[0] as TextPart;
                        if (firstSymbol != null && IsSemantic(firstSymbol.TextValue, metadata))
                        {
                            AddVariable(depth, firstSymbol.TextValue, (element.ConstituentSymbols[1] as TextPart).TextValue, varMap);

                            // Jump to next elemen
                            continue;
                        }
                    }
                    else if (element is ReturnStatementExpression)
                    {
                        // Replace with gl_FragColor or gl_FragColor[n]
                        if (IsReturnStatementFragment(functionName, metadata))
                        {
                            ReturnStatementFragmentProcessor(stringBuilder, element as ReturnStatementExpression, functionName, metadata, varMap, depth);
                            
                            // Jump to next element
                            continue;
                        }
                        else if (IsReturnStatementVertex(functionName, metadata))
                        {
                            ReturnStatementVertexProcessor(stringBuilder, element as ReturnStatementExpression, functionName, metadata, varMap, depth);

                            // Jump to next element
                            continue;
                        }                        
                    }

                    CommonProcessor(stringBuilder, element.ConstituentSymbols, metadata, varMap, depth);                        
                }
                else if (element is IfStatementExpression)
                    IfStatementProcessor(stringBuilder, element as IfStatementExpression, functionName, metadata, varMap, depth);
                else if (element is ForStatementExpression)
                    ForStatementProcessor(stringBuilder, element as ForStatementExpression, functionName, metadata, varMap, depth);
            }
        }
        static void ForStatementProcessor(StringBuilder stringBuilder, ForStatementExpression expression, string functionName, ShaderMetadata metadata, List<VariableCache> varMap, int depth = 0)
        {
            foreach (var element in expression.ConstituentSymbols)
            {
                int index = expression.ConstituentSymbols.IndexOf(element);
                var nextElement = index + 1 < expression.ConstituentSymbols.Count ?
                    expression.ConstituentSymbols[index + 1] : null;

                // Pre
                if (element is OpenCurlyBracket || element is CloseCurlyBracket)
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.AppendTab(depth);
                }

                // Main
                if (element is ForHeaderExpression)
                {
                    CommonProcessor(stringBuilder, element.ConstituentSymbols, metadata, varMap, depth);
                }
                else if (element is StatementBlockExpression)
                    StatementBlockProcessor(stringBuilder, element as StatementBlockExpression, functionName, metadata, varMap, depth + 1);
                else
                    stringBuilder.Append(element.ToString());

                // Post
                if (nextElement == null)
                    return;

                if (element is OpenRoundBracket)
                    continue;

                if (nextElement is CloseRoundBracket)
                    continue;

                if (element is OpenCurlyBracket || element is CloseCurlyBracket)
                {
                    if (element is CloseCurlyBracket && nextElement != null)
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.AppendTab(depth);
                    }

                    continue;
                }

                stringBuilder.Append(" ");
            }
        }
        static void IfStatementProcessor(StringBuilder stringBuilder, IfStatementExpression expression, string functionName, ShaderMetadata metadata, List<VariableCache> varMap, int depth = 0)
        {
            foreach (var element in expression.ConstituentSymbols)
            {
                int index = expression.ConstituentSymbols.IndexOf(element);
                var nextElement = index + 1 < expression.ConstituentSymbols.Count ?
                    expression.ConstituentSymbols[index + 1] : null;

                // Pre
                if (element is OpenCurlyBracket || element is CloseCurlyBracket)
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.AppendTab(depth);
                }

                // Main
                if (element is IfHeaderExpression)
                {
                    CommonProcessor(stringBuilder, element.ConstituentSymbols, metadata, varMap, depth);
                }
                else if (element is StatementBlockExpression)
                    StatementBlockProcessor(stringBuilder, element as StatementBlockExpression, functionName, metadata, varMap, depth + 1);
                else
                {
                    stringBuilder.Append(element.ToString());
                }

                // Post
                if (nextElement == null)
                    return;

                if (element is OpenRoundBracket)
                    continue;

                if (nextElement is CloseRoundBracket)
                    continue;

                if (element is OpenCurlyBracket || element is CloseCurlyBracket)
                {
                    if (element is CloseCurlyBracket && nextElement != null)
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.AppendTab(depth);
                    }

                    continue;
                }

                stringBuilder.Append(" ");
            }
        }
        static void CommonProcessor(StringBuilder stringBuilder, List<Symbol> expression, ShaderMetadata metadata, List<VariableCache> varMap, int depth = 0)
        {
            Symbol skipAfterIt = null;
            foreach (var element in expression)
            {
                int index = expression.IndexOf(element);
                var beforeElement = index - 1 >= 0 ?
                    expression[index - 1] : null;
                var beforeBeforeElement = index - 2 >= 0 ?
                    expression[index - 2] : null;
                var nextElement = index + 1 < expression.Count ?
                    expression[index + 1] : null;
                var nextNextElement = index + 2 < expression.Count ?
                    expression[index + 2] : null;

                // Pre
                if (skipAfterIt != null)
                {
                    if (element == skipAfterIt)
                        skipAfterIt = null;
                    
                    continue;
                }

                // Main
                if (element is ProcedureExpression)
                    ProcedureProcessor(stringBuilder, element as ProcedureExpression, metadata, varMap);
                else if (element is OpenRoundBracket && nextElement is TextPart && nextNextElement is CloseRoundBracket && IsType((nextElement as TextPart).TextValue))
                {
                    int indexOfNextNextElement = expression.IndexOf(nextNextElement);
                    Symbol valueElement = expression[indexOfNextNextElement + 1];
                    skipAfterIt = expression[indexOfNextNextElement + 1];

                    CastProcessor(stringBuilder, nextElement.ToString(), valueElement, metadata, varMap);
                }
                else if (element is TextPart)
                    SyntaxProcessor(stringBuilder, element as TextPart, metadata, beforeElement, varMap);
                else
                {
                    if (element is Equals)
                    {
                        // Add var to map
                        if (beforeElement is TextPart && beforeBeforeElement is TextPart)
                        {
                            AddVariable(depth + 1, (beforeBeforeElement as TextPart).TextValue, (beforeElement as TextPart).TextValue, varMap);
                        }
                    }

                    stringBuilder.Append(element.ToString());
                }

                // Begin Post
                if (((element is Plus || element is Minus ||
                    element is Asterisk || element is ForwardSlash ||
                    element is GreaterThan || element is LessThan ||
                    element is ExclamationMark || element is Equals) && nextElement is Equals))
                    continue;

                if (element is ExclamationMark)
                    continue;

                if (element is OpenRoundBracket)
                    continue;

                if (!(element is Equals) && (nextElement is OpenRoundBracket || nextElement is CloseRoundBracket))
                    continue;

                if (nextElement is Comma || nextElement is Semicolon)
                    continue;

                if ((element is VerticalBar && nextElement is VerticalBar) ||
                    (element is Ampersand && nextElement is Ampersand))
                    continue;

                if ((element is Plus && nextElement is Plus) ||
                    (!(element is Semicolon) && nextElement is Plus && nextNextElement is Plus) ||
                    (element is Plus && beforeElement is Plus))
                    continue;

                if ((element is Minus && nextElement is Minus) ||
                    (!(element is Semicolon) && nextElement is Minus && nextNextElement is Minus) ||
                    (element is Minus && beforeElement is Minus))
                    continue;

                if (element is ProcedureExpression && nextElement is TextPart)
                {
                    var colonTextPart = nextElement as TextPart;
					if (colonTextPart.TextValue.StartsWith(".", StringComparison.InvariantCulture))
                        continue;
                }

                if (nextElement == null)
                    return;

                stringBuilder.Append(" ");
                // End Post
            }
        }
        static void ProcedureProcessor(StringBuilder stringBuilder, ProcedureExpression expression, ShaderMetadata metadata, List<VariableCache> varMap)
        {
            // SWAP ARGUMENTS !!!

            if (expression == null)
                return;

            // Pre process
            // For other procedure .... like mul, saturate, cross, etc (in HLSL is not needed, maybe do it on GLSL)
            // 1. Mul, mul(x, y) => x * y
            if (expression.Name == "mul")
            {
                // Find x, y part
                // Find saturate syntax part
                List<Symbol> xyPart = expression.ConstituentSymbols.GetRange(2, expression.ConstituentSymbols.Count - 3);

                // Clear expression
                expression.ConstituentSymbols.Clear();

                // Find index of ,
                var comma = xyPart.Find(p => p is Comma);
                int indexOfComma = xyPart.IndexOf(comma);

                // Add open bracket
				expression.ConstituentSymbols.Add(new OpenRoundBracket());

				// Add x part
				expression.ConstituentSymbols.AddRange(xyPart.GetRange(0, indexOfComma));
				// Add * part
				expression.ConstituentSymbols.Add(new Asterisk());
				// Add y part
				expression.ConstituentSymbols.AddRange(xyPart.GetRange(indexOfComma + 1, xyPart.Count - 1 - indexOfComma));

				// Add close bracket
				expression.ConstituentSymbols.Add(new CloseRoundBracket());
            }
            // 2. Saturate, saturate(x) => clamp(x, 0.0, 1.0)
            if (expression.Name == "saturate")
            {
                // Find saturate syntax part
                TextPart saturatePart = expression.ConstituentSymbols[0] as TextPart;

                // Find x part
                List<Symbol> xPart = expression.ConstituentSymbols.GetRange(2, expression.ConstituentSymbols.Count - 3);
                // Remove x part
                foreach (var x in xPart)
                    expression.ConstituentSymbols.Remove(x);

                // Rename saturate
                saturatePart.TextValue = "clamp";

                // Add any symbol "x,0.0,1.0"
                expression.ConstituentSymbols.InsertRange(2, xPart);

                int nextIndexAfterInsertX = expression.ConstituentSymbols.Count - 1;
                expression.ConstituentSymbols.Insert(nextIndexAfterInsertX, new Comma());
                expression.ConstituentSymbols.Insert(nextIndexAfterInsertX + 1, new TextPart("0.0"));
                expression.ConstituentSymbols.Insert(nextIndexAfterInsertX + 2, new Comma());
                expression.ConstituentSymbols.Insert(nextIndexAfterInsertX + 3, new TextPart("1.0"));
            }
            // 3. Cross,  cross(T, N) =>  cross(N, T)
            else if (expression.Name == "cross")
            {
                // Find t part
                TextPart tPart = expression.ConstituentSymbols[2] as TextPart;

                // Find n part
                TextPart nPart = expression.ConstituentSymbols[4] as TextPart;

                // Rename t part
                string cacheTPart = tPart.TextValue;
                tPart.TextValue = nPart.TextValue;
                // Rename n part
                nPart.TextValue = cacheTPart;
            }

            // Main
            CommonProcessor(stringBuilder, expression.ConstituentSymbols, metadata, varMap);
        }
        static void SyntaxProcessor(StringBuilder stringBuilder, TextPart expression, ShaderMetadata metadata, Symbol beforeExpression, List<VariableCache> varMap)
        {
            // REPLACE or RENAME !!!

            if (expression == null)
                return;

            // Prepare
            string translationResult = string.Empty;

            // For other syntax .... like float3, float4, float2, int, bool etc (in GLSL is not needed, maybe do it on GLSL)
			// 1. For procedure "tex2D"
			if (expression.TextValue == "tex2D" && metadata is PixelShaderMetadata)
			{
				// Translate
				stringBuilder.Append("texture2D");
			}
			// 1. For procedure "texCube"
			else if (expression.TextValue == "texCube" && metadata is PixelShaderMetadata)
			{
				// Translat
				stringBuilder.Append("textureCube");
			}
			// 2. For procedure "tex2Dproj"
			else if (expression.TextValue == "tex2Dproj" && metadata is PixelShaderMetadata)
			{
				// Translat
				stringBuilder.Append("texture2Dproj");
			}
			// 3. For procedure "atan2"
			else if (expression.TextValue == "atan2")
			{
				// Transl
				stringBuilder.Append("atan");
			}
			// 4. For procedure "ddx"
			else if (expression.TextValue == "ddx")
			{
				// Transla
				stringBuilder.Append("dFdx");
			}
			// 5. For procedure "ddy"
			else if (expression.TextValue == "ddy")
			{
				// Transl
				stringBuilder.Append("dFdy");
			}
			// 6. For procedure "fmod"
			else if (expression.TextValue == "fmod")
			{
				// Translat
				stringBuilder.Append("mod");
			}
			// 7. For procedure "frac"
			else if (expression.TextValue == "fract")
			{
				// Translat
				stringBuilder.Append("fract");
			}
			// 8. For procedure "lerp"
			else if (expression.TextValue == "lerp")
			{
				// Translat
				stringBuilder.Append("mix");
			}
			// 9. For procedure "noise"
			else if (expression.TextValue == "noise")
			{
				// Translat
				stringBuilder.Append("noise1");
			}
			// 10. For procedure "rqrt"
			else if (expression.TextValue == "rsqrt")
			{
				// Translat
				stringBuilder.Append("inversesqrt");
			}
			// 11. For replace sampler            
			else if (IsSampler(expression.TextValue, metadata, out translationResult))
			{
				stringBuilder.Append(translationResult);
			}
			// 12. For remove "f" in type float number, glsl not supported
			else if (TryToMatchFloatingPoint(expression.TextValue, out translationResult))
			{
				// Translate
				stringBuilder.Append(translationResult);
			}
			// 13. For replace variable, ex : input.tex output.pos blabla.xy input.tex.xy etc
			else if (TryToMatchVariable(expression.TextValue, metadata, varMap, out translationResult))
			{
				// Translate
				stringBuilder.Append(translationResult);
			}
			// Other goes to here
			else
			{
				// Main
				stringBuilder.Append(ShaderHelper.ConvertToGlslType(expression.TextValue));
			}
        }

        static void CastProcessor(StringBuilder stringBuilder, string type, Symbol expression, ShaderMetadata metadata, List<VariableCache> varMap)
        {
            stringBuilder.Append(ConvertType(type) + "(");
            List<Symbol> value = new List<Symbol>();
            value.Add(expression);
            CommonProcessor(stringBuilder, value, metadata, varMap);
            stringBuilder.Append(")");
        }

        static bool TryToMatchVariable(string input, ShaderMetadata metadata, List<VariableCache> varMap, out string result)
        {
            result = string.Empty;

            // Convert variable
            if (input.Contains(DotChar))
            {
                // Split by dot 
                string[] parts = input.Split(DotChar);

                // Test
#if DEBUG
                System.Diagnostics.Debug.WriteLine("=>" + input);
                foreach (string part in parts)
                    System.Diagnostics.Debug.WriteLine("==>" + part);
#endif

                // Like struct or semantics
                if (parts.Length > 1)
                {
                    VariableCache vars;
                    if (ContainsVariableName(0, parts[0], varMap, out vars))
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("result : " + vars.Type + " " + vars.Name);
#endif
                        StructExpression semantic = metadata.Semantics.Find(s => s.Name == vars.Type);
                        if (semantic != null)
                        {
                            // Get entry point
                            FunctionExpression entryPoint = metadata.EntryPoint;

                            // Prepare
                            string inputName = (((ArgumentExpression)entryPoint.ConstituentSymbols[3]).ConstituentSymbols[0] as TextPart).TextValue;
                            string outputName = (entryPoint.ConstituentSymbols[0] as TextPart).TextValue;
                            bool isFragment = metadata is PixelShaderMetadata;

                            // Get struct statement block
                            StructStatementBlockExpression block = semantic.ConstituentSymbols.Find(s => s is StructStatementBlockExpression) as StructStatementBlockExpression;

                            // Count of statements
                            int structStatementCount = block.ConstituentSymbols.Count;

                            // Iterate
                            // Write statement
                            for (int j = 0; j < block.ConstituentSymbols.Count; j++)
                            {
                                StructStatementExpression statement = block.ConstituentSymbols[j] as StructStatementExpression;

                                // Get semantic type
                                TextPart signatureType = statement.ConstituentSymbols[3] as TextPart;

                                // Get signature name
                                TextPart signatureName = statement.ConstituentSymbols[1] as TextPart;

                                if (signatureName.TextValue == parts[1])
                                {
                                    if (inputName == semantic.Name)
                                    {
                                        if (isFragment)
                                        {
                                            result = "v_" + signatureName.TextValue;

                                            if (parts.Length > 2)
                                                result += "." + parts[2];

                                            break;
                                        }
                                        else
                                        {
                                            result = signatureName.TextValue;

                                            if (parts.Length > 2)
                                                result += "." + parts[2];

                                            break;
                                        }
                                    }
                                    else if (outputName == semantic.Name)
                                    {
                                        if (isFragment)
                                        {
                                            // If signature type is COLOR
											if (signatureType.TextValue.Contains(SignatureColorString))
											{
												string signatureId = signatureType.TextValue.Replace(SignatureColorString, "");

												//if (!string.IsNullOrEmpty(signatureId))
												//{
												result = "gl_FragData[" + signatureId + "]";

												if (parts.Length > 2)
													result += "." + parts[2];

												break;
												//}
											}

											// If signature type is DEPTH
											if (signatureType.TextValue.Contains(SignatureDepthString))
											{
												result = "gl_FragDepth";

												break;
											}
                                        }
                                        else
                                        {
                                            // If signature type is POSITION0
                                            /*if (signatureType.TextValue == SIGNATURE_POSITION0 || signatureType.TextValue == SIGNATURE_POSITION)
                                            {
                                                result = "gl_Position";

                                                if (parts.Length > 2)
                                                    result += "." + parts[2];

                                                break;
                                            }
                                            else
                                            {*/
                                            result = "v_" + signatureName.TextValue;

                                            if (parts.Length > 2)
                                                result += "." + parts[2];

                                            break;
                                            //}
                                        }
                                    }
                                }
                            }

                            return true;
                        }
                        else
                        {
                            //result = vars.Name;
                            result = input;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        static bool TryToMatchFloatingPoint(string input, out string result)
        {
			if (input.EndsWith(FloatString, StringComparison.InvariantCulture))
            {
                string number = input.Remove(input.Length - 1, 1);
                float realNumber = 0;
                if (float.TryParse(number, out realNumber))
                {
                    result = number;
                    return true;
                }
            }

            result = string.Empty;
            return false;
        }

        static bool IsSampler(string sampler, ShaderMetadata metadata, out string resource)
        {
            if (metadata is PixelShaderMetadata)
            {
                PixelShaderMetadata pixelShader = metadata as PixelShaderMetadata;
                if (pixelShader.Samplers != null)
                    foreach (var s in pixelShader.Samplers)
                        if (s.Name == sampler)
                        {
                            resource = s.Resource;
                            return true;
                        }
            }

            resource = string.Empty;
            return false;
        }

        static bool IsReturnStatementVertex(string functionName, ShaderMetadata metadata)
        {
            if (metadata is VertexShaderMetadata && metadata.Name == functionName)
                return true;

            return false;
        }
        static bool IsReturnStatementFragment(string functionName, ShaderMetadata metadata)
        {
            if (metadata is PixelShaderMetadata && metadata.Name == functionName)
                return true;

            return false;
        }

        static void ReturnStatementVertexProcessor(StringBuilder stringBuilder, ReturnStatementExpression expression, string functionName, ShaderMetadata metadata, List<VariableCache> varMap, int depth = 0)
        {
            // Get return value
            Symbol value = expression.ConstituentSymbols[1];

            // Remove "return" syntax
            expression.ConstituentSymbols.RemoveAt(0);

            if (value is TextPart)
            {
                // Get entry point
                FunctionExpression entryPoint = metadata.EntryPoint;

                // Prepare
                string outputName = (entryPoint.ConstituentSymbols[0] as TextPart).TextValue;

                StructExpression semantic = metadata.Semantics.Find(s => s.Name == outputName);

                if (semantic != null)
                {
                    StructStatementBlockExpression block = semantic.ConstituentSymbols.Find(s => s is StructStatementBlockExpression) as StructStatementBlockExpression;

                    // Count of statements
                    int structStatementCount = block.ConstituentSymbols.Count;

                    // Iterate
                    // Write statement
                    for (int j = 0; j < block.ConstituentSymbols.Count; j++)
                    {
                        StructStatementExpression statement = block.ConstituentSymbols[j] as StructStatementExpression;

                        // Get semantic type
                        TextPart signatureType = statement.ConstituentSymbols[3] as TextPart;

                        // Get signature name
                        TextPart signatureName = statement.ConstituentSymbols[1] as TextPart;

                        // If signature type is POSITION0
                        if (signatureType.TextValue == SignaturePosition0String || signatureType.TextValue == SignaturePositionString)
                        {
							// Get position varying variableg
							/*string positionVarying = "v_" + signatureName;

							// Fix flip vertically
							stringBuilder.Append(positionVarying + ".y = " + positionVarying + ".y * PositionFixup.y;");
							stringBuilder.Append(Environment.NewLine);
							stringBuilder.Append(positionVarying + ".xy += PositionFixup.zw * " + positionVarying + ".ww;");
							stringBuilder.Append(Environment.NewLine);*/

							stringBuilder.Append("gl_Position = v_" + signatureName + ";");
							stringBuilder.Append(Environment.NewLine);
							stringBuilder.Append("gl_Position.y = gl_Position.y * PositionFixup.y;");
							stringBuilder.Append(Environment.NewLine);
							stringBuilder.Append("gl_Position.xy += PositionFixup.zw * gl_Position.ww;");

                            return;
                        }
                    }
                }
            }
        }

        static void ReturnStatementFragmentProcessor(StringBuilder stringBuilder, ReturnStatementExpression expression, string functionName, ShaderMetadata metadata, List<VariableCache> varMap, int depth = 0)
        {
            // Get return value
            Symbol value = expression.ConstituentSymbols[1];

            // Remove "return" syntax
            expression.ConstituentSymbols.RemoveAt(0);

            if (value is TextPart)
            {
                VariableCache vars;
                if (ContainsVariableName(0, ((TextPart)value).TextValue, varMap, out vars))
                {
                    StructExpression semantic = metadata.Semantics.Find(s => s.Name == vars.Type);
                    if (semantic != null)
                    {
						// Skip return processing if return value is semantic
                        //CommonProcessor(stringBuilder, expression.ConstituentSymbols, metadata, varMap, depth);
                        return;
                    }
                }
            }

            stringBuilder.Append("gl_FragColor = ");
            CommonProcessor(stringBuilder, expression.ConstituentSymbols, metadata, varMap, depth);          
        }

        static string ConvertType(string input)
        {
            string type = input;

            switch (type)
            {
                case "float2":
                    type = "vec2";
                    break;
                case "float3":
                    type = "vec3";
                    break;
                case "float4":
                    type = "vec4";
                    break;                
                case "float2x2":
                    type = "mat2";
                    break;
                case "float3x3":
                    type = "mat3";
                    break;
                case "float4x4":
                    type = "mat4";
                    break;
                case "bool2":
                    type = "bvec2";
                    break;
                case "bool3":
                    type = "bvec3";
                    break;
                case "bool4":
                    type = "bvec4";
                    break;
                case "int2":
                    type = "ivec2";
                    break;
                case "int3":
                    type = "ivec3";
                    break;
                case "int4":
                    type = "ivec4";
                    break;
            }

            return type;
        }
        static bool IsType(string input)
        {
            switch (input)
            {
                case "float":
                    return true;
                case "int":
                    return true;
                case "bool":
                    return true;
                case "float2":
                    return true;
                case "float3":
                    return true;
                case "float4":
                    return true;
                case "float2x2":
                    return true;
                case "float3x3":
                    return true;
                case "float4x4":
                    return true;
                case "bool2":
                    return true;
                case "bool3":
                    return true;
                case "bool4":
                    return true;
                case "int2":
                    return true;
                case "int3":
                    return true;
                case "int4":
                    return true;
            }

            return false;
        }
        static string ConvertTypeByDimension(string input, int dimension1, int dimension2)
        {
            string type = input;

            if (dimension1 > 1)
            {
                if (dimension2 > 1 && dimension1 == dimension2)
                {
                    switch (type)
                    {
                        case "float":
                            type = "mat";
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case "float":
                            type = "vec";
                            break;
						case "half":
							type = "vec";
							break;
                        case "bool":
                            type = "bvec";
                            break;
                        case "int":
                            type = "ivec";
                            break;
                    }
                }
            }           

            return type;
        }
        static string ConvertResource(string input)
        {
            return input.Replace("texture", "sampler");
        }

        static string CombineType(string input, int dimension1, int dimension2)
        {
            string type = input;

            if (dimension1 > 1)
            {
                type += dimension1;

                if (dimension2 > 1)
                    type += "x" + dimension2;
            }

            return type;
        }

        static void AddVariable(int level, string type, string name, List<VariableCache> vars)
        {
            VariableCache result;
            if (!HasInsertVariableName(level, name, vars, out result))
            {
                VariableCache varDesc = new VariableCache();
                varDesc.Level = level;
                varDesc.Type = type;
                varDesc.Name = name;

                vars.Add(varDesc);
            }
        }
        static bool HasInsertVariableName(int level, string name, List<VariableCache> vars, out VariableCache result)
        {
            foreach (var varDesc in vars)
            {
                if (varDesc.Name == name)
                {
                    if (varDesc.Level <= level)
                    {
                        result = varDesc;
                        return true;
                    }
                }
            }

            result = default(VariableCache);
            return false;
        }
        static bool ContainsVariableName(int level, string name, List<VariableCache> vars, out VariableCache result)
        {
            foreach (var varDesc in vars)
            {
                if (varDesc.Name == name)
                {
                    if (level <= varDesc.Level)
                    {
                        result = varDesc;
                        return true;
                    }
                }
            }

            result = default(VariableCache);
            return false;
        }
        static bool IsSemantic(string type, ShaderMetadata metadata)
        {
            foreach (var semantic in metadata.Semantics)
            {
                if (semantic.Name == type)
                    return true;
            }

            return false;
        }
        static VertexShaderAttribute GetVertexShaderAttribute(StructStatementExpression statement)
        {
            /*Position,
            Color,
            TextureCoordinate,
            Normal,
            Binormal,
            Tangent,
            BlendIndices,
            BlendWeight,
            Depth,
            Fog,
            PointSize,
            Sample,
            TessellateFactor*/

            VertexShaderAttribute result = new VertexShaderAttribute();
            result.Name = statement.ConstituentSymbols[1].ToString();

            string attribute = statement.ConstituentSymbols[3].ToString();
            int usage;
            string index = "0";
			if (attribute.StartsWith("POSITION", StringComparison.InvariantCulture))
            {
                usage = 0;
                index = attribute.Replace("POSITION", string.Empty);
            }
            else if (attribute.StartsWith("COLOR", StringComparison.InvariantCulture))
            {
                usage = 1;
                index = attribute.Replace("COLOR", string.Empty);
            }
            else if (attribute.StartsWith("TEXCOORD", StringComparison.InvariantCulture))
            {
                usage = 2;
                index = attribute.Replace("TEXCOORD", string.Empty);
            }
            else if (attribute.StartsWith("NORMAL", StringComparison.InvariantCulture))
            {
                usage = 3;
                index = attribute.Replace("NORMAL", string.Empty);
            }
            else if (attribute.StartsWith("BINORMAL", StringComparison.InvariantCulture))
            {
                usage = 4;
                index = attribute.Replace("BINORMAL", string.Empty);
            }
            else if (attribute.StartsWith("TANGENT", StringComparison.InvariantCulture))
            {
                usage = 5;
                index = attribute.Replace("TANGENT", string.Empty);
            }
            else if (attribute.StartsWith("BLENDINDICES", StringComparison.InvariantCulture))
            {
                usage = 6;
                index = attribute.Replace("BLENDINDICES", string.Empty);
            }
            else if (attribute.StartsWith("BLENDWEIGHT", StringComparison.InvariantCulture))
            {
                usage = 7;
                index = attribute.Replace("BLENDWEIGHT", string.Empty);
            }
            else
                throw new NotSupportedException(attribute);

            result.Usage = usage;
            if (!string.IsNullOrEmpty(index))
            {
                result.Index = int.Parse(index, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
                result.Index = 0;

            return result;
        }
        #endregion

        protected override void OnVertexBuild(string entryPoint, string source, bool optimize, out byte[] output)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // Append shader attribute
                    List<VertexShaderAttribute> attributes;
                    _vertexShaderAttributes.TryGetValue(entryPoint, out attributes);

                    if (attributes != null)
                    {
                        writer.Write(attributes.Count);
                        foreach (VertexShaderAttribute attribute in attributes)
                        {
                            writer.Write(attribute.Name);
                            writer.Write(attribute.Usage);
                            writer.Write(attribute.Index);
                        }
                    }
                    else
                        throw new ArgumentException("Vertex shader not contain attributes");

					if (optimize)
					{
#if DEBUG
						System.Diagnostics.Debug.WriteLine("Starting optimze GLSLES Vertex Shader " + entryPoint);
						System.Diagnostics.Debug.WriteLine(source);
#endif
						string optimizeSource = GlslOptimizer.Optimize(source, true, false, true);
						if (!string.IsNullOrEmpty(optimizeSource))
						{
#if DEBUG
							System.Diagnostics.Debug.WriteLine("Optimze GLSL Vertex Shader");
							System.Diagnostics.Debug.WriteLine(optimizeSource);
#endif
							writer.Write(optimizeSource);
						}
						else
							writer.Write(source);
					}
					else
						writer.Write(source);
                }

                output = stream.ToArray();
            }
        }
		protected override void OnPixelBuild(string entryPoint, string source, bool optimize, out byte[] output)
        {
			if (optimize)
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine("Starting optimze GLSLES Pixel Shader " + entryPoint);
				System.Diagnostics.Debug.WriteLine(source);;
#endif

				string optimizeSource = GlslOptimizer.Optimize(source, true, false, false);
				if (!string.IsNullOrEmpty(optimizeSource))
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine("Optimze GLSLES Pixel Shader");
					System.Diagnostics.Debug.WriteLine(optimizeSource);
#endif
					output = System.Text.Encoding.ASCII.GetBytes(optimizeSource);
				}
				else
					output = System.Text.Encoding.ASCII.GetBytes(source);
			}
			else
				output = System.Text.Encoding.ASCII.GetBytes(source);
        }

        protected sealed override string GetLang()
        {
            return "GLSL_ES_100";
        }
        #endregion
    }
}
