using Microsoft.AspNetCore.StaticFiles;
using Paste.Shared;
using System.Net.Mime;

namespace Paste.Server
{
    public class DbContentTypeProvider : IContentTypeProvider
    {

        public bool TryGetContentType(string subpath, out string contentType)
        {
            Upload upload;
            using (var db = new PasteDbContext())
            {
                upload = db.Uploads.Find(subpath.Substring(1));
            }

            if(upload == null)
            {
                contentType = null;
                return false;
            }

            contentType = upload.ContentType;
            return true;
        }

        internal void SetDisposition(StaticFileResponseContext response)
        {
            Upload upload;
            using (var db = new PasteDbContext())
            {
                upload = db.Uploads.Find(response.File.Name);
            }

            response.Context.Response.Headers.Remove("Content-Disposition");

            var method = "inline";
            if(upload.ContentType == MediaTypeNames.Application.Octet)
            {
                method = "attachment";
            }

            response.Context.Response.Headers.Add("Content-Disposition", $"{method}; filename=\"{upload.Name}\"");
        }
    }
}
