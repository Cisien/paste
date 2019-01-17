using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.StaticFiles;
using Paste.Shared;
using System;
using System.Net.Mime;


namespace Paste.Server
{
    public class DbContentTypeProvider : IContentTypeProvider
    {
        private readonly IServiceProvider _services;

        public DbContentTypeProvider(IServiceProvider services)
            => _services = services;

        public bool TryGetContentType(string subpath, out string contentType)
        {
            Upload upload;
            using(var scope = _services.CreateScope())
            using (var db = scope.ServiceProvider.GetService<PasteDbContext>())
            {
                upload = db.Uploads.Find(subpath.Substring(1));

                if (upload != null)
                {
                    upload.Views++;
                    upload.LastViewed = DateTimeOffset.UtcNow;
                    db.SaveChanges();
                }
                else
                {
                    contentType = null;
                    return false;
                }
            }

            contentType = upload.ContentType;

            return true;
        }

        internal void SetDisposition(StaticFileResponseContext response)
        {
            Upload upload;
            using (var scope = _services.CreateScope())
            using (var db = scope.ServiceProvider.GetService<PasteDbContext>())
            {
                upload = db.Uploads.Find(response.File.Name);
            }

            response.Context.Response.Headers.Remove("Content-Disposition");

            var method = "inline";
            if (upload.ContentType == MediaTypeNames.Application.Octet)
            {
                method = "attachment";
            }

            response.Context.Response.Headers.Add("Content-Disposition", $"{method}; filename=\"{upload.Name}\"");
        }
    }
}
