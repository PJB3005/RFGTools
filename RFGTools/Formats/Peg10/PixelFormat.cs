
namespace RFGTools.Formats.Peg10
{
    //This enum is from the Gibbed.Volition repo: https://github.com/gibbed/Gibbed.Volition/blob/master/projects/Gibbed.Volition.FileFormats/Peg/PixelFormat.cs
    public enum PixelFormat : ushort
    {
        // ReSharper disable InconsistentNaming
        // Supported by: SR2, RFG, RFA
        //    DX9: D3DFMT_DXT1
        //   DX10: DXGI_FORMAT_BC1_TYPELESS
        DXT1 = 400,

        // Supported by: SR2, RFG, RFA
        //    DX9: D3DFMT_DXT3 (DX9)
        //   DX10: DXGI_FORMAT_BC2_TYPELESS
        DXT3 = 401,

        // Supported by: SR2, RFG, RFA
        //    DX9: D3DFMT_DXT5
        //   DX10: DXGI_FORMAT_BC3_TYPELESS
        DXT5 = 402,

        // Supported by: SR2, RFG, RFA
        //    DX9: D3DFMT_R5G6B5
        //   DX10: DXGI_FORMAT_B5G6R5_UNORM
        R5G6B5 = 403,

        // Supported by: SR2, RFA (DX9)
        //    DX9: D3DFMT_A1R5G5B5
        A1R5G5B5 = 404,

        // Supported by: SR2, RFA (DX9)
        //    DX9: D3DFMT_A4R4G4B4
        A4R4G4B4 = 405,

        // Supported by: SR2, RFA (DX9)
        //    DX9: D3DFMT_R8G8B8
        R8G8B8 = 406,

        // Supported by: SR2, RFG, RFA
        //    DX9: D3DFMT_A8R8G8B8
        //   DX10: DXGI_FORMAT_B8G8R8A8_TYPELESS
        A8R8G8B8 = 407,

        // Supported by: SR2, RFG, RFA (DX9)
        //    DX9: D3DFMT_V8U8
        V8U8 = 408,

        // Supported by: SR2, RFA (DX9)
        //    DX9: D3DFMT_CxV8U8
        CxV8U8 = 409,

        // Supported by: SR2
        //    DX9: D3DFMT_A8
        A8 = 410,

        // XBox2 format?
        Unknown_603 = 603,
        // ReSharper restore InconsistentNaming
    }
}
