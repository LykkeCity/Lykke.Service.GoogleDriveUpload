using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lykke.Service.GoogleDriveUpload.Models
{
    public class GetPermissionsModel
    {
        [Required]
        public string FileId { get; set; }
    }
}
