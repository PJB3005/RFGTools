using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RFGFormats;
using RFGTools.CorruptionChecker;

namespace PackfileUnpacker
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

            }*/






            //string PackfilePath = @"C:\Users\moneyl\RFG Unpack\Unpack\unpack3\activities.vpp_pc\je_ad_01.str2_pc";//@"B:\RFG Unpack\data\misc.vpp_pc";
            //string OutputPath = @"C:\Users\moneyl\RFG Unpack\Unpack\unpack3\activities.vpp_pc\Subfiles\je_ad_01.str2_pc\";
            //var Packfile = new Packfile3();
            //Packfile.Deserialize(PackfilePath, OutputPath);

            //Todo: Validate vpp unpacks, then unpack all str2, get file types, and validate those


            var Timer = new Stopwatch();
            Timer.Start();
            //FileInfo[] DataFolder = new DirectoryInfo(@"B:\RFG Unpack\data\").GetFiles();
            FileInfo[] DataFolder =
                new DirectoryInfo(@"C:\Users\moneyl\RFG Unpack\Unpack\").GetFiles("*",
                    SearchOption.AllDirectories);
            //FileInfo[] DataFolder = new DirectoryInfo(@"B:\RFG Unpack\Unpack\unpack2\terr01_l0.vpp_pc\").GetFiles();
            var Checker = new CorruptionChecker();
            foreach (var File in DataFolder)
            {
                
                //if (File.Extension == ".vpp_pc")// || File.Extension == ".str2_pc")
                //{
                //    try
                //    {
                //        var Packfile = new Packfile3();
                //        string PackfilePath = File.FullName;
                //        string OutputFolderPath = File.Directory.Parent.FullName + @"\Unpack\unpack3\" + File.Name + @"\";
                //        Packfile.Deserialize(PackfilePath, OutputFolderPath);
                //    }
                //    catch (Exception Ex)
                //    {
                //        MessageBox.Show("Exception caught while unpacking " + File.Name + ": " + Ex.Message);
                //    }
                //}
            }
            Checker.Check(@"C:\Users\moneyl\RFG Unpack\Unpack\", Console.OpenStandardOutput());
            Timer.Stop();
            Console.WriteLine("Complete! Time elapsed: {0}ms | {1}s", Timer.Elapsed.TotalMilliseconds, Timer.Elapsed.TotalSeconds);
            Console.WriteLine("Press any key to close.");
            //
            Console.ReadKey(); //Waits for keypress to exit.
        }
    }
}
