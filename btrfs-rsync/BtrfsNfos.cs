using System.Collections.Generic;
using System.IO;
using SearchAThing;

namespace btrfs_rsync
{

    public class BtrfsNfos
    {

        List<BtrfsNfo> entries = new List<BtrfsNfo>();

        public IReadOnlyList<BtrfsNfo> Entries => entries;

        /// <summary>
        /// key: uuid - value: obj
        /// </summary>    
        Dictionary<string, BtrfsNfo> btrfsNfoDict = new Dictionary<string, BtrfsNfo>();

        /// <summary>
        /// key: uuid - value: children objs
        /// </summary>        
        Dictionary<string, List<BtrfsNfo>> btrfsNfoChildrenDict = new Dictionary<string, List<BtrfsNfo>>();

        public BtrfsNfos()
        {
        }

        bool UUIDIsNull(string uuid) => string.IsNullOrEmpty(uuid) || uuid == "-";

        /// <summary>
        /// return parent obj or null if no parent
        /// </summary>
        public BtrfsNfo GetParent(string uuid)
        {
            var nfo = btrfsNfoDict[uuid];
            if (!UUIDIsNull(nfo.ParentUUID))
                return btrfsNfoDict[nfo.ParentUUID];
            else
                return null;
        }

        /// <summary>
        /// retrieve children objects
        /// </summary>
        public IEnumerable<BtrfsNfo> GetChildren(string uuid)
        {
            List<BtrfsNfo> children = null;
            if (btrfsNfoChildrenDict.TryGetValue(uuid, out children))
            {
                foreach (var child in children) yield return child;
            }
            else
                yield break;
        }

        public void Add(string basePath, string relPath, string uuid, string parentUUID, long generation, long genAtCreation)
        {
            // System.Console.WriteLine($"fullpath:[{fullpath}] uuid:[{uuid}] parentUUID:[{parentUUID}]"); // TODO: logger
            var nfo = new BtrfsNfo(this, basePath, relPath, uuid, parentUUID, generation, genAtCreation);
            btrfsNfoDict.Add(uuid, nfo);

            // add this obj as child of parent uuid string
            if (!UUIDIsNull(parentUUID))
            {
                List<BtrfsNfo> children = null;
                if (!btrfsNfoChildrenDict.TryGetValue(parentUUID, out children))
                {
                    children = new List<BtrfsNfo>();
                    btrfsNfoChildrenDict.Add(parentUUID, children);
                }
                children.Add(nfo);
            }
            entries.Add(nfo);
        }
    }

}