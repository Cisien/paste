using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Paste.Shared
{
    public class PasteDbContext : DbContext
    {
        private readonly string _basePath;

        public PasteDbContext() 
            => _basePath = MagicValues.DefaultBasePath;

        public PasteDbContext(IConfiguration config) 
            => _basePath = config["BasePath"] ?? MagicValues.DefaultBasePath;

        public PasteDbContext(DbContextOptions<PasteDbContext> contextOptions, IConfiguration config) : base(contextOptions) 
            => _basePath = config["BasePath"] ?? MagicValues.DefaultBasePath;

        public virtual DbSet<Upload> Uploads { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
            optionsBuilder.UseSqlite($"Data Source={_basePath}/paste.db");
        }
    }
}
