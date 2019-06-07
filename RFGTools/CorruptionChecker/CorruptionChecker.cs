using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RFGTools.CorruptionChecker
{
    public class CorruptionChecker
    {
        private List<string> _ignoredFormatList = new List<string>();
        private List<FormatData> _checkedFormatList = new List<FormatData>();

        private uint _corruptedFileCount = 0;
        public uint CorruptedFileCount => _corruptedFileCount;

        private uint _unknownFormatCount = 0;
        public uint UnknownFormatCount => _unknownFormatCount;

        private uint _ignoredFileCount = 0;
        public uint IgnoredFileCount => _ignoredFileCount;

        private uint _detectedFileCount = 0;
        public uint DetectedFileCount => _detectedFileCount;

        public enum CheckType
        {
            SingleFile,
            Folder,
            FolderRecursive
        }

        public class FormatData
        {
            private string _extension;
            public string Extension => _extension;

            private uint _signature;
            public uint Signature => _signature;

            public FormatData(string extension, uint signature)
            {
                _extension = extension;
                _signature = signature;
            }
        }

        public CorruptionChecker()
        {
            //Register extensions to be ignored. These have no corruption detection method yet.
            IgnoreExtension(new []{".wav", ".txt", ".xml", ".xtbl", ".mtbl", ".lua",
                ".gpeg_pc", ".gvbm_pc", ".gsmesh_pc", ".gtmesh_pc", ".gcmesh_pc", ".gcar_pc", ".xsb_pc",
                ".xwb_pc", ".gefct_pc", ".gterrain_pc", ".vint_proj", ".scriptx", ".fsmib", ".png", ".dds",
                ".dtodx", ".gtodx", ".NEW", ".gchk_pc", ".vint_xdoc", ".rig_pc", ".gstch_pc"});

            //Register extensions to be checked. Simply checks the magic signature (the files first 4 bytes)
            RegisterExtension(new []{".vpp_pc", ".str2_pc"}, 1367935694);
            RegisterExtension(new []{".cpeg_pc", ".cvbm_pc"}, 1447773511); //Sig: GEKV
            RegisterExtension(".asm_pc", 3203399405);
            RegisterExtension(".mat_pc", 2954754766);
            RegisterExtension(".ccmesh_pc", 4207104425);
            RegisterExtension(".csmesh_pc", 3237998097);
            RegisterExtension(".morph_pc", 195935983);
            RegisterExtension(".ccar_pc", 28);
            RegisterExtension(".anim_pc", 1296649793); //Sig: ANIM
            RegisterExtension(new []{".rfgzone_pc", ".layer_pc"}, 1162760026); //Sig: ZONE
            RegisterExtension(".vint_doc", 12327);
            RegisterExtension(".vf3_pc", 1414415958); //Sig: VFNT
            RegisterExtension(".precache", 0);
            RegisterExtension(".cchk_pc", 2966351781);
            RegisterExtension(".sim_pc", 1);
            RegisterExtension(".cfmesh_pc", 267501985);
            RegisterExtension(".cterrain_pc", 1381123412); //Sig: TERR
            RegisterExtension(".fxo_kg", 1262658030);
            RegisterExtension(".rfglocatext", 2823585651);
            RegisterExtension(".vfdvp_pc", 1346651734); //Sig: VFDP (Full thing is VFDPPC but to keep it simple just doing the first 4)
            RegisterExtension(".rfgvp_pc", 1868057136); //Sig: 0JXo
            RegisterExtension(".cefct_pc", 1463955767);
            RegisterExtension(".cstch_pc", 2966351781);
            RegisterExtension(".ctmesh_pc", 1514296659); //Sig: SUBZ
            RegisterExtension(".aud_pc", 541345366); //Sig: VFDPC
            RegisterExtension(".xgs_pc", 1179862872); //Sig: XGSF
            RegisterExtension(".le_strings", 2823585651);
        }

        public void Check(string path, Stream outputStream, CheckType type = CheckType.FolderRecursive)
        {
            _corruptedFileCount = 0;
            _unknownFormatCount = 0;
            _ignoredFileCount = 0;
            _detectedFileCount = 0;
        
            var output = new StreamWriter(outputStream);
        
            var directory = new DirectoryInfo(path);
            output.WriteLine("Checking for corrupted files in " + directory.Name + " and it's sub-folders...");

            FileInfo[] fileList = {};
            switch (type)
            {
                case CheckType.SingleFile:
                    fileList = new[] {new FileInfo(path)};
                    break;
                case CheckType.Folder:
                    fileList = new DirectoryInfo(path).GetFiles();
                    break;
                case CheckType.FolderRecursive:
                    fileList = new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "How'd you even do this?");
            }

            foreach (var file in fileList)
            {
                _detectedFileCount++;
                
                if (ExtensionOnIgnoreList(file.Extension))
                {
                    _ignoredFileCount++;
                    continue;
                }
                if (file.Length == 0)
                {
                    output.WriteLine("{0}/{1} is corrupt, size is 0 bytes.", file.Directory.Name, file.Name);
                    _corruptedFileCount++;
                    continue;
                }

                string extension = file.Extension;
                uint signature;
                using (var reader = new BinaryReader(new FileStream(file.FullName, FileMode.Open)))
                {
                    signature = reader.ReadUInt32();
                }

                //Check registered formats
                bool shouldExitLoop = false;
                foreach (var format in _checkedFormatList)
                {
                    if (extension == format.Extension)
                    {
                        if (signature == format.Signature)
                        {
                            shouldExitLoop = true;
                            break;
                        }
                        else
                        {
                            output.WriteLine(@"{0}/{1} is corrupt, invalid signature for file extension.", file.Directory.Name, file.Name);
                            _corruptedFileCount++;
                            shouldExitLoop = true;
                            break;
                        }
                    }
                }
                if (shouldExitLoop)
                {
                    continue;
                }

                if (file.Name == "bitmaps_pc") //Special case
                {
                    if (signature != 5)
                    {
                        output.WriteLine(@"{0}/{1} is corrupt, invalid signature for file extension.", file.Directory.Name, file.Name);
                        _corruptedFileCount++;
                    }
                }
                else
                {
                    output.WriteLine("{0}/{1} has an unknown or unregistered format extension \"{2}\"", file.Directory.Name, file.Name, extension);
                    Console.WriteLine(file.Directory.Name + "/" + file.Name + " has an unknown or unregistered format extension.");
                    _unknownFormatCount++;
                }
            }
            
            output.WriteLine("Complete! Results:");
            output.WriteLine("Number of detected files: " + DetectedFileCount.ToString());
            output.WriteLine("Number of corrupted files: " + CorruptedFileCount.ToString());
            output.WriteLine("Number of unknown formats: " + UnknownFormatCount.ToString());
            output.WriteLine("Number of ignored files: " + IgnoredFileCount.ToString());
            float percentageCorrupted = (float)CorruptedFileCount / (float)DetectedFileCount;
            float maxPercentageCorrupted = ((float)CorruptedFileCount + (float)IgnoredFileCount) / (float)DetectedFileCount;
            output.WriteLine("Percentage corrupted: {0}", percentageCorrupted);
            output.WriteLine("Max percentage corrupted: {0}", maxPercentageCorrupted);
            output.Flush();
        }

        void RegisterExtension(string extension, uint signature)
        {
            _checkedFormatList.Add(new FormatData(extension, signature));
        }

        void RegisterExtension(string[] extension, uint signature)
        {
            foreach (var ext in extension)
            {
                _checkedFormatList.Add(new FormatData(ext, signature));
            }
        }

        void IgnoreExtension(string[] extensions)
        {
            foreach (var extension in extensions)
            {
                _ignoredFormatList.Add(extension);
            }
        }

        bool ExtensionOnIgnoreList(string extension)
        {
            return _ignoredFormatList.Contains(extension);
        }
    }
}