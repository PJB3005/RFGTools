using System;
using RFGTools.Formats.Peg10;

namespace ExtractPEG
{
    class Program
    {
        static void Main(string[] args)
        {
            string InputPath = @"C:\Users\moneyl\RFG Unpack\data\Unpack\misc.vpp_pc\always_loaded.cpeg_pc";
            string OutputPath = @"C:\Users\moneyl\RFG Unpack\cpeg_unpack\"; 

            var Pegfile = new Peg10();
            try
            {
                Pegfile.Deserialize(InputPath, OutputPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }

            Console.WriteLine("\nComplete! Press any key to exit.");
            Console.ReadKey();
        }
    }
}
