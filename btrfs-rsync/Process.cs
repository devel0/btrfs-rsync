using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace btrfs_rsync
{

    public static partial class Toolkit
    {

        /// <summary>
        /// start a process in background redirecting standard output, error;
        /// a cancellation token can be supplied to cancel underlying process
        /// </summary>        
        public static Task<(int exitcode, string stdout, string stderr)> Exec(string cmd, IEnumerable<string> args, CancellationToken ct, bool sudo = false)
        {
            var task = Task<(int exitcode, string stdout, string stderr)>.Run(() =>
            {
                var p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                if (sudo)
                {
                    p.StartInfo.FileName = "sudo";
                    p.StartInfo.ArgumentList.Add(cmd);
                }
                else
                    p.StartInfo.FileName = cmd;
                foreach (var a in args) p.StartInfo.ArgumentList.Add(a);

                var sbOut = new StringBuilder();
                var sbErr = new StringBuilder();

                p.OutputDataReceived += (s, e) =>
                {
                    sbOut.AppendLine(e.Data);
                };

                p.ErrorDataReceived += (s, e) =>
                {
                    sbErr.AppendLine(e.Data);
                };

                if (!p.Start()) throw new Exception($"can't run process");

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                while (!ct.IsCancellationRequested)
                {
                    if (p.WaitForExit(100)) break;
                }
                if (ct.IsCancellationRequested)
                {
                    System.Console.WriteLine($"cancel requested");
                    p.Kill();
                }                

                return (p.ExitCode, sbOut.ToString(), sbErr.ToString());
            });

            return task;
        }


    }

}
