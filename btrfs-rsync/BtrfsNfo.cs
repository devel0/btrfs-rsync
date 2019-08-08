using System.Collections.Generic;
using System.IO;
using SearchAThing;

namespace btrfs_rsync
{
     
    public class BtrfsNfo
    {
        public string BasePath { get; private set; }
        public string RelPath { get; private set; }
        public string Fullpath => Path.Combine(BasePath, RelPath);
        public string UUID { get; private set; }
        public string ParentUUID { get; private set; }
        public long Generation { get; private set; }
        public long GenAtCreation { get; private set; }

        readonly BtrfsNfos nfos;

        public BtrfsNfo Parent => nfos.GetParent(UUID);

        public IEnumerable<BtrfsNfo> Children => nfos.GetChildren(UUID);

        public string CounterPart(string basePath) => Path.Combine(basePath, RelPath);

        public BtrfsNfo(BtrfsNfos nfos, string basePath, string relPath, string uuid, string parentUUID, long generation, long genAtCreation)
        {
            this.nfos = nfos;

            BasePath = basePath;
            RelPath = relPath;
            UUID = uuid;
            ParentUUID = parentUUID;
            Generation = generation;
            GenAtCreation = genAtCreation;
        }

        public override string ToString()
        {
            return Fullpath;
        }

    }

}