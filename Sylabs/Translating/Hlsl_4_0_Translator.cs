using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Translating
{
    public sealed class Hlsl_4_0_Translator : HlslTranslator
    {
        #region Constructors
        public Hlsl_4_0_Translator(ShaderParser parser)
            : base(parser)
        {
        }
        #endregion

        #region Methods
        protected override void OnVertexBuild(string entryPoint, string source, bool optimize, out byte[] output)
        {
#if DEBUG
			System.Diagnostics.Debug.WriteLine("Starting optimze HLSL Vertex Shader " + entryPoint);
			System.Diagnostics.Debug.WriteLine(source);
#endif
            Compile(entryPoint, "vs_" + GetCompileVersion(), source, out output);
        }

        protected override void OnPixelBuild(string entryPoint, string source, bool optimize, out byte[] output)
        {
#if DEBUG
			System.Diagnostics.Debug.WriteLine("Starting optimze HLSL Pixel Shader " + entryPoint);
			System.Diagnostics.Debug.WriteLine(source);
#endif
            Compile(entryPoint, "ps_" + GetCompileVersion(), source, out output);
        }

        protected override string GetCompileVersion()
        {
            return "4_0";
        }
        #endregion
    }
}
