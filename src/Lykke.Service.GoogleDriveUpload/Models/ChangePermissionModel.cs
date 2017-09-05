using Lykke.Service.GoogleDriveUpload.Core.Domain.GoogleDrive;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lykke.Service.GoogleDriveUpload.Models
{
    public class ChangePermissionModel
    {
        [Required]
        public string FileId { get; set; }


        [Required, EmailAddress]
        public string EmailAddress { get; set; }


        [Required]
        public Role Role { get; set; }
    }
}
