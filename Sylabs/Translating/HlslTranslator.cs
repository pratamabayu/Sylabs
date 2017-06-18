using Sylabs.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Sylabs.Translating
{
    public abstract class HlslTranslator : ShaderTranslator
    {
		public static readonly bool USE_UNROLL_ATTRIBUTE = true;
		public static readonly bool USE_BRANCH_ATTRIBUTE = false;

        #region Constructors
        public HlslTranslator(ShaderParser parser)
            : base(parser)
        {
        }
        #endregion

        #region Methods
        protected override void OnVertexProcess(VertexShaderMetadata metadata, out string output)
        {
            StringBuilder builder = new StringBuilder();

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
            StringBuilder builder = new StringBuilder();

            ConstantsProcessor(builder, metadata);
            builder.Append(Environment.NewLine);
            ResourcesProcessor(builder, metadata);
            builder.Append(Environment.NewLine);
            SamplersProcessor(builder, metadata);
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
                // Write type
                stringBuilder.Append(expression.Type);
                // Write dimensions
                if (expression.Dimension[0] > 1)
                {
                    stringBuilder.Append(expression.Dimension[0]);
                    if(expression.Dimension[1] > 1)
                        stringBuilder.Append("x" + expression.Dimension[1]);
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
            }
        }
        static void ResourcesProcessor(StringBuilder stringBuilder, PixelShaderMetadata metadata)
        {
            if (metadata.Resources == null)
                return;

            foreach (var expression in metadata.Resources)
            {
                // Write type
                stringBuilder.Append(expression.Type.Replace("texture", "Texture"));

                // Write name
                stringBuilder.Append(" ");
                stringBuilder.Append(expression.Name);

                // Finalize
                stringBuilder.Append(";");
                stringBuilder.Append(Environment.NewLine);
            }
        }
        static void SamplersProcessor(StringBuilder stringBuilder, PixelShaderMetadata metadata)
        {
            if (metadata.Samplers == null)
                return;

            foreach (var expression in metadata.Samplers)
            {
                // Write type
				if(expression.Type.EndsWith("2D", StringComparison.InvariantCulture) || 
				   expression.Type.EndsWith("Cube", StringComparison.InvariantCulture))
                    stringBuilder.Append("SamplerState");

                // Write name
                stringBuilder.Append(" ");
                stringBuilder.Append(expression.Name);

                // Finalize
                stringBuilder.Append(";");
                stringBuilder.Append(Environment.NewLine);
            }
        }
        static void SemanticsProcessor(StringBuilder stringBuilder, ShaderMetadata metadata)
        {
            if (metadata.Semantics == null)
                return;

            for (int i = 0; i < metadata.Semantics.Count; i++)
            {
                // Get 
                StructExpression item = metadata.Semantics[i];

                // Write type
                stringBuilder.Append("struct");

                // Write name
                stringBuilder.Append(" ");
                stringBuilder.Append(item.ConstituentSymbols[1].ToString());

                // Begin
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append("{");
                stringBuilder.Append(Environment.NewLine);

                // Get struct statement block
                StructStatementBlockExpression block = item.ConstituentSymbols.Find(s => s is StructStatementBlockExpression) as StructStatementBlockExpression;
                if (block == null)
                    throw new ArgumentNullException("block");

                // Write statement
                for (int j = 0; j < block.ConstituentSymbols.Count; j++)
                {
                    StructStatementExpression statement = block.ConstituentSymbols[j] as StructStatementExpression;

                    // Write tab
                    stringBuilder.Append("\t");

                    // Write type
                    stringBuilder.Append(statement.ConstituentSymbols[0].ToString());

                    // Write name
                    stringBuilder.Append(" ");
                    stringBuilder.Append(statement.ConstituentSymbols[1].ToString());

                    // Write double dot
                    stringBuilder.Append(" :");

                    // Write semantic
                    stringBuilder.Append(" ");
                    string semantic = statement.ConstituentSymbols[3].ToString();
                    int index = metadata.Semantics.IndexOf(item);
                    if (metadata is VertexShaderMetadata)
                    {
                        if (metadata.Semantics.IndexOf(item) == 1 && semantic.StartsWith("POSITION", StringComparison.InvariantCulture))
                            semantic = semantic.Replace("POSITION", "SV_POSITION");
                    }
                    else if (metadata is PixelShaderMetadata)
                    {
                        if (index == 0 && semantic.StartsWith("POSITION", StringComparison.InvariantCulture))
                            semantic = semantic.Replace("POSITION", "SV_POSITION");
                        else if (index == 1 && semantic.StartsWith("COLOR", StringComparison.InvariantCulture))
                            semantic = semantic.Replace("COLOR", "SV_TARGET");
                        else if (index == 1 && semantic.StartsWith("DEPTH", StringComparison.InvariantCulture))
                            semantic = semantic.Replace("DEPTH", "SV_DEPTH");
                    }
                    stringBuilder.Append(semantic);

                    // Finalize
                    stringBuilder.Append(";");
                    stringBuilder.Append(Environment.NewLine);
                }

                // End
                stringBuilder.Append("};");
                stringBuilder.Append(Environment.NewLine);
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
                if(metadata.ExtendedFunctions.IndexOf(expression) < metadata.ExtendedFunctions.Count - 1)
                    stringBuilder.Append(Environment.NewLine);                
            }
        }
        static void FunctionProcessor(StringBuilder stringBuilder, FunctionExpression expression, ShaderMetadata metadata)
        {
            // Write type
            stringBuilder.Append(expression.ConstituentSymbols[0].ToString());

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
                    stringBuilder.Append(argumentExpression.ConstituentSymbols[i].ToString());

                    // Write name
                    stringBuilder.Append(" ");
                    stringBuilder.Append(argumentExpression.ConstituentSymbols[i + 1].ToString());
                    if(i + 3 < argumentExpression.ConstituentSymbols.Count)
                        stringBuilder.Append(", ");
                }
            }

            // End arguments
            stringBuilder.Append(")");

            // Checking has return semantic for entry point function only
            Colon doubleDotExpression = expression.ConstituentSymbols[5] as Colon;
            if (doubleDotExpression != null && metadata.EntryPoint == expression)
            {
                stringBuilder.Append(" : ");
                string returnSemantic = expression.ConstituentSymbols[6].ToString();
                if(returnSemantic.StartsWith("POSITION", StringComparison.InvariantCulture))
                    stringBuilder.Append(returnSemantic.Replace("POSITION", "SV_POSITION"));
                if (returnSemantic.StartsWith("COLOR", StringComparison.InvariantCulture))
                    stringBuilder.Append(returnSemantic.Replace("COLOR", "SV_TARGET"));
            }

            // Begin statement block
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("{");

            // Write statement block
            StatementBlockExpression statementBlockExpression = doubleDotExpression == null ? 
                expression.ConstituentSymbols[6] as StatementBlockExpression : 
                expression.ConstituentSymbols[8] as StatementBlockExpression;
            StatementBlockProcessor(stringBuilder, statementBlockExpression, metadata, 1);

            // End statement block
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("}");
        }
        
        static void StatementBlockProcessor(StringBuilder stringBuilder, StatementBlockExpression expression, ShaderMetadata metadata, int depth = 0)
        {
            foreach (var element in expression.ConstituentSymbols)
            {
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.AppendTab(depth);

                if (element is StatementExpression ||
                    element is ClipStatementExpression ||
                    element is ReturnStatementExpression ||
                    element is BreakStatementExpression ||
                    element is ContinueStatementExpression ||
                    element is DiscardStatementExpression)
                    CommonProcessor(stringBuilder, element.ConstituentSymbols, metadata);
                else if (element is IfStatementExpression)
                    IfStatementProcessor(stringBuilder, element as IfStatementExpression, metadata, depth);
                else if (element is ForStatementExpression)
                    ForStatementProcessor(stringBuilder, element as ForStatementExpression, metadata, depth);
            }
        }
        static void ForStatementProcessor(StringBuilder stringBuilder, ForStatementExpression expression, ShaderMetadata metadata, int depth = 0)
        {
			// Append unroll atrribute to each if statement
			if (USE_UNROLL_ATTRIBUTE)
			{
				stringBuilder.Append("[unroll]");
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.AppendTab(depth);
			}

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
                    CommonProcessor(stringBuilder, element.ConstituentSymbols, metadata);
                }
                else if (element is StatementBlockExpression)
                    StatementBlockProcessor(stringBuilder, element as StatementBlockExpression, metadata, depth + 1);
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
        static void IfStatementProcessor(StringBuilder stringBuilder, IfStatementExpression expression, ShaderMetadata metadata, int depth = 0)
        {
			// Append branch atrribute to each if statement
			if (USE_BRANCH_ATTRIBUTE)
			{
				stringBuilder.Append("[branch]");
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.AppendTab(depth);
			}

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
                    CommonProcessor(stringBuilder, element.ConstituentSymbols, metadata);
                }
                else if (element is StatementBlockExpression)
                    StatementBlockProcessor(stringBuilder, element as StatementBlockExpression, metadata, depth + 1);
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
        static void CommonProcessor(StringBuilder stringBuilder, List<Symbol> expression, ShaderMetadata metadata)
        {
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

                // Main
                if (element is ProcedureExpression)
                    ProcedureProcessor(stringBuilder, element as ProcedureExpression, metadata);
                else if (element is TextPart)
                    SyntaxProcessor(stringBuilder, element as TextPart, metadata);
                else
                {
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
                    if(colonTextPart.TextValue.StartsWith(".", StringComparison.InvariantCulture))
                        continue;
                }

                if (nextElement == null)
                    return;                

                stringBuilder.Append(" ");
                // End Post
            }
        }
        static void ProcedureProcessor(StringBuilder stringBuilder, ProcedureExpression expression, ShaderMetadata metadata)
        {
            // SWAP ARGUMENTS !!!

            if (expression == null)
                return;

            // Pre process
            // For procedure "tex2D"
			if (expression.Name.Equals("tex2D") && metadata is PixelShaderMetadata)
            {
                // Cast to pixel metadata
                var pixelMetadata = metadata as PixelShaderMetadata;

                // Find tex2D syntax part
                TextPart tex2DPart = expression.ConstituentSymbols[0] as TextPart;

                // Find sampler name
                TextPart samplerPart = expression.ConstituentSymbols[2] as TextPart;
                
                // Find sampler expression
                SamplerExpression samplerExpression = pixelMetadata.Samplers.Find(s => s.Name == samplerPart.TextValue);

                if (samplerExpression == null)
                    throw new ArgumentNullException("samplerExpression");

                // Find texture name
                if(string.IsNullOrEmpty(samplerExpression.Resource))
                    throw new ArgumentNullException("samplerExpression.Resource");

                // Modify
                tex2DPart.TextValue = samplerExpression.Resource + ".Sample";
            }
			if (expression.Name.Equals("texCube") && metadata is PixelShaderMetadata)
			{
				// Cast to pixel metadata
				var pixelMetadata = metadata as PixelShaderMetadata;

				// Find tex2D syntax part
				TextPart texCubePart = expression.ConstituentSymbols[0] as TextPart;

				// Find sampler name
				TextPart samplerPart = expression.ConstituentSymbols[2] as TextPart;

				// Find sampler expression
				SamplerExpression samplerExpression = pixelMetadata.Samplers.Find(s => s.Name == samplerPart.TextValue);

				if (samplerExpression == null)
					throw new ArgumentNullException("samplerExpression");

				// Find texture name
				if (string.IsNullOrEmpty(samplerExpression.Resource))
					throw new ArgumentNullException("samplerExpression.Resource");

				// Modify
				texCubePart.TextValue = samplerExpression.Resource + ".Sample";
			}
			// Check for unsupport construct float4x4(n) in hlsl, so convert to (float4x4)n
			if (expression.Name.Equals("float4x4"))
			{
				// Find float4x4 syntax part
                TextPart float4x4Part = expression.ConstituentSymbols[0] as TextPart;

				// Find value name
				TextPart valuePart = expression.ConstituentSymbols[2] as TextPart;

				if (valuePart != null && 
				    (valuePart.TextValue.Equals("0") || valuePart.TextValue.Equals(".0") || 
				     valuePart.TextValue.Equals(".0f") || valuePart.TextValue.Equals("0.0") || valuePart.TextValue.Equals("0.0f")))
				{
					// Remove procedur name and value => "()"
					expression.ConstituentSymbols.Remove(float4x4Part);
					expression.ConstituentSymbols.Remove(valuePart);

					// Insert procedur name to bracket => (float4x4)
					expression.ConstituentSymbols.Insert(1, float4x4Part);
					// Insert value in last => (float4x4)n
					expression.ConstituentSymbols.Add(valuePart);
				}
			}
            // For other procedure .... like clamp, max, min, etc (in HLSL is not needed, maybe do it on GLSL)
            
            // Main
            CommonProcessor(stringBuilder, expression.ConstituentSymbols, metadata);
        }
        static void SyntaxProcessor(StringBuilder stringBuilder, TextPart expression, ShaderMetadata metadata)
        {
            // REPLACE or RENAME !!!

            if (expression == null)
                return;

            // Pre process
            // Process (rename) syntax, example : maybe Vector3 -> float3
            /*// TODO Convert bool to int
             * // Removed !! shader supported boolean data type
            if (metadata.Constants != null)
            {
                ConstantExpression constantExpression = metadata.Constants.Find(c => c.Name == expression.TextValue);
            }*/

            // For other syntax .... like float3, float4, float2, int, bool etc (in GLSL is not needed, maybe do it on GLSL)

            // Main
            stringBuilder.Append(expression.ToString());
        }
        #endregion

        #region Build Helpers
        public static void Compile(string entryPoint, string targetProfile, string source, out byte[] output)
        {
            // Get fxc.exe path
            string compilerPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Windows Kits\8.0\bin\"), @"x86\fxc.exe");

			if (!File.Exists(compilerPath))
				compilerPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Windows Kits\8.1\bin\"), @"x86\fxc.exe");

			if (!File.Exists(compilerPath))
				compilerPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Windows Kits\10\bin\"), @"x86\fxc.exe");

			if (!File.Exists(compilerPath))
				compilerPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Microsoft DirectX SDK (June 2010)\\Utilities\\bin\\x86\\fxc.exe";

            if (!File.Exists(compilerPath))
            {
                // Try to get fxc.exe path
                var paths = Environment.GetEnvironmentVariable("Path").Split(';');
                foreach (var path in paths)
                {
                    if (path.Contains("DXSDK"))
                    {
                        compilerPath = Path.Combine(path, "fxc.exe");
                        break;
                    }
                }
            }

			if (!File.Exists(compilerPath))
				throw new FileNotFoundException(compilerPath);

            // Create input and output temporary file
            string tempInputFile = Path.GetTempFileName();
            File.WriteAllText(tempInputFile, source);
            string tempOutputFile = Path.GetTempFileName();

            // Setting up arguments
            string arguments = String.Format("{0} {1} {2} {3}", "/T " + targetProfile, "/E " + entryPoint, "/Fo" + tempOutputFile, tempInputFile);

            // Create compiler            
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = compilerPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Compiling process ... 
            string error = string.Empty;
            using (var proc = System.Diagnostics.Process.Start(startInfo))
            {
                StreamReader sr = proc.StandardError;
                error = sr.ReadToEnd().Replace(tempInputFile, "Line ");

                if (!proc.WaitForExit(5000))
                {
                    error = "General failure while compiling (timeout).";
                }
            }

            // Get shader byte code
            byte[] shaderByteCode = File.ReadAllBytes(tempOutputFile);

            if (shaderByteCode == null || shaderByteCode.Length == 0 || !string.IsNullOrEmpty(error))
                throw new InvalidOperationException("\"" + entryPoint + "\" shader\nError :\n" + error);

            // Delete temporary input file
            if (File.Exists(tempInputFile))
            {
                File.Delete(tempInputFile);
            }

            // Delete temporary output file
            if (File.Exists(tempOutputFile))
            {
                File.Delete(tempOutputFile);
            }

            output = shaderByteCode;
        }
        #endregion

        protected sealed override string GetLang()
        {
            return "HLSL_" + GetCompileVersion();
        }

        protected abstract string GetCompileVersion();
        #endregion
    }
}
