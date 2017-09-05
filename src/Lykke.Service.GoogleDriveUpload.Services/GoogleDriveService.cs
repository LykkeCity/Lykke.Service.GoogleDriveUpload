
using Common;
using Common.Log;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Lykke.Service.GoogleDriveUpload.Core;
using Lykke.Service.GoogleDriveUpload.Core.Domain.GoogleDrive;
using Lykke.Service.GoogleDriveUpload.Core.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using File = Google.Apis.Drive.v3.Data.File;

namespace Lykke.Service.GoogleDriveUpload.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {
        /// <summary>The Drive API scopes.</summary>
        private static readonly string[] Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive };

        private static string FolderMimeType = "application/vnd.google-apps.folder";

        private readonly DriveService service;

        private readonly GoogleDriveUploadSettings _settings;
        private readonly ILog _log;

        ~GoogleDriveService()
        {
            if (service != null)
            {
                service.Dispose();
            }
        }

        public GoogleDriveService(GoogleDriveUploadSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;

            GoogleCredential credential = GoogleCredential.FromJson(JsonConvert.SerializeObject(settings.GoogleDriveApiKey)).CreateScoped(Scopes);
            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Lykke GoogleDrive Upload",
            });
        }

        public List<FolderPath> GetFolderPaths()
        {
            var files = GetFilesListAsync(true).Result;
            var folders = files
                .Where(f => f.MimeType == FolderMimeType)
                .ToDictionary(key => key.Id, val => val);

            var getParent = new Func<File, File>(folder => {
                var parentId = folder.Parents?.FirstOrDefault();
                if (string.IsNullOrEmpty(parentId))
                    return null;

                if (folders.ContainsKey(parentId))
                    return folders[parentId];

                return null;
            });

            var result = new List<FolderPath>();
            foreach (var folderPair in folders)
            {
                var path = AbsPath(folderPair.Value, getParent);
                if (!string.IsNullOrEmpty(path))
                    result.Add(new FolderPath() { GoogleId = folderPair.Key, Name = folderPair.Value.Name, Path = path });
            }

            return result;
        }



        private static string AbsPath(File file, Func<File, File> getParent)
        {
            var name = file.Name;

            if ((file.Parents?.Count ?? 0) == 0)
            {
                return name;
            }

            var path = new List<string>();

            while (true)
            {
                var parent = getParent(file);

                // Stop when we find the root dir
                if (parent == null)
                {
                    break;
                }

                path.Insert(0, parent.Name);
                file = parent;
            }
            path.Add(name);
            return path.Aggregate((current, next) => Path.Combine(current, next));
        }
        
        private async Task<IList<File>> GetFilesListAsync(bool isFoldersOnly)
        {
            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 1000;
            listRequest.Fields = "nextPageToken, files(id, name, mimeType, parents, properties)";

            if (isFoldersOnly)
            {
                listRequest.Q = $"mimeType = '{FolderMimeType}'";
            }

            var result = new List<File>();

            while (true)
            {
                var fileListResponse = await listRequest.ExecuteAsync();
                if (fileListResponse == null)
                    break;

                result.AddRange(fileListResponse.Files);

                if (fileListResponse.NextPageToken == null)
                    break;

                listRequest.PageToken = fileListResponse.NextPageToken;
            }

            return result;
        }
        
        public async Task<string> UploadFileAsync(string fileName, byte[] fileData, string ParentFolderId)
        {
            var uploadStream = new MemoryStream(fileData);
            using (uploadStream)
            {
                var file = new File()
                {
                    Name = fileName,
                    Parents = new List<string> { ParentFolderId }
                };

                var request = service.Files.Create(
                    file,
                    uploadStream,
                    System.Net.Mime.MediaTypeNames.Application.Pdf);

                request.Fields = "id";
                request.ChunkSize = ResumableUpload.MinimumChunkSize * 2;

                var uploadProgress = await request.UploadAsync();

                if (uploadProgress.Status == UploadStatus.Completed)
                {
                    //await InsertPermission(service, request.ResponseBody.Id, "oleksandr.lysenko@lykke.com", "user", "writer");
                }

                return request.ResponseBody?.Id;
            }
        }

        /// <summary>
        /// Add new permission or update existed one.
        /// </summary>
        /// <param name="fileId">ID of the file to insert permission for.</param>
        /// <param name="account">
        /// User or group e-mail address, domain name or null for "default" type.
        /// </param>
        /// <param name="role">The value "owner", "writer" or "reader".</param>
        /// <returns>The inserted permission, null is returned if an API error occurred</returns>
        public async Task<FilePermission> AddOrUpdatePermissionAsync(string fileId, string account, Role role = Role.Reader)
        {
            if (fileId == null || role == Role.Unknown)
                return null;

            Permission newPermission = new Permission
            {
                EmailAddress = account,
                Type = "user",
                Role = role.ToString().ToLower()
            };

            try
            {
                var permission = await service.Permissions.Create(newPermission, fileId).ExecuteAsync();
                return new FilePermission() {
                    EmailAddress = permission.EmailAddress,
                    Domain = permission.Domain,
                    Type = permission.Type,
                    GoogleId = permission.Id,
                    ExpirationTime = permission.ExpirationTime,
                    Role = role
                };
            }
            catch (Exception ex)
            {
                var context = (new { fileId, account, role }).ToJson();
                await _log.WriteErrorAsync("GoogleDriveService", nameof(AddOrUpdatePermissionAsync), context, ex);

                return null;
            }
        }

        /// <summary>
        /// Remove permission.
        /// </summary>
        /// <param name="fileId">ID of the file to remove permission for.</param>
        /// <param name="account">
        /// User or group e-mail address, domain name or null for "default" type.
        /// </param>
        /// <returns><c>True</c> if removed. Otherwise <c>False</c></returns>
        public async Task<bool> RemovePermissionAsync(string fileId, string account)
        {
            try
            {
                var allPermissions = await GetAllPermissionsAsync(fileId);

                account = account.Trim().ToLower();
                var permissionsToRemove = allPermissions.Where(p => p.EmailAddress.Trim().ToLower() == account);

                foreach (var permission in permissionsToRemove)
                {
                    var resp = await service.Permissions.Delete(fileId, permission.Id).ExecuteAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                var context = (new { fileId, account }).ToJson();
                await _log.WriteErrorAsync("GoogleDriveService", nameof(RemovePermissionAsync), context, ex);

                return false;
            }
        }

        /// <summary>
        /// Get permissions for file (folder)
        /// </summary>
        /// <param name="fileId">ID of file (folder) obtained from Google API</param>
        /// <returns>Set of permissions</returns>
        public async Task<List<FilePermission>> GetPermissionsAsync(string fileId)
        {
            var allPermissions = await GetAllPermissionsAsync(fileId);
            var result = new List<FilePermission>();

            if (!allPermissions.Any())
                return result;

            var serviceEmailAddress = _settings.GoogleDriveApiKey.client_email.Trim().ToLower();
            var ownerRole = Role.Owner.ToString().ToLower();
            result.AddRange(allPermissions.Where(p => p.EmailAddress.Trim().ToLower() != serviceEmailAddress && p.Role != ownerRole).Select(p => new FilePermission()
            {
                GoogleId = p.Id,
                Domain = p.Domain,
                EmailAddress = p.EmailAddress,
                ExpirationTime = p.ExpirationTime,
                Role = Enum.Parse<Role>(p.Role, true),
                Type = p.Type
            }));
            
            return result;
        }
        
        private async Task<List<Permission>> GetAllPermissionsAsync(string fileId)
        {
            var permissionsListRequest = service.Permissions.List(fileId);
            permissionsListRequest.PageSize = 100;
            permissionsListRequest.Fields = "nextPageToken, permissions(id, domain, emailAddress, expirationTime, role, type)";
            
            var result = new List<Permission>();

            while (true)
            {
                var permissionListResponse = await permissionsListRequest.ExecuteAsync();
                if (permissionListResponse == null)
                    break;

                var serviceEmailAddress = _settings.GoogleDriveApiKey.client_email.Trim().ToLower();
                result.AddRange(permissionListResponse.Permissions);

                if (permissionListResponse.NextPageToken == null)
                    break;

                permissionsListRequest.PageToken = permissionListResponse.NextPageToken;
            }

            return result;
        }
    }
}
