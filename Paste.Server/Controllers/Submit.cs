using Paste.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Paste.Server.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Paste.Server.Controllers
{
    [Route("[controller]")]
    public class SubmitController : Controller
    {
        private readonly PasteDbContext _db;
        private readonly string _basePath;

        public SubmitController(PasteDbContext context, IConfiguration config)
        {
            _db = context;
            _basePath = config["BasePath"] ?? "/app/data";
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000_000)]
        public async Task<ActionResult<SubmitResponse>> Post(List<IFormFile> files)
        {
            if((files?.Count ?? 0) > 1)
            {
                return BadRequest();
            }

            var file = files?.FirstOrDefault();
            var filename = GetRandomFileNameWithoutExt();
            var filePath = Path.Combine(_basePath, filename);
            await SaveFile(file, filePath);

            var upload = new Upload
            {
                Id = filename,
                Name = file.FileName,
                ContentType = file.ContentType,
            };

            await _db.Uploads.AddAsync(upload);
            await _db.SaveChangesAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}/";
            return Ok(new SubmitResponse { Url = string.Concat(baseUrl, "f/", filename) });
        }

        private async Task SaveFile(IFormFile file, string filename)
        {
            if(!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            using (var fileContents = new FileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite))
            using (var fileStream = file.OpenReadStream())
            {
                await fileStream.CopyToAsync(fileContents);
            }
        }

        private string GetRandomFileNameWithoutExt()
        {
            var randomName = Path.GetRandomFileName();

            return randomName.Substring(0, randomName.IndexOf("."));
        }
    }
}
