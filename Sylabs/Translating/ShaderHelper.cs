using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylabs.Translating
{
    public static class ShaderHelper
    {
        #region Hlsl
        public static bool IsType(string inputTypeTest)
        {
            switch (inputTypeTest)
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
        #endregion

        #region GLSL
        public static string ConvertToGlslType(string inputTypeTest)
        {
            string type = inputTypeTest;

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
				case "half":
					type = "float";
					break;
            }

            return type;
        }
        #endregion
    }
}
