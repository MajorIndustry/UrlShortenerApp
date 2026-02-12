using Microsoft.EntityFrameworkCore;
using UrlShortenerApp.Models;

namespace UrlShortenerApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UrlEntry> UrlEntries { get; set; }
    }
}