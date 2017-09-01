using Lykke.Service.GoogleDriveUpload.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.GoogleDriveUpload.Controllers
{
    [Route("api/[controller]")]
    public class GoogleDriveController : ControllerBase
    {
        private readonly IGoogleDriveService _service;

        public GoogleDriveController(IGoogleDriveService service)
        {
            _service = service;
        }

        public async Task<ActionResult> Get()
        {
            return Ok(DateTime.Now);
        }

    }
}
