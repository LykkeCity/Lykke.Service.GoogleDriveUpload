/*
 Google DEV account:
    login: backoffice_dev@lykke.com
    password:
 
 */


using Common.Log;
using Google.Apis.Drive.v3;
using Lykke.Service.GoogleDriveUpload.Core;
using Lykke.Service.GoogleDriveUpload.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.GoogleDriveUpload.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly GoogleDriveUploadSettings _settings;
        private readonly ILog _log;

        public GoogleDriveService(GoogleDriveUploadSettings settings, ILog log)        {
            _settings = settings;
            _log = log;
        }


        private async Task<List<KeyValuePair<string, string>>> GetFilesList(DriveService service)
        {
            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 100;
            listRequest.Fields = "nextPageToken, files(id, name)";

            var result = new List<KeyValuePair<string, string>>();
            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files =
                (await listRequest.ExecuteAsync())
                .Files;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    Console.WriteLine("{0} ({1})", file.Name, file.Id);
                    result.Add(new KeyValuePair<string, string>(file.Name, file.Id));
                }
            }
            else
            {
                Console.WriteLine("No files found.");
            }

            return result;
        }

    }
}
