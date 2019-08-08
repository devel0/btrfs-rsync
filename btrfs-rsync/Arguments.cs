using System;
using System.IO;
using System.Linq;

namespace btrfs_rsync
{

    public enum RunMode
    {
        dryRun,
        normal
    }

    public partial class Tool
    {

        void PrintUsage()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($@"
Usage: ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{AppDomain.CurrentDomain.FriendlyName} [OPTIONS] SOURCE TARGET");

            Console.ResetColor();
            Console.WriteLine($@"
Synchronize btrfs SOURCE filesystem with given TARGET.
SOURCE and TARGET must mounted btrfs filesystem path.");

            Console.WriteLine($@"
 Mandatory:

  SOURCE        source path
  TARGET        target path

 Optional:

  --dry-run     list sync actions without apply (simulation mode)
");
        }

        string SourcePath { get; set; }

        string TargetPath { get; set; }

        RunMode RunMode { get; set; }

        /// <summary>
        /// check command line arguments
        /// </summary>        
        bool ParseArgs(string[] args)
        {
            var options = args.Where(r => r.StartsWith("-")).ToArray();
            var nonOptions = args.Where(r => !r.StartsWith("-")).ToArray();

            if (nonOptions.Length != 2)
            {
                PrintErr($"Invalid SOURCE or TARGET given");
                return false;
            }

            SourcePath = nonOptions[0];
            TargetPath = nonOptions[1];

            if (!Directory.Exists(SourcePath))
            {
                PrintErr($"SOURCE directory [{SourcePath}] not exists");
                return false;
            }

            if (!Directory.Exists(TargetPath))
            {
                PrintErr($"TARGET directory [{TargetPath}] not exists");
                return false;
            }

            RunMode = RunMode.normal;

            foreach (var opt in options)
            {
                if (opt == "--dry-run")
                    RunMode = RunMode.dryRun;
                else
                {
                    PrintErr($"unknown given option [{opt}]");
                    return false;
                }
            }             

            return true;
        }

    }

}