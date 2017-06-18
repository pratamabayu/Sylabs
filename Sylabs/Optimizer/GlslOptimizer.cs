using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Sylabs.Optimizer
{
    public static class GlslOptimizer
    {
        public static string Optimize(string rawShader, bool openGLES, bool es30, bool vertex)
		{
			string shaderAsString = null;

			GlslOptimizerSharp.Target target = GlslOptimizerSharp.Target.OpenGL;
			GlslOptimizerSharp.ShaderType shaderType = vertex ? GlslOptimizerSharp.ShaderType.Vertex : GlslOptimizerSharp.ShaderType.Fragment;

			if (openGLES)
			{
				if (es30)
					target = GlslOptimizerSharp.Target.OpenGLES30;
				else
					target = GlslOptimizerSharp.Target.OpenGLES20;
			}

			using (var optimizer = new GlslOptimizerSharp.GlslOptimizer(GlslOptimizerSharp.Target.OpenGL))
			{
				var result = optimizer.Optimize(shaderType, rawShader, GlslOptimizerSharp.OptimizationOptions.None);
				shaderAsString = result.OutputCode;
			}

			return shaderAsString;
        }
    }
}
