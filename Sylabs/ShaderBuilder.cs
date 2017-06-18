using Sylabs.Translating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sylabs
{
    public static class ShaderBuilder
    {
        static Dictionary<ShaderVersion, Type> _supportedTranslators = new Dictionary<ShaderVersion, Type>();
        
        static ShaderBuilder()
        {
            _supportedTranslators.Add(ShaderVersion.Hlsl_4_0_level_9_1, typeof(Hlsl_4_0_level_9_1_Translator));
            _supportedTranslators.Add(ShaderVersion.Hlsl_4_0_level_9_3, typeof(Hlsl_4_0_level_9_3_Translator));
            _supportedTranslators.Add(ShaderVersion.Hlsl_3_0, typeof(Hlsl_3_0_Translator));
            _supportedTranslators.Add(ShaderVersion.Hlsl_4_0, typeof(Hlsl_4_0_Translator));
            _supportedTranslators.Add(ShaderVersion.Hlsl_5_0, typeof(Hlsl_5_0_Translator));
            _supportedTranslators.Add(ShaderVersion.Glsl_120, typeof(Glsl_120_Translator));
            _supportedTranslators.Add(ShaderVersion.GlslEs_100, typeof(GlslEs_100_Translator));
        }

        public static ShaderTranslator Process(string source, ShaderVersion shaderVersion)
        {
            ShaderParser parser = new ShaderParser();
            parser.Process(source);

            Type translatorType;
            if (_supportedTranslators.TryGetValue(shaderVersion, out translatorType))
            {
                ShaderTranslator result = Activator.CreateInstance(translatorType, parser) as ShaderTranslator;
                result.Process();

                return result;
            }

            return null;
        }

		public static byte[] Build(string source, ShaderVersion shaderVersion, bool optimize)
        {
            var result = Process(source, shaderVersion);

            if (result == null)
                throw new ArgumentNullException("Shader translator", "Shader translator for " + shaderVersion.ToString() + " platform is null");

			return result.Build(optimize);
        }

		public static byte[] BuildAndShowOutput(string source, ShaderVersion shaderVersion, bool optimize, out string output)
		{
			var result = Process(source, shaderVersion);

			output = result.ShowOutput();

			if (result == null)
				throw new ArgumentNullException("Shader translator", "Shader translator for " + shaderVersion.ToString() + " platform is null");

			return result.Build(optimize);
		}

        public static string ShowOutput(string source, ShaderVersion shaderVersion)
        {
            var result = Process(source, shaderVersion);

            if (result == null)
                throw new ArgumentNullException("Shader translator", "Shader translator for " + shaderVersion.ToString() + " platform is null");

            return result.ShowOutput();
        }
    }
}
