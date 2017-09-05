using System.Collections.Generic;

namespace Lykke.Service.GoogleDriveUpload.Models.Responses
{
    public class IsAliveResponse
    {
        public string Version { get; set; }
        public string Env { get; set; }
        public bool IsDebug { get; set; }
        public IEnumerable<IssueIndicator> IssueIndicators { get; set; }
        public string Name => "Lykke.Service.GoogleDriveUpload";

        public class IssueIndicator
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}
