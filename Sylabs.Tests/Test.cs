using NUnit.Framework;
using System;
using System.IO;

namespace Sylabs.Tests
{
	public class Test
	{
		static void Compile(string shader, ShaderVersion version, bool optimize)
		{
			var workingDirectory = TestContext.CurrentContext.TestDirectory;

			var source = File.ReadAllText(Path.Combine(workingDirectory, shader));

			string output = null;
			var compiled = ShaderBuilder.BuildAndShowOutput(source, version, optimize, out output);

			File.WriteAllText(Path.Combine(workingDirectory, shader + "." + version + ".output"), output);
			File.WriteAllBytes(Path.Combine(workingDirectory, shader + "." + version + ".compiled"), compiled);
		}

		static void ShowOutput(string shader, ShaderVersion version)
		{
			var workingDirectory = TestContext.CurrentContext.TestDirectory;

			var source = File.ReadAllText(Path.Combine(workingDirectory, shader));

			var output = ShaderBuilder.ShowOutput(source, version);

			File.WriteAllText(Path.Combine(workingDirectory, shader + "." + version + ".output"), output);
		}

		[TestCase("Shaders/ColorEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/SpriteBatchEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Stock-BasicEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Stock-SkinnedEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-BasicEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-ParticleEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-AlphaTestEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-DualMapEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-SkinnedEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-TerrainEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Deferred-BasicEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Deferred-LightingEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Deferred-SkinnedEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Deferred-TerrainEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-ClearGBufferEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-ReconstructDepthEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-BasicEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-LightingEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-SkinnedEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-TerrainEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-BloomCombine.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-BloomExtract.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Blur.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Chalk.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-DepthOfField.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Distortion.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Emboss.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Flip.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-GaussianBlur.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Grayscale.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Negative.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Ripple.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Wavy.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Wiggle.fx", ShaderVersion.Glsl_120)]
		public void TestShowOutputGlsl(string shader, ShaderVersion version)
		{
			ShowOutput(shader, version);
		}

		[TestCase("Shaders/ColorEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/SpriteBatchEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Stock-BasicEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Stock-SkinnedEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-BasicEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-ParticleEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-AlphaTestEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-DualMapEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-SkinnedEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Forward-TerrainEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Deferred-BasicEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Deferred-LightingEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Deferred-SkinnedEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/Deferred-TerrainEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-ClearGBufferEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-ReconstructDepthEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-BasicEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-LightingEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-SkinnedEffect.fx", ShaderVersion.Glsl_120)]
		//[TestCase("Shaders/LPP/Deferred-TerrainEffect.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-BloomCombine.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-BloomExtract.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Blur.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Chalk.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-DepthOfField.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Distortion.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Emboss.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Flip.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-GaussianBlur.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Grayscale.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Negative.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Ripple.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Wavy.fx", ShaderVersion.Glsl_120)]
		[TestCase("Shaders/PP-Wiggle.fx", ShaderVersion.Glsl_120)]
		public void TestCompileGlsl(string shader, ShaderVersion version)
		{
			Compile(shader, version, true);
		}

		[TestCase("Shaders/ColorEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/SpriteBatchEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Stock-BasicEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Stock-SkinnedEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-BasicEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-ParticleEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-AlphaTestEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-DualMapEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-SkinnedEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-TerrainEffect.fx", ShaderVersion.GlslEs_100)]
		//[TestCase("Shaders/Deferred-BasicEffect.fx", ShaderVersion.GlslEs_100)]
		//[TestCase("Shaders/Deferred-LightingEffect.fx", ShaderVersion.GlslEs_100)]
		//[TestCase("Shaders/Deferred-SkinnedEffect.fx", ShaderVersion.GlslEs_100)]
		//[TestCase("Shaders/Deferred-TerrainEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-BloomCombine.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-BloomExtract.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Blur.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Chalk.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-DepthOfField.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Distortion.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Emboss.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Flip.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-GaussianBlur.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Grayscale.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Negative.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Ripple.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Wavy.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Wiggle.fx", ShaderVersion.GlslEs_100)]
		public void TestShowOutputGlslEs(string shader, ShaderVersion version)
		{
			ShowOutput(shader, version);
		}

		[TestCase("Shaders/ColorEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/SpriteBatchEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Stock-BasicEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Stock-SkinnedEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-BasicEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-ParticleEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-AlphaTestEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-DualMapEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-SkinnedEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/Forward-TerrainEffect.fx", ShaderVersion.GlslEs_100)]
		//[TestCase("Shaders/Deferred-BasicEffect.fx", ShaderVersion.GlslEs_100)]
		//[TestCase("Shaders/Deferred-LightingEffect.fx", ShaderVersion.GlslEs_100)]
		//[TestCase("Shaders/Deferred-SkinnedEffect.fx", ShaderVersion.GlslEs_100)]
		//[TestCase("Shaders/Deferred-TerrainEffect.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-BloomCombine.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-BloomExtract.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Blur.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Chalk.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-DepthOfField.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Distortion.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Emboss.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Flip.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-GaussianBlur.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Grayscale.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Negative.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Ripple.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Wavy.fx", ShaderVersion.GlslEs_100)]
		[TestCase("Shaders/PP-Wiggle.fx", ShaderVersion.GlslEs_100)]
		public void TestCompileGlslEs(string shader, ShaderVersion version, bool optimize = true)
		{
			Compile(shader, version, optimize);
		}

		[TestCase("Shaders/ColorEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/SpriteBatchEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Stock-BasicEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Stock-SkinnedEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-BasicEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-ParticleEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-AlphaTestEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-DualMapEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-SkinnedEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-TerrainEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Deferred-BasicEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Deferred-LightingEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Deferred-SkinnedEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Deferred-TerrainEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-BloomCombine.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-BloomExtract.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Blur.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Chalk.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-DepthOfField.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Distortion.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Emboss.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Flip.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-GaussianBlur.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Grayscale.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Negative.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Ripple.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Wavy.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Wiggle.fx", ShaderVersion.Hlsl_4_0)]
		public void TestShowOutputHlsl(string shader, ShaderVersion version)
		{
			ShowOutput(shader, version);
		}

		[TestCase("Shaders/ColorEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/SpriteBatchEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Stock-BasicEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Stock-SkinnedEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-BasicEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-ParticleEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-AlphaTestEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-DualMapEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-SkinnedEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Forward-TerrainEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Deferred-BasicEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Deferred-LightingEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Deferred-SkinnedEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/Deferred-TerrainEffect.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-BloomCombine.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-BloomExtract.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Blur.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Chalk.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-DepthOfField.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Distortion.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Emboss.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Flip.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-GaussianBlur.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Grayscale.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Negative.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Ripple.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Wavy.fx", ShaderVersion.Hlsl_4_0)]
		[TestCase("Shaders/PP-Wiggle.fx", ShaderVersion.Hlsl_4_0)]
		public void TestCompileHlsl(string shader, ShaderVersion version, bool optimize = true)
		{
			Compile(shader, version, optimize);
		}
    }
}
