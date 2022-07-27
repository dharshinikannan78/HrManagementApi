using HrMangementApi.Model;
using Microsoft.EntityFrameworkCore;

namespace HrMangementApi.UserDbContext
{
    public class UserdbContext : DbContext
    {
        public UserdbContext(DbContextOptions<UserdbContext> Options) : base(Options) { }
        public DbSet<Login> LoginModels { get; set; }
        public DbSet<AttendanceDetails> AttendanceModel { get; set;  }
        public DbSet<EmployeeDetails> EmployeeModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBulider)

        {
            modelBulider.Entity<Login>().ToTable("usermanagement");
            modelBulider.Entity<AttendanceDetails>().ToTable("attendance_details");
            modelBulider.Entity<EmployeeDetails>().ToTable("employee_details");
        }
    }
}
