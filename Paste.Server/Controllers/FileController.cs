using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Paste.Shared;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using IOFile = System.IO.File;

namespace Paste.Server.Controllers
{
    [Route("f")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly PasteDbContext _db;

        public FileController(IConfiguration config, PasteDbContext db)
        {
            _config = config;
            _db = db;
        }

        [HttpGet("{owner}/{filename}.{extension}")]
        [HttpGet("{owner}/{filename}")]
        [HttpGet("{filename}.{extension}")]
        [HttpGet("{filename}")]
        public async Task<ActionResult> GetFile(string? owner, string filename)
        {
            var safePath = ValidatePath(owner, filename);
            if (!safePath)
            {
                return NotFound();
            }
            
            owner = owner == null ? null : HttpUtility.UrlDecode(owner);

            string path = BuildFilePath(owner, filename);

            var dbId = owner == null ? filename : $"{owner}/{filename}";
            var upload = await _db.Uploads.FindAsync(dbId);

            if (upload == null)
            {
                return NotFound();
            }

            if (!IOFile.Exists(path))
            {
                return NotFound();
            }

            upload.Views++;
            upload.LastViewed = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            var method = "inline";
            if (upload.ContentType == MediaTypeNames.Application.Octet)
            {
                method = "attachment";
            }

            Response.Headers.Add("Content-Disposition", $"{method}; filename=\"{upload.Name}\"");

            return File(IOFile.OpenRead(path), upload.ContentType);
        }

        private string BuildFilePath(string? owner, string filename)
        {
            string path;
            if (owner == null)
            {
                path = Path.Combine(_config["BasePath"] ?? MagicValues.DefaultBasePath, $"{filename}");
            }
            else
            {
                path = Path.Combine(_config["BasePath"] ?? MagicValues.DefaultBasePath, owner, $"{filename}");
            }

            return path;
        }

        private static bool ValidatePath(string? owner, string filename)
        {
            if (owner?.Contains("..") ?? false)
            {
                return false;
            }

            if (filename.Contains("..") || filename.EndsWith("."))
            {
                return false;
            }

            return true;
        }
    }
}
