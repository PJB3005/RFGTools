using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CommandLine;
using RFGTools.Formats.Packfile3;

namespace PackfileUnpacker
{
    class Program
    {
        public class Options
        {
            [Value(0, MetaName = "Input", Required = true, HelpText = "Input file or folder.")]
            public IEnumerable<string> Input { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Output verbose log messages.")]
            public bool Verbose { get; set; }

            [Option('r', "recursive", Required = false, HelpText = "After unpacking a packfile, unpack any valid packfiles it contained.")]
            public bool Recursive { get; set; }

            [Option('s', "search", Required = false, HelpText = "Unpack packfiles in the provided folder and it's subfolders.")]
            public bool Search { get; set; }
        }

        static void Main(string[] args)
        {
            //Todo: Add warning when unpacking large amount of files or folders
            //Todo: Add timer
            //Todo: Add support for custom unpack folder/location

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    Console.WriteLine("Debug options output:");
                    Console.WriteLine("Input: {0}", options.Input);
                    Console.WriteLine("Verbose: {0}", options.Verbose);
                    //Console.WriteLine("All: {0}", options.All);
                    Console.WriteLine("Recursive: {0}", options.Recursive);
                    Console.WriteLine("Search: {0}", options.Search);

                    foreach (var input in options.Input)
                    {
                        if (Directory.Exists(input)) //Check if it's a folder
                        {
                            //Since it's a folder, first handle -a and -s options, then -r on each packfile
                            Console.WriteLine("Input exists and is a directory");

                            FileInfo[] inputFolder = { };
                            if (options.Search) //Search for packfiles in subfolders and the provided folder
                            {
                                inputFolder = new DirectoryInfo(input).GetFiles("*", SearchOption.AllDirectories);
                            }
                            else //Only search for packfiles in the provided folder
                            {
                                inputFolder = new DirectoryInfo(input).GetFiles();
                            }

                            foreach (var file in inputFolder)
                            {
                                if (file.Extension == ".vpp_pc" || file.Extension == ".str2_pc")
                                {
                                    string inputPath = file.FullName;
                                    string outputPath = Path.Combine(file.DirectoryName, "Unpack", file.Name);

                                    try
                                    {
                                        var Packfile = new Packfile3(options.Verbose);
                                        Packfile.Deserialize(inputPath, outputPath);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Exception caught while unpacking " + file.Name + ": " + ex.Message);
                                    }

                                    if (options.Recursive)
                                    {
                                        //Check OutputPath for valid packfiles and extract them
                                        FileInfo[] unpackFolder = new DirectoryInfo(outputPath).GetFiles();
                                        foreach (var subfile in unpackFolder)
                                        {
                                            if (subfile.Extension == ".vpp_pc" || subfile.Extension == ".str2_pc")
                                            {
                                                try
                                                {
                                                    string subInputPath = subfile.FullName;
                                                    string subOutputPath = Path.Combine(subfile.DirectoryName, "Subfiles", subfile.Name);
                                                    var subPackfile = new Packfile3(options.Verbose);
                                                    subPackfile.Deserialize(subInputPath, subOutputPath);
                                                }
                                                catch (Exception ex)
                                                {
                                                    MessageBox.Show("Exception caught while unpacking " + subfile.Name + ": " + ex.Message);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (File.Exists(input)) //Check if it's a file
                        {
                            //Since it's a file, can safely ignore -a and -s options
                            Console.WriteLine("Input exists and is a file!");

                            var PackfileInfo = new FileInfo(input);
                            string InputPath = input;
                            string OutputPath = Path.Combine(PackfileInfo.DirectoryName, "Unpack", PackfileInfo.Name);

                            try
                            {
                                var Packfile = new Packfile3(options.Verbose);
                                Packfile.Deserialize(InputPath, OutputPath);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Exception caught while unpacking " + PackfileInfo.Name + ": " + ex.Message);
                            }

                            if (options.Recursive)
                            {
                                //Check OutputPath for valid packfiles and extract them
                                FileInfo[] unpackFolder = new DirectoryInfo(OutputPath).GetFiles();
                                foreach (var file in unpackFolder)
                                {
                                    if (file.Extension == ".vpp_pc" || file.Extension == ".str2_pc")
                                    {
                                        try
                                        {
                                            string subInputPath = file.FullName;
                                            string subOutputPath = Path.Combine(file.DirectoryName, "Subfiles", file.Name);
                                            var subPackfile = new Packfile3(options.Verbose);
                                            subPackfile.Deserialize(subInputPath, subOutputPath);
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show("Exception caught while unpacking " + file.Name + ": " + ex.Message);
                                        }
                                    }
                                }
                            }
                        }
                    }
                });

            Console.WriteLine("\nPress any key to close.");
            Console.ReadKey();
        }
    }
}
