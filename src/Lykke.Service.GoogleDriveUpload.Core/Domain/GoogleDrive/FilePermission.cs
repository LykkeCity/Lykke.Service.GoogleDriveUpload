using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.GoogleDriveUpload.Core.Domain.GoogleDrive
{
    public class FilePermission
    {
        public string GoogleId { get; set; }
        public string Domain { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public Role Role { get; set; }

        [JsonIgnore]
        public string Type { get; set; }
    }
}
