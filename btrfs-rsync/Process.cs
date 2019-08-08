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

        public static async Task<(int exitcode, string output, string error)> ExecNoRedirect(string cmd,
            IEnumerable<string> args, CancellationToken ct, bool sudo = false) =>
            await Exec(cmd, args, ct, sudo, false, false);

        /// <summary>
        /// start a process in background redirecting standard output, error;
        /// a cancellation token can be supplied to cancel underlying process
        /// </summary>        
        public static async Task<(int exitcode, string output, string error)> Exec(string cmd,
            IEnumerable<string> args, CancellationToken ct, bool sudo = false, bool redirectStdout = true, bool redirectStderr = true)
        {
            var res = await Task<(int exitcode, string output, string error)>.Run(() =>
            {
                var p = new Process();
                p.StartInfo.UseShellExecute = !redirectStdout && !redirectStderr;
                p.StartInfo.RedirectStandardOutput = redirectStdout;
                p.StartInfo.RedirectStandardError = redirectStderr;
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

                if (redirectStdout)
                {
                    p.OutputDataReceived += (s, e) =>
                    {
                        sbOut.AppendLine(e.Data);
                    };
                }

                if (redirectStderr)
                {
                    p.ErrorDataReceived += (s, e) =>
                    {
                        sbErr.AppendLine(e.Data);
                    };
                }

                if (!p.Start()) throw new Exception($"can't run process");

                if (redirectStdout) p.BeginOutputReadLine();
                if (redirectStderr) p.BeginErrorReadLine();

                while (!ct.IsCancellationRequested)
                {
                    if (p.WaitForExit(100)) break;
                }
                if (ct.IsCancellationRequested)
                {
                    System.Console.WriteLine($"cancel requested");
                    p.Kill();
                }

                if (redirectStdout) p.CancelOutputRead();
                if (redirectStderr) p.CancelErrorRead();

                return (p.ExitCode, sbOut.ToString(), sbErr.ToString());
            });

            return res;
        }


    }

}
