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

        /// <summary>
        /// Return all folders (with paths) available for current account 
        /// </summary>
        /// <returns>List of folders</returns>
        public async Task<IList<FolderPath>> GetFolderPathsAsync()
        {
            var paths = await _service.AllFolderPathsAsync();

            return paths;
        }

        /// <summary>
        /// Upload file to Google Drive
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="fileData">File's content</param>
        /// <param name="parentFolderId">ID of folder where file should be uploaded (ID obtained from Google Drive API)</param>
        /// <returns></returns>
        public async Task<string> UploadFileAsync(string fileName, byte[] fileData, string parentFolderId)
        {
            var fileDataBase64 = Convert.ToBase64String(fileData, Base64FormattingOptions.None);
            var response = await _service.UploadFileAsync(new UploadFileModel() { FileName = fileName, FileData = fileDataBase64, ParentFolderGoogleId = parentFolderId });
            return response.GoogleId;
        }

        /// <summary>
        /// Return permissions for file with ID = <c>fileId</c>
        /// </summary>
        /// <param name="fileId">ID of file (folder) obtained from Google Drive API</param>
        /// <returns>List of permissions</returns>
        public async Task<IList<FilePermission>> GetPermissionsAsync(string fileId)
        {
            var permissions = await _service.GetPermissionsAsync(fileId);
            return permissions;
        }

        /// <summary>
        /// Add new permission or update existing one
        /// </summary>
        /// <param name="fileId">ID of the file to insert/change permission for.</param>
        /// <param name="emailAddress">Account's e-mail address</param>
        /// <returns>Permission</returns>
        public async Task<FilePermission> AddOrUpdatePermissionAsync(string fileId, string emailAddress)
        {
            var permission = await _service.AddOrUpdatePermissionAsync(new ChangePermissionModel() { FileId = fileId, EmailAddress = emailAddress, Role = Role.Reader });

            return permission;
        }

        /// <summary>
        /// Remove permission.
        /// </summary>
        /// <param name="fileId">ID of the file to remove permission for.</param>
        /// <param name="emailAddress">Account's e-mail address</param>
        /// <returns><c>True</c> if removed. Otherwise <c>False</c></returns>
        public async Task<bool> RemovePermissionAsync(string fileId, string emailAddress)
        {
            var isRemoved = await _service.RemovePermissionAsync(new ChangePermissionModel() { FileId = fileId, EmailAddress = emailAddress });

            return isRemoved ?? false;
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
