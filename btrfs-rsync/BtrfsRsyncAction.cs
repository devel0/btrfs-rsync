using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace btrfs_rsync
{

    public enum BtrfsRsyncActionMode
    {
        /// <summary>
        /// rsync content
        /// </summary>
        rsync,
        /// <summary>
        /// snap into subvol
        /// </summary>
        snap,
        /// <summary>
        /// create subvol
        /// </summary>
        createSubvol,
        /// <summary>
        /// delete subvol
        /// </summary>
        deleteSubvol,
        /// <summary>
        /// ensure directories
        /// </summary>
        ensurePath
    }

    public class BtrfsResynActionNfo
    {

        public readonly string SourcePath;
        public readonly string DestPath;

        public BtrfsRsyncActionMode Mode { get; private set; }

        public IEnumerable<string> rsyncExclusions { get; private set; }

        public BtrfsResynActionNfo(BtrfsRsyncActionMode mode, string sourcePath, string destPath, IEnumerable<string> rsyncExclusions = null)
        {
            Mode = mode;
            SourcePath = sourcePath;
            DestPath = destPath;
            if (rsyncExclusions != null)
                this.rsyncExclusions = rsyncExclusions;
            else
                this.rsyncExclusions = new List<string>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();            

            sb.Append($"{Mode}: {SourcePath} -> {DestPath}");

            if (rsyncExclusions.Any())
            {
                sb.AppendLine();
                foreach (var re in rsyncExclusions)
                {
                    sb.AppendLine($"  exclude: [{re}]");
                }
            }
            else
                sb.AppendLine();

            return sb.ToString();
        }

    }

}