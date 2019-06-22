using System;
using RFGTools.Formats.Packfile3;

namespace PackfilePacker
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputPath = @"C:\Users\moneyl\RFG Unpack\Unpack\unpack3\steam.vpp_pc\";
            string outputPath = @"C:\Users\moneyl\RFG Unpack\Pack\steam.vpp_pc";

            var Packfile = new Packfile3(true);
            Packfile.Serialize(inputPath, outputPath, true, false);

            Console.WriteLine("\nComplete! Press any key to exit.");
            Console.ReadKey();
        }
    }
}
