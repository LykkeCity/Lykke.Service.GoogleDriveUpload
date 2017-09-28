using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.GoogleDriveUpload.Core.Domain.GoogleDrive
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Role
    {
        Unknown = 0,

        Organizer = 10,
        Owner = 20,
        Writer = 30,
        Commenter = 40,
        Reader = 50
    }
}
