
using Lykke.Service.GoogleDriveUpload.Client.AutorestClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.GoogleDriveUpload.Client
{
    public interface IGoogleDriveUploadClient
    {
        Task<IList<FolderPath>> GetFolderPathsAsync();
        Task<string> UploadFileAsync(string fileName, byte[] fileData, string ParentFolderId);
        Task<IList<FilePermission>> GetPermissionsAsync(string fileId);
        Task<FilePermission> AddOrUpdatePermissionAsync(string fileId, string emailAddress);
        Task<bool> RemovePermissionAsync(string fileId, string emailAddress);
    }
}