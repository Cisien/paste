using Paste.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Paste.Server.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using IOFile = System.IO.File;
using System.Security.Claims;
using System.Web;

namespace Paste.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SubmitController : ControllerBase
    {
        private readonly PasteDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly string _basePath;

        public SubmitController(PasteDbContext context, IConfiguration config, ITokenService tokenService)
        {
            _db = context;
            _tokenService = tokenService;
            _basePath = config["BasePath"] ?? "/app/data";
        }

        [HttpPost]
        [Authorize(Roles = nameof(Permissions.UploadFiles) + "," + nameof(Permissions.FilesAdmin) + "," + nameof(Permissions.Administrator))]
        [RequestSizeLimit(100_000_000_000)]
        public async Task<ActionResult<SubmitResponse>> Post(IFormFile file)
        {
            var callerTokenClaim = User.FindFirst(ClaimTypes.Actor);
            if (callerTokenClaim == null)
            {
                return Unauthorized();
            }

            var userToken = await _tokenService.GetTokenAsync(callerTokenClaim.Value);
            if (userToken == null)
            {
                return Unauthorized();
            }

            var filename = GetRandomFileNameWithoutExt();
            var filePath = Path.Combine(_basePath, filename);
            await SaveFile(file, filePath);

            var upload = new Upload
            {
                Id = filename,
                Name = file.FileName,
                ContentType = file.ContentType,
                Timestamp = DateTimeOffset.UtcNow,
                Owner = userToken
            };

            await _db.Uploads.AddAsync(upload);
            await _db.SaveChangesAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}/";
            var ext = Path.GetExtension(file.FileName);
            return Ok(new SubmitResponse(string.Concat(baseUrl, "f/", filename, ext)));
        }

        [HttpPost("[action]")]
        [Authorize(Roles = nameof(Permissions.UploadNamedFiles) + "," + nameof(Permissions.NamedFilesAdmin) + "," + nameof(Permissions.Administrator))]
        [RequestSizeLimit(100_000_000_000)]
        public async Task<ActionResult<SubmitResponse>> PostSpecific(IFormFile file)
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized();
            }

            var callerTokenClaim = User.FindFirst(ClaimTypes.Actor);
            if (callerTokenClaim == null)
            {
                return Unauthorized();
            }

            var userToken = await _tokenService.GetTokenAsync(callerTokenClaim.Value);
            if (userToken == null)
            {
                return Unauthorized();
            }

            if (file.FileName.Contains(".."))
            {
                return BadRequest(new { Error = "Malformed filename" });
            }

            var filename = Path.GetFileNameWithoutExtension(file.FileName);
            var filePath = Path.Combine(_basePath, User.Identity.Name, filename);
            await SaveFile(file, filePath);

            var id = Path.Combine(User.Identity.Name, filename).Replace("\\", "/");
            var existingUpload = await _db.Uploads.Include(a => a.Owner).SingleOrDefaultAsync(a => a.Id == id);
            if (existingUpload == null)
            {
                existingUpload = new Upload
                {
                    Id = id,
                    Name = file.FileName,
                    Owner = userToken,
                };

                _db.Uploads.Add(existingUpload);
            }

            if (existingUpload.Owner.Id != userToken.Id)
            {
                return Forbid();
            }

            existingUpload.ContentType = file.ContentType;
            existingUpload.Timestamp = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}/";
            return Ok(new SubmitResponse(string.Concat(baseUrl, $"f/{HttpUtility.UrlEncode(User.Identity.Name)}", "/", file.FileName)));
        }

        private async Task SaveFile(IFormFile file, string filename)
        {
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            var withoutfile = Path.GetDirectoryName(filename)!;
            if (!Directory.Exists(withoutfile))
            {
                Directory.CreateDirectory(withoutfile);
            }

            if (IOFile.Exists(filename))
            {
                IOFile.Delete(filename);
            }

            using var fileContents = IOFile.OpenWrite(filename);
            using var fileStream = file.OpenReadStream();

            await fileStream.CopyToAsync(fileContents);
        }

        private static string GetRandomFileNameWithoutExt()
        {
            var randomName = Path.GetRandomFileName();
            return randomName.Substring(0, randomName.IndexOf("."));
        }
    }
}
