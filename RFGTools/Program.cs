using System;
using System.IO;
using System.Windows.Forms;

namespace RFGFormats
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Args:
            Default: PackfileUnpacker.exe InputFile [OutputFolder] ... Auto unpacks to folder with the same name if not OutputFolder provided, maybe strip off extension for less confusion
            -a: Unpack all in folder, args: PackfileUnpacker.exe InputFolder [OutputFolder] ... If no OutputFolder provided, unpack to ./unpack/packfilename, perhaps with extension stripped
            Consider adding option to auto detect wanted behavior depending on if a folder or file is dropped on it.
            Have messagebox pop up to confirm action on activities which might take forever.
            */


            /*
            if(args.Length > 1)
            {

            }
            string PackfilePath = @"B:\RFG Unpack\Unpack\unpack1\vehicles_r\civ_truck1.ccar_pc.str2_pc";//@"B:\RFG Unpack\data\misc.vpp_pc";
            string OutputPath = @"B:\RFG Unpack\Unpack\unpack1\vehicles_r\civ_truck1_alt\";
            var Packfile = new Packfile3();
            Packfile.Deserialize(PackfilePath, OutputPath);*/

            FileInfo[] DataFolder = new DirectoryInfo(@"B:\RFG Unpack\data\").GetFiles();
            foreach(var File in DataFolder)
            {
                var Packfile = new Packfile3();
                string PackfilePath = File.FullName;
                string OutputFolderPath = File.Directory.Parent.FullName + @"\Unpack\unpack2\" + File.Name + @"\";
                Packfile.Deserialize(PackfilePath, OutputFolderPath);
            }
            Console.WriteLine("Complete. Press any key to close.");
            Console.ReadKey(); //Waits for keypress to exit.
        }
    }
}
