using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs
{
    public enum ShaderVersion
    {
		/// <summary>
		/// The hlsl 4.0 level 9.1 is like SM 2.0
		/// </summary>
        Hlsl_4_0_level_9_1,
		/// <summary>
		/// The hlsl 4.0 level 9.3 is like SM 2.0 for Windows Phone
		/// </summary>
        Hlsl_4_0_level_9_3,
        Hlsl_3_0,
        Hlsl_4_0,
        Hlsl_5_0,
        Glsl_120,
        Glsl_330,
        GlslEs_100,
        GlslEs_300,
    }
}
