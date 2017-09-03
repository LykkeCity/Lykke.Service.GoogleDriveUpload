using Lykke.Service.GoogleDriveUpload.Core.Domain.GoogleDrive;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.GoogleDriveUpload.Core.Services
{
    public interface IGoogleDriveService
    {
        List<FolderPath> GetFolderPaths();
        Task<string> UploadFileAsync(string fileName, byte[] fileData, string ParentFolderId);
        Task<List<FilePermission>> GetPermissions(string fileId);
    }
}
