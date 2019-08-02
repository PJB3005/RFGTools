using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ShaderTool
{
    class Program
    {
        public class Options
        {
            [Value(0, MetaName = "Path", Required = true, HelpText = "Input file path.")]
            public string Path { get; set; }

            [Option('c', "Compiled", Required = false, HelpText = "Dump compiled shaders for use with tools like SlimShader Studio.")]
            public bool DumpCompiledShaders { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                ExtractRfgDx11Shader(options.Path, options.DumpCompiledShaders);
            });
#if DEBUG
            Console.WriteLine("\nComplete! Press any key to exit.");
            Console.ReadKey();
#endif
        }

        public static void ExtractRfgDx11Shader(string shaderPath, bool dumpCompiledShaders)
        {
            string shaderName = Path.GetFileNameWithoutExtension(shaderPath);
            if (!File.Exists(shaderPath))
            {
                throw new Exception($"\"{shaderPath}\" is not a valid file path!");
            }

            using (var stream = new BinaryReader(new FileStream(shaderPath, FileMode.Open)))
            {
                uint signature = stream.ReadUInt32();
                if (signature != 1262658030)
                {
                    throw new Exception($"Invalid file signature! Expected 1262658030, found {signature}. Check that the vpp_pc file containing the shader was extracted properly.");
                }

                uint version = stream.ReadUInt32();
                if (version != 7)
                {
                    if (version == 9)
                    {
                        throw new Exception($"Unsupported shader format version. Found version 9. This shader file is likely from RFA due to the version.");
                    }
                    else
                    {
                        throw new Exception($"Unsupported shader format version! Expected version 7, found version {version}.");
                    }
                }

                stream.ReadBytes(10); //Skip 10 bytes since those values don't matter for the moment. Ignoring the RFG header info for now.
                uint numVertexShaders = stream.ReadByte();
                uint numPixelShaders = stream.ReadByte();
                uint totalShaderCount = numVertexShaders + numPixelShaders;

                Directory.CreateDirectory("./ShaderDump/");
                for (int i = 0; i < totalShaderCount; i++)
                {
                    SeekToNextDx11Shader(stream);
#if DEBUG
                    Console.WriteLine($"Found dx11 shader header at byte offset {stream.BaseStream.Position}");
#endif
                    stream.ReadBytes(24); //Skip unneeded values
                    int shaderSize = stream.ReadInt32();
                    stream.BaseStream.Seek(-28, SeekOrigin.Current); //Return to shader header

                    var shaderBuffer = stream.ReadBytes(shaderSize);

                    if (dumpCompiledShaders)
                    {
                        File.WriteAllBytes($"./ShaderDump/{shaderName}_compiled{i}.dx11", shaderBuffer);
                    }

                    var bytecodeContainer = new SlimShader.BytecodeContainer(shaderBuffer);
                    File.WriteAllText($"./ShaderDump/{shaderName}_disassembled{i}.txt", bytecodeContainer.ToString());
                }
            }
        }

        public static void SeekToNextDx11Shader(BinaryReader stream)
        {
            while (true)
            {
                if (stream.ReadByte() == 'D')
                {
                    if (stream.ReadByte() == 'X')
                    {
                        if (stream.ReadByte() == 'B')
                        {
                            if (stream.ReadByte() == 'C')
                            {
                                stream.BaseStream.Seek(-4, SeekOrigin.Current);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
