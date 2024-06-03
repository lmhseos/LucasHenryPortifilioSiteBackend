using Microsoft.EntityFrameworkCore;
using PersonalSiteBackend.Models;

namespace PersonalSiteBackend.Data
{
    public class RagDbContext : DbContext
    {
        public RagDbContext(DbContextOptions<RagDbContext> options) : base(options)
        {
        }
        
        public DbSet<Document> Documents { get; set; }
        public DbSet<Question> Questions { get; set; }
        
    }
}
