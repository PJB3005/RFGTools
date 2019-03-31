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
            Add verbose option
            Add recursive unpack option
            */


            /*
            if(args.Length > 1)
            {

            }
            string PackfilePath = @"B:\RFG Unpack\Unpack\unpack1\vehicles_r\civ_truck1.ccar_pc.str2_pc";//@"B:\RFG Unpack\data\misc.vpp_pc";
            string OutputPath = @"B:\RFG Unpack\Unpack\unpack1\vehicles_r\civ_truck1_alt\";
            var Packfile = new Packfile3();
            Packfile.Deserialize(PackfilePath, OutputPath);*/

            //Todo: Validate vpp unpacks, then unpack all str2, get file types, and validate those
            FileInfo[] DataFolder = new DirectoryInfo(@"B:\RFG Unpack\data\vehicles_r2\").GetFiles();
            foreach(var File in DataFolder)
            {
                if(File.Extension == ".vpp_pc" || File.Extension == ".str2_pc")
                {
                    try
                    {
                        var Packfile = new Packfile3();
                        string PackfilePath = File.FullName;
                        string OutputFolderPath = File.Directory.Parent.FullName + @"\Unpack\unpack3\" + File.Name + @"\";
                        Packfile.Deserialize(PackfilePath, OutputFolderPath);
                    }
                    catch (Exception Ex)
                    {
                        //MessageBox.Show("Exception caught while unpacking " + File.Name + ": " + Ex.Message);
                    }
                }
            }
            Console.WriteLine("Complete. Press any key to close.");
            Console.ReadKey(); //Waits for keypress to exit.
        }
    }
}
