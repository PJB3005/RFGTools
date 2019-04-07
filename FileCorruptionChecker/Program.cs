using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace FileCorruptionChecker
{
    class Program
    {
        static StreamWriter LogFile;
        static uint CorruptedFileCount = 0;
        static uint UnknownFormatCount = 0;
        static uint IgnoredFileCount = 0;
        static uint DetectedFileCount = 0;
        static string[] IgnoredExtensionList = {".wav", ".txt", ".xml", ".xtbl", ".mtbl", ".lua",
        ".gpeg_pc", ".gvbm_pc", ".gsmesh_pc", ".gtmesh_pc", ".gcmesh_pc", ".gcar_pc", ".xsb_pc",
        ".xwb_pc", ".gefct_pc", ".gterrain_pc", ".vint_proj", ".scriptx", ".fsmib", ".png", ".dds",
        ".dtodx", ".gtodx", ".NEW", ".gchk_pc", ".vint_xdoc", ".rig_pc"};

        static void Main(string[] args)
        { 
            LogFile = new StreamWriter(@"./RFG_File_Validation_Log.txt", false);
            var Files = new DirectoryInfo(@"B:\RFG Unpack\Unpack\unpack4\");
            Console.WriteLine("Checking for corrupted files in " + Files.Name + " and it's sub-folders...");
            WalkDirectoryTree(Files);

            LogFile.WriteLine("Complete! Results:");
            LogFile.WriteLine("Number of detected files: " + DetectedFileCount.ToString());
            LogFile.WriteLine("Number of corrupted files: " + CorruptedFileCount.ToString());
            LogFile.WriteLine("Number of unknown formats: " + UnknownFormatCount.ToString());
            LogFile.WriteLine("Number of ignored files: " + IgnoredFileCount.ToString());

            Console.WriteLine("Complete! Results:");
            Console.WriteLine("Number of detected files: " + DetectedFileCount.ToString());
            Console.WriteLine("Number of corrupted files: " + CorruptedFileCount.ToString());
            Console.WriteLine("Number of unknown formats: " + UnknownFormatCount.ToString());
            Console.WriteLine("Number of ignored files: " + IgnoredFileCount.ToString());
            Console.WriteLine("Press any key to close.");
            Console.ReadKey(); //Waits for keypress to exit.
        }

        static void WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                ///log.Add(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                //Console.WriteLine(root.Name + "/> Checking folder for corrupted files...");
                foreach (System.IO.FileInfo File in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().

                    //Console.WriteLine(fi.FullName);
                    DetectedFileCount++;
                    string Extension = File.Extension;
                    if (ExtensionOnIgnoreList(File.Extension))
                    {
                        continue;
                    }
                    var FileInfo = new FileInfo(File.FullName);
                    if (FileInfo.Length == 0)
                    {
                        Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, size is 0 bytes.");
                        LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, size is 0 bytes.");
                        CorruptedFileCount++;
                        continue;
                    }
                    var Stream = new FileStream(File.FullName, FileMode.Open);
                    var Reader = new BinaryReader(Stream);
                    uint Signature = Reader.ReadUInt32();

                    //Unknowns: fsmib, gsmesh, csmesh, ccmesh, xwb_pc, xsb_pc, gterrain_pc
                    if(Extension == ".vpp_pc" || Extension == ".str2_pc")
                    {
                        if(Signature != 1367935694)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".cpeg_pc" || Extension == ".cvbm_pc")
                    {
                        if(Signature != 1447773511) //GEKV
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".asm_pc")
                    {
                        if (Signature != 3203399405)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if(Extension == ".mat_pc")
                    {
                        if (Signature != 2954754766)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".ccmesh_pc")
                    {
                        if (Signature != 4207104425)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".csmesh_pc") 
                    {
                        if (Signature != 3237998097)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".morph_pc")
                    {
                        if (Signature != 195935983)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".ccar_pc")
                    {
                        if (Signature != 28)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".anim_pc")
                    {
                        if (Signature != 1296649793) //ANIM
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".rfgzone_pc" || Extension == ".layer_pc")
                    {
                        if (Signature != 1162760026) //ZONE
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".vint_doc")
                    {
                        if (Signature != 12327)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".vf3_pc")
                    {
                        if (Signature != 1414415958) //VFNT
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".precache")
                    {
                        if (Signature != 0)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".cchk_pc")
                    {
                        if (Signature != 2966351781)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".sim_pc")
                    {
                        if (Signature != 1)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    /*else if (Extension == ".rig_pc")
                    {
                        if (Signature != 0) //Not 100% confirmed, but all the ones I checked started with 0
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }*/
                    else if (File.Name == "bitmaps_pc") //Special case
                    {
                        if (Signature != 5)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".fxo_kg")
                    {
                        if (Signature != 1262658030)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".rfglocatext")
                    {
                        if (Signature != 2823585651)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".vfdvp_pc")
                    {
                        if (Signature != 1346651734) //VFDP (Full thing is VFDPPC but to keep it simple just doing the first 4)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".rfgvp_pc") 
                    {
                        if (Signature != 1868057136) //0JXo
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".cefct_pc")
                    {
                        if (Signature != 1463955767)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".cstch_pc")
                    {
                        if (Signature != 1902830517)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".ctmesh_pc")
                    {
                        if (Signature != 0)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".aud_pc")
                    {
                        if (Signature != 541345366) //VFD PC
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".xgs_pc") 
                    {
                        if (Signature != 1179862872) //XGSF
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else if (Extension == ".le_strings")
                    {
                        if (Signature != 2823585651)
                        {
                            Console.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " is corrupt, invalid signature for file extension.");
                            CorruptedFileCount++;
                        }
                    }
                    else
                    {
                        Console.WriteLine(File.Directory.Name + "/" + File.Name + " has an unknown or unregistered format extension.");
                        LogFile.WriteLine(File.Directory.Name + "/" + File.Name + " has an unknown or unregistered format extension.");
                        UnknownFormatCount++;
                    }
                    /*else if (Extension == ")
                    {
                        if (Signature != 1162760026)
                        {
                            Console.WriteLine(File.Name + " is corrupt, invalid signature for file extension.");
                            LogFile.WriteLine(File.Name + " is corrupt, invalid signature for file extension.");
                        }
                    }*/
                }
                //Console.WriteLine(root.Name + "/> Complete!");
                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Recursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo);
                }
            }
        }
        static bool ExtensionOnIgnoreList(string Extension)
        {
            foreach(var Entry in IgnoredExtensionList)
            {
                if(Extension == Entry)
                {
                    IgnoredFileCount++;
                    return true;
                }
            }
            return false;
        }
    }
}
