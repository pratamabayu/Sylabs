using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Translating
{
    public sealed class Hlsl_5_0_Translator : HlslTranslator
    {
        #region Constructors
        public Hlsl_5_0_Translator(ShaderParser parser)
            : base(parser)
        {
        }
        #endregion

        #region Methods
        protected override void OnVertexBuild(string entryPoint, string source, bool optimize, out byte[] output)
        {
            Compile(entryPoint, "vs_" + GetCompileVersion(), source, out output);
        }

        protected override void OnPixelBuild(string entryPoint, string source, bool optimize, out byte[] output)
        {
            Compile(entryPoint, "ps_" + GetCompileVersion(), source, out output);
        }

        protected override string GetCompileVersion()
        {
            return "5_0";
        }
        #endregion
    }
}
