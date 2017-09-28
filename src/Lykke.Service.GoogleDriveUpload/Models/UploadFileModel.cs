using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lykke.Service.GoogleDriveUpload.Models
{
    public class UploadFileModel
    {
        /// <value>The FileData property gets/sets the data of file encoded with Base64</value>
        [Required]
        public string FileData { get; set; }

        /// <value>The FileName property gets/sets the name of file</value>
        [Required]
        public string FileName { get; set; }

        /// <value>The ParentFolderGoogleId property gets/sets ID of file's parent folder obtained from Google Drive REST API</value>
        [Required]
        public string ParentFolderGoogleId { get; set; }
    }
}
