using Lykke.Service.GoogleDriveUpload.Core.Domain.GoogleDrive;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.GoogleDriveUpload.Models.Responses
{

    public class GetAllFolderPathsResponse
    {
        public GetAllFolderPathsResponse()
        {
            Paths = new List<FolderPath>();
        }

        public List<FolderPath> Paths { get; set; }
    }
}
