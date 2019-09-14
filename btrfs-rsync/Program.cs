using System;

namespace btrfs_rsync
{
    class Program
    {

        static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("BTRFS_RSYNC_ENVIRONMENT");

            var tool = new Tool(env != null && env == "Development");

            tool.Run(args);
        }

    }

}
