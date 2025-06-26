using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Omnitrack.Models;

namespace Omnitrack.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        private DbSet<User> users;

        public DbSet<User> GetUsers()
        {
            return users;
        }

        public void SetUsers(DbSet<User> value)
        {
            users = value;
        }

        public DbSet<Omnitrack.Models.Tasks> Tasks { get; set; } = default!;
        
    }
}
