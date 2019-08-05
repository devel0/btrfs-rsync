using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace btrfs_rsync
{

    public partial class Tool
    {

        static void PrintErr(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        
    }

}