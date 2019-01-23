using System.Collections.Generic;
using System.IO;

namespace filefeed.Models
{
    public class RssListViewModel
    {
        public DirectoryInfo CurrentDir { get; set; }
        public IEnumerable<DirectoryInfo> Directories { get; set; }
        public IEnumerable<FileInfo> Files { get; set; }
        public bool DisplayParentLink { get; set; }
    }
}