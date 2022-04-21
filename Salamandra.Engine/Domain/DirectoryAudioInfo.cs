using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain
{
    public class DirectoryAudioInfo
    {
        public string DirectoryPath { get; set; }
        public List<string> Files { get; set; }
        public DateTime LastScanDate { get; set; }

        public DirectoryAudioInfo(string directoryPath)
        {
            this.DirectoryPath = directoryPath;
            this.Files = new List<string>();
        }
    }
}
