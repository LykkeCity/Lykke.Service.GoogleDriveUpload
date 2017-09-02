using Common.Log;
using Lykke.Service.GoogleDriveUpload.Core.Domain.GoogleDrive;
using Lykke.Service.GoogleDriveUpload.Core.Services;
using Lykke.Service.GoogleDriveUpload.Models;
using Lykke.Service.GoogleDriveUpload.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.GoogleDriveUpload.Controllers
{
    [Route("api/[controller]")]
    public class GoogleDriveController : ControllerBase
    {
        private readonly IGoogleDriveService _service;
        private readonly ILog _log;

        public GoogleDriveController(IGoogleDriveService service, ILog log)
        {
            _service = service;
            _log = log;
        }
        
        [HttpGet("AllFolderPaths")]
        [SwaggerOperation("AllFolderPaths")]
        [ProducesResponseType(typeof(List<FolderPath>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetAllFolderPaths()
        {
            try
            {
                var data = _service.GetFolderPaths();
                return Ok(data);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("GoogleDriveController", nameof(GetAllFolderPaths), string.Empty, ex);
                return StatusCode(500, ErrorResponse.Create(ex.Message));
            }
        }

        [HttpPost("UploadFile")]
        [SwaggerOperation("UploadFile")]
        [ProducesResponseType(typeof(UploadFileResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UploadFile([FromBody] UploadFileModel model)
        {
            #region Validation

            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.Create(ModelState));
            }

            #endregion

            try
            {
                var fileData = Convert.FromBase64String(model.FileData);
                var fileId = await _service.UploadFileAsync(model.FileName, fileData, model.ParentFolderGoogleId);
                var response = new UploadFileResponse() { GoogleId = fileId };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("GoogleDriveController", nameof(UploadFile), string.Empty, ex);
                return StatusCode(500, ErrorResponse.Create(ex.Message));
            }
        }
    }
}
