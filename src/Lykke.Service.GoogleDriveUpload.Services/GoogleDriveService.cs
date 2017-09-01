
using Common;
using Common.Log;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Lykke.Service.GoogleDriveUpload.Core;
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


            var files = GetFilesListAsync().Result;
            var folders = files
                .Where(f => f.MimeType == FolderMimeType)
                .Select(f => (Name: f.Name, Id: f.Id, Parents: f.Parents))
                .ToList();

            var tmp = GetFolderPaths();
        }

        public List<(string Id, string Path)> GetFolderPaths()
        {
            var files = GetFilesListAsync().Result;
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

            var result = new List<(string Id, string Path)>();
            foreach (var folderPair in folders)
            {
                var path = AbsPath(folderPair.Value, getParent);
                if (!string.IsNullOrEmpty(path))
                    result.Add((folderPair.Key, path));
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

        //private static File GetParent(string id)
        //{
        //    // Check cache
        //    if (files.ContainsKey(id))
        //    {
        //        return files[id];
        //    }

        //    // Fetch file from drive
        //    var request = service.Files.Get(id);
        //    request.Fields = "name,parents";
        //    var parent = request.Execute();

        //    // Save in cache
        //    files[id] = parent;

        //    return parent;
        //}

        private async Task<IList<File>> GetFilesListAsync()
        {
            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 100;
            listRequest.Fields = "nextPageToken, files(id, name, mimeType, parents, properties)";

            return (await listRequest.ExecuteAsync()).Files;
        }

        private async Task<IUploadProgress> UploadFileAsync(string fileName, byte[] fileData, string ParentFolderId)
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

                return uploadProgress;
            }
        }

        /// <summary>
        /// Insert a new permission.
        /// </summary>
        /// <param name="service">Drive API service instance.</param>
        /// <param name="fileId">ID of the file to insert permission for.</param>
        /// <param name="account">
        /// User or group e-mail address, domain name or null for "default" type.
        /// </param>
        /// <param name="type">The value "user", "group", "domain" or "default".</param>
        /// <param name="role">The value "owner", "writer" or "reader".</param>
        /// <returns>The inserted permission, null is returned if an API error occurred</returns>
        private async Task<Permission> InsertPermissionAsync(string fileId, string account, string type = "user", string role = "writer")
        {
            Permission newPermission = new Permission
            {
                EmailAddress = account,
                Type = type,
                Role = role
            };

            try
            {
                return await service.Permissions.Create(newPermission, fileId).ExecuteAsync();
            }
            catch (Exception ex)
            {
                var context = (new { fileId, account, type, role }).ToJson();
                await _log.WriteErrorAsync("GoogleDriveService", "InsertPermissionAsync", context, ex);
            }

            return null;
        }
    }
}
