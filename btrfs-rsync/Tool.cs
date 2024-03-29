using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SearchAThing;
using System.Linq;
using Path = System.IO.Path;

namespace btrfs_rsync
{

    public enum RunMode
    {
        dryRun,
        normal
    }

    public partial class Tool
    {

        readonly bool development;

        public Tool(bool development)
        {
            this.development = development;
        }

        #region GetSubVolumeInfo
        async Task<(string uuid, string parentUUID, long generation, long genAtCreation, List<string> snapshots)>
            GetSubVolumeInfo(string SourcePath, string fullpath, CancellationToken ct)
        {
            var res = await UtilToolkit.Exec("btrfs", new[] { "sub", "show", fullpath }, ct, development);

            var uuid = "";
            var parentUUID = "";
            var generation = 0L;
            var genAtCreation = 0L;
            List<string> Snapshots = null;

            foreach (var y in res.Output.Lines())
            {
                if (y.Trim().StartsWith("UUID:"))
                    uuid = y.Trim().StripBegin("UUID:").Trim();
                else if (y.Trim().StartsWith("Parent UUID:"))
                    parentUUID = y.Trim().StripBegin("Parent UUID:").Trim();
                else if (y.Trim().StartsWith("Generation:"))
                    generation = long.Parse(y.Trim().StripBegin("Generation:").Trim());
                else if (y.Trim().StartsWith("Gen at creation:"))
                    genAtCreation = long.Parse(y.Trim().StripBegin("Gen at creation:").Trim());
                else if (y.Trim().StartsWith("Snapshot(s):"))
                    Snapshots = new List<string>();
                else if (Snapshots != null)
                    Snapshots.Add(Path.Combine(SourcePath, y.Trim()));
            }

            if (Snapshots == null) Snapshots = new List<string>();

            return (uuid, parentUUID, generation, genAtCreation, Snapshots);
        }
        #endregion

        async Task<BtrfsNfos> ReadBtrfsNfo(string path)
        {
            var nfos = new BtrfsNfos();

            {
                var res = await UtilToolkit.Exec("btrfs", new[] { "sub", "list", path }, ct, development);

                foreach (var x in res.Output.Lines())
                {
                    var q = x.IndexOf("path ");
                    if (q != -1)
                    {
                        var relpath = x.Substring(q + "path ".Length);
                        var subvolFullpath = Path.Combine(path, relpath);

                        var subvolInfo = await GetSubVolumeInfo(path, subvolFullpath, ct);

                        nfos.Add(path, relpath, subvolInfo.uuid, subvolInfo.parentUUID, subvolInfo.generation, subvolInfo.genAtCreation);
                    }
                }
            }

            return nfos;
        }

        #region RunIt
        async Task RunIt(RunMode RunMode, string SourcePath, string TargetPath, bool SkipSubVolResync, bool skipDeleteSubvol)
        {
            if (!Directory.Exists(SourcePath))
            {
                System.Console.WriteLine($"Source path [{SourcePath}] not found");
                Environment.Exit(1);
                return;
            }

            if (!Directory.Exists(TargetPath))
            {
                System.Console.WriteLine($"Target path [{TargetPath}] not found");
                Environment.Exit(1);
                return;
            }

            var srcNfos = await ReadBtrfsNfo(SourcePath);
            var dstNfos = await ReadBtrfsNfo(TargetPath);

            #region log helper
            var indent = 0;
            Action<BtrfsNfo> logNfo = null;
            logNfo = (entry) =>
            {
                var indentSpaces = " ".Repeat(2 * indent);

                System.Console.WriteLine($@"
{indentSpaces}path:[{entry.Fullpath}]
{indentSpaces}uuid:[{entry.UUID}]
{indentSpaces}parentUUID:[{entry.ParentUUID}]=[{entry.Parent?.Fullpath}]
{indentSpaces}generation:[{entry.Generation}]
{indentSpaces}genAtCreation:[{entry.GenAtCreation}]
{indentSpaces}children:[{entry.Children.Count()}]");

                ++indent;
                foreach (var c in entry.Children.OrderBy(w => w.GenAtCreation))
                {
                    logNfo(c);
                }
                --indent;
            };

            Action header1 = () => { System.Console.WriteLine("=".Repeat(78)); };
            Action header2 = () => { System.Console.WriteLine("-".Repeat(78)); };
            #endregion

            // log
            foreach (var entry in srcNfos.Entries.Where(r => r.Parent == null))
            {
                logNfo(entry);
            }

            // plan work
            var workPlan = new List<BtrfsResynActionNfo>();

            #region plan work actions from source nfos
            Action<BtrfsNfo> planWorkActFromSourceNfos = null;

            planWorkActFromSourceNfos = (entry) =>
            {
                var entryCounterPart = entry.CounterPart(TargetPath);

                if (entry.Parent == null && !Directory.Exists(entryCounterPart))
                {
                    var dstPath = Path.GetDirectoryName(entryCounterPart);
                    if (!Directory.Exists(dstPath))
                    {
                        workPlan.Add(new BtrfsResynActionNfo(BtrfsRsyncActionMode.ensurePath,
                            Path.GetDirectoryName(entry.Fullpath),
                            dstPath));
                    }
                    workPlan.Add(new BtrfsResynActionNfo(BtrfsRsyncActionMode.createSubvol, entry.Fullpath, entryCounterPart));
                }

                if (entry.Children.Any())
                {
                    foreach (var (child, idx, isLast) in entry.Children.OrderBy(w => w.GenAtCreation).WithIndexIsLast())
                    {
                        var childCounterPart = child.CounterPart(TargetPath);
                        var childCounterPartExists = Directory.Exists(childCounterPart);

                        if (childCounterPartExists)
                        {
                            if (!SkipSubVolResync)
                                workPlan.Add(new BtrfsResynActionNfo(BtrfsRsyncActionMode.rsync, child.Fullpath, childCounterPart));
                        }
                        else if (entry.Parent == null)
                        {
                            workPlan.Add(new BtrfsResynActionNfo(BtrfsRsyncActionMode.rsync, child.Fullpath, entryCounterPart));
                            workPlan.Add(new BtrfsResynActionNfo(BtrfsRsyncActionMode.snap, entryCounterPart, childCounterPart));
                        }

                        if (child.Children.Any())
                            planWorkActFromSourceNfos(child);

                        if (isLast)
                        {
                            workPlan.Add(new BtrfsResynActionNfo(BtrfsRsyncActionMode.rsync, entry.Fullpath, entryCounterPart,
                                entry.Children.Select(w => w.CounterPart(TargetPath))));
                        }
                    }
                }
                else
                {
                    workPlan.Add(new BtrfsResynActionNfo(BtrfsRsyncActionMode.rsync, entry.Fullpath, entryCounterPart));
                }
            };

            foreach (var entry in srcNfos.Entries.Where(r => r.Parent == null))
            {
                planWorkActFromSourceNfos(entry);
            }
            #endregion

            #region plan work actions from dest nfos
            Action<BtrfsNfo> planWorkActFromDestNfos = null;

            planWorkActFromDestNfos = (entry) =>
            {
                var entryCounterPart = entry.CounterPart(SourcePath);

                if (!Directory.Exists(entryCounterPart))
                {
                    System.Console.WriteLine($"---> entr:[{entry}] counterpart [{entryCounterPart}]");
                    workPlan.Add(new BtrfsResynActionNfo(BtrfsRsyncActionMode.deleteSubvol, null, entry.Fullpath));
                }
            };

            if (!skipDeleteSubvol)
            {
                foreach (var entry in dstNfos.Entries)//.Where(r => r.Parent == null))
                {
                    planWorkActFromDestNfos(entry);
                }
            }
            #endregion

            #region log workplane
            {
                System.Console.WriteLine();
                header1();
                System.Console.WriteLine($"WORKPLAN");
                header1();
                foreach (var wp in workPlan)
                {
                    System.Console.WriteLine(wp.ToString());
                }
            }
            #endregion

            #region dryrun log helper
            Action<string, IEnumerable<string>> dryRunProg = (p, a) =>
             {
                 System.Console.WriteLine($"{p} {string.Join(" ", a)}");
             };
            #endregion

            System.Console.WriteLine();
            header1();
            System.Console.WriteLine($"RUNNING");
            header1();

            foreach (var wp in workPlan)
            {
                switch (wp.Mode)
                {
                    case BtrfsRsyncActionMode.ensurePath:
                        {
                            // TODO: acl and permission set
                            var cmdprog = "mkdir";
                            var cmdargs = new List<string>() { "-p", wp.DestPath };
                            if (RunMode == RunMode.dryRun)
                                dryRunProg(cmdprog, cmdargs);
                            else
                            {
                                var cmdres = await UtilToolkit.ExecNoRedirect(cmdprog, cmdargs, ct, development);
                            }
                        }
                        break;

                    case BtrfsRsyncActionMode.createSubvol:
                        {
                            var cmdprog = "btrfs";
                            var cmdargs = new List<string>() { "sub", "create", wp.DestPath };
                            if (RunMode == RunMode.dryRun)
                                dryRunProg(cmdprog, cmdargs);
                            else
                            {
                                var cmdres = await UtilToolkit.ExecNoRedirect(cmdprog, cmdargs, ct, development);
                            }
                        }
                        break;

                    case BtrfsRsyncActionMode.deleteSubvol:
                        {
                            var cmdprog = "btrfs";
                            var cmdargs = new List<string>() { "sub", "delete", wp.DestPath };
                            if (RunMode == RunMode.dryRun)
                                dryRunProg(cmdprog, cmdargs);
                            else
                            {
                                var cmdres = await UtilToolkit.ExecNoRedirect(cmdprog, cmdargs, ct, development);
                            }
                        }
                        break;

                    case BtrfsRsyncActionMode.rsync:
                        {
                            var cmdprog = "rsync";
                            var cmdargs = new List<string>();
                            cmdargs.Add("-Aav");
                            cmdargs.Add("--delete");
                            foreach (var excl in wp.rsyncExclusions)
                            {
                                cmdargs.Add($"--exclude={excl}");
                            }
                            cmdargs.Add(wp.SourcePath + "/");
                            cmdargs.Add(wp.DestPath + "/");
                            if (RunMode == RunMode.dryRun)
                                dryRunProg(cmdprog, cmdargs);
                            else
                            {
                                var cmdres = await UtilToolkit.ExecNoRedirect(cmdprog, cmdargs, ct, development);
                            }
                        }
                        break;

                    case BtrfsRsyncActionMode.snap:
                        {
                            var cmdprog = "btrfs";
                            var cmdargs = new List<string>() { "sub", "snap", wp.SourcePath, wp.DestPath };
                            if (RunMode == RunMode.dryRun)
                                dryRunProg(cmdprog, cmdargs);
                            else
                            {
                                var cmdres = await UtilToolkit.ExecNoRedirect(cmdprog, cmdargs, ct, development);
                            }
                        }
                        break;
                }
            }
        }
        #endregion

        CancellationToken ct;

        public void Run(string[] args)
        {
            var cts = new CancellationTokenSource();
            ct = cts.Token;

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            CmdlineParser.Create("Synchronize btrfs SOURCE filesystem with given TARGET.", (parser) =>
            {
                parser.AddShortLong("h", "help", "show usage", null, (item) => item.MatchParser.PrintUsage());

                var dryRunMode = parser.AddShortLong("u", "dry-run", "list sync actions without apply (simulation mode)");
                var skipSnapResync = parser.AddShortLong("n", "skip-snap-resync", "avoid resync existing subvolume snapshots");
                var skipDeleteSubvol = parser.AddShortLong("s", "skip-del-subvol", "avoid to remove destination subvol not present in source");

                var SourcePath = parser.AddMandatoryParameter("source", "source path");
                var TargetPath = parser.AddMandatoryParameter("target", "target path");

                parser.OnCmdlineMatch(() =>
                {
                    Task.Run(async () =>
                    {
                        var runMode = RunMode.normal;
                        if (dryRunMode) runMode = RunMode.dryRun;

                        await RunIt(runMode, SourcePath, TargetPath, skipSnapResync, skipDeleteSubvol);
                    }).Wait();
                });

                parser.Run(args);
            });


        }

    }

}