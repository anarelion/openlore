using Godot;
using Godot.Collections;
using OpenLore.resource_manager.wld_file.data_types;
using OpenLore.resource_manager.wld_file.fragments;

namespace OpenLore.resource_manager.godot_resources.Intermediates;

[GlobalClass]
public partial class LoreMaterial : ShaderMaterial, ILoreResource<LoreMaterial>
{
    public LoreMaterial()
    {
        SetCode("""
                shader_type spatial;
                void fragment() {
                  ALBEDO = vec3(0.1, 0.3, 0.5);
                }
                """);
    }

    public LoreMaterial(Frag30MaterialDef frag30, EqResourceLoader loader)
    {
        ResourceName = frag30.Name;
        var uniforms = "";
        var fragment = "";
        Array<string> renderModes = [];
        switch (frag30.ShaderType)
        {
            case ShaderTypeEnumType.Boundary:
            case ShaderTypeEnumType.Invisible:
                SetCode("""
                        shader_type spatial;
                        void fragment() {
                            ALPHA = 0.0;
                        }
                        """);
                return;
            case ShaderTypeEnumType.TransparentAdditive:
                renderModes.Add("blend_add");
                break;
            default:
                renderModes.Add("blend_mix");
                break;
        }

        // var texture = loader.Textures.Get(frag30.SimpleSprite.SimpleSpriteDef.Index);
        // if (frag30.SimpleSprite.SimpleSpriteDef.Animated)
        // {
        //     uniforms += """
        //                 uniform sampler2DArray textures;
        //                 uniform int step_time;
        //                 uniform int total_time;
        //                 """;
        //     fragment += """
        //                 int texture_number = (int(TIME * 1000.0) % total_time) / step_time;
        //                 vec4 texture_color = texture(textures, vec3(UV, float(texture_number)));
        //                 """;
        //     SetShaderParameter("textures", (Texture2DArray)texture);
        //     SetShaderParameter("step_time", frag30.SimpleSprite.SimpleSpriteDef.AnimationDelayMs);
        //     SetShaderParameter("total_time", frag30.SimpleSprite.SimpleSpriteDef.AnimationDelayMs * texture.Count);
        // }
        // else
        // {
        //     uniforms += """
        //                 uniform sampler2D albedo;
        //                 """;
        //     fragment += """
        //                 vec4 texture_color = texture(albedo, UV);
        //                 """;
        //     SetShaderParameter("albedo", (ImageTexture)texture);
        // }
        //
        // SetMeta("render_method", $"0x{frag30.RenderMethod:x}");
        // SetCode(
        //     $"shader_type spatial;\nrender_mode {string.Join(", ", renderModes)};\n{uniforms}\nvoid fragment() {{ {fragment} ALBEDO.rgb = texture_color.rgb;}} ");
    }

    private void SetCode(string code)
    {
        Shader = new Shader()
        {
            Code = code,
        };
    }

    public static LoreMaterial Load(string path)
    {
        throw new System.NotImplementedException();
    }

    public void Save(string path)
    {
        throw new System.NotImplementedException();
    }

    public static string GetExtension()
    {
        throw new System.NotImplementedException();
    }
}