using System;
using System.Threading;
using System.Threading.Tasks;

namespace btrfs_rsync
{

    public partial class Tool
    {
        public Tool()
        {            
        }

        public Task Run(string[] args)
        {
            if (!ParseArgs(args))
            {
                PrintUsage();
                Environment.Exit(1);
            }            

            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            Console.CancelKeyPress += (s,e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            return Task.Run(async () => {
                while (true)
                {
                    System.Console.WriteLine(DateTime.Now);
                    if (ct.IsCancellationRequested)
                    {
                        System.Console.WriteLine($"CANCEL RQ");
                        break;
                    }
                    await Task.Delay(1000);
                }
            });
        }

    }

}