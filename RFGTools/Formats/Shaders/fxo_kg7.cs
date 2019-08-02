using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimShader;

namespace RFGTools.Formats.Shaders
{
    class fxo_kg7
    {
        //rfgr data -- Data prepended to the actual shader which is used by rfg
        uint Signature;
        uint Version;
        uint ShaderFlags;
        ushort NumTexFixups; //Unknown usage //ushort + 2 bytes null. 
        ushort NumConstFixups; //Unknown usage
        ushort NumBoolFixups; //Unknown usage
        char NumVertexShaders;
        char NumPixelShaders;
        int vertex_shaders; //offset
        int pixel_shaders; //offset
        int tex_fixups; //offset
        int const_fixups; //offset
        int bool_fixups; //offset
        char BaseTechniqueIndex;
        char LightingTechniqueIndex;
        char DepthOnlyTechniqueIndex;
        char UserTechniqueStartIndex;
        int ShaderTechniques; //offset
        uint NumTechniques;



        //D3D11 data -- Data from the actual shader that's loaded by D3D11
        //

        void Deserialize(string inputPath, string outputPath)
        {
            
        }
    }
}

//Useful structs dumped from rfg.pdb

//struct rl_shader_header
//{
//    int signature;
//    int version;
//    unsigned int shader_flags;
//    __int16 num_tex_fixups;
//    __int16 num_const_fixups;
//    __int16 num_bool_fixups;
//    char num_vertex_shaders;
//    char num_pixel_shaders;
//    et_ptr_offset<rl_vertex_shader,0> vertex_shaders; //Note: All these et_ptr_offset values are just ints
//    et_ptr_offset<rl_pixel_shader,0> pixel_shaders;
//    et_ptr_offset<rl_shader_tex_fixup,0> tex_fixups;
//    et_ptr_offset<rl_shader_const_fixup,0> const_fixups;
//    et_ptr_offset<rl_shader_const_fixup,0> bool_fixups;
//    char base_technique_index;
//    char lighting_technique_index;
//    char depth_only_technique_index;
//    char user_technique_start_index;
//    et_ptr_offset<rl_shader_technique,0> shader_techniques;
//    int num_techniques;
//};

//struct rl_shader_header_runtime
//{
//    int signature;
//    int version;
//    unsigned int shader_flags;
//    __int16 num_tex_fixups;
//    __int16 num_const_fixups;
//    __int16 num_bool_fixups;
//    char num_vertex_shaders;
//    char num_pixel_shaders;
//    rl_dev_vertex_shader* vertex_shaders;
//    rl_dev_pixel_shader* pixel_shaders;
//    rl_shader_tex_fixup_runtime* tex_fixups;
//    rl_shader_const_fixup* const_fixups;
//    rl_shader_const_fixup* bool_fixups;
//    char base_technique_index;
//    char lighting_technique_index;
//    char depth_only_technique_index;
//    char user_technique_start_index;
//    rl_shader_technique_runtime* shader_techniques;
//    int num_techniques;
//};