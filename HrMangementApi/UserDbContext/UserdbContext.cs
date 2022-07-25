using HrMangementApi.Model;
using Microsoft.EntityFrameworkCore;

namespace HrMangementApi.UserDbContext
{
    public class UserdbContext : DbContext
    {
        public UserdbContext(DbContextOptions<UserdbContext> Options) : base(Options)
        {
        }
        public DbSet<Login> LoginModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBulider)

        {
            modelBulider.Entity<Login>().ToTable("login");
        }
    }
}
