
using Lykke.Service.GoogleDriveUpload.Client.AutorestClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.GoogleDriveUpload.Client
{
    public interface IGoogleDriveUploadClient
    {
        Task<IList<FolderPath>> GetFolderPaths();
        Task<string> UploadFileAsync(string fileName, byte[] fileData, string ParentFolderId);
    }
}