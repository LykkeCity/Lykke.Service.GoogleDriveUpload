using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.GoogleDriveUpload.Core.Domain.GoogleDrive
{
    public class FolderPath
    {
        public string GoogleId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public override string ToString() => Path;
    }
}
