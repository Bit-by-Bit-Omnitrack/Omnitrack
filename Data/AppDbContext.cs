using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;
using UserRoles.Models.Domain;

namespace UserRoles.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        // This is where I connect the domain User model to the database
        public DbSet<User> UsersTable { get; set; }

        // Identity stuff (like Users, Roles, Claims) is handled automatically by IdentityDbContext<Users>
    }
}
