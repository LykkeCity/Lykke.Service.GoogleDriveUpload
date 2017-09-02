using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.GoogleDriveUpload.Client.AutorestClient;
using System.Collections.Generic;
using Lykke.Service.GoogleDriveUpload.Client.AutorestClient.Models;

namespace Lykke.Service.GoogleDriveUpload.Client
{
    public class GoogleDriveUploadClient : IGoogleDriveUploadClient, IDisposable
    {
        private readonly ILog _log;
        private GoogleDriveUploadAPI _service;

        public GoogleDriveUploadClient(string serviceUrl, ILog log)
        {
            _log = log;
            _service = new GoogleDriveUploadAPI(new Uri(serviceUrl));
        }


        public async Task<IList<FolderPath>> GetFolderPaths()
        {
            var paths = await _service.AllFolderPathsAsync();
            var result = paths.Paths;

            return paths.Paths;
        }

        public async Task<string> UploadFileAsync(string fileName, byte[] fileData, string parentFolderId)
        {
            var fileDataBase64 = Convert.ToBase64String(fileData, Base64FormattingOptions.None);
            var response = await _service.UploadFileAsync(new UploadFileModel() { FileName = fileName, FileData = fileDataBase64, ParentFolderGoogleId = parentFolderId });
            return response.GoogleId;
        }


        public void Dispose()
        {
            if (_service == null)
                return;
            _service.Dispose();
            _service = null;
        }
    }
}
