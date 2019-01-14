using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Paste.Shared;
using System.Linq;
using System.Net.Mime;

namespace Paste.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                });
            });

            services.AddDbContext<PasteDbContext>();
            services.AddSingleton<DbContentTypeProvider, DbContentTypeProvider>();
            
            using (var db = services.BuildServiceProvider().GetService<PasteDbContext>())
            {
                db.Database.Migrate();
            }
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration config, DbContentTypeProvider contentTypeProvider)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = contentTypeProvider,
                DefaultContentType = MediaTypeNames.Application.Octet,
                ServeUnknownFileTypes = false,
                OnPrepareResponse = contentTypeProvider.SetDisposition,
                FileProvider = new PhysicalFileProvider(config["BasePath"] ?? "/app/data"),
                RequestPath = "/f"
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller}/{action}/{id?}");
            });
        }
    }
}
