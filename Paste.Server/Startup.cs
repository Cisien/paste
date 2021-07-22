using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paste.Shared;

namespace Paste.Server
{

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<PasteDbContext>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddAuthentication("token").AddScheme<AuthenticationSchemeOptions, TokenAuthHandler>("token", "token", o => { });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, PasteDbContext db, ITokenService tokenService)
        {
            db.Database.Migrate();
            tokenService.EnsureAdminTokenExists();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();


            app.UseAuthentication().UseAuthorization();

            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
