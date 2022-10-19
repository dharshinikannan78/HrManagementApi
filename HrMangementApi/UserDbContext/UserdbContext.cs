using HrMangementApi.Model;
using Microsoft.EntityFrameworkCore;

namespace HrMangementApi.UserDbContext
{
    public class UserdbContext : DbContext
    {
        public UserdbContext(DbContextOptions<UserdbContext> Options) : base(Options) { }
        public DbSet<Login> LoginModels { get; set; }
        public DbSet<AttendanceDetails> AttendanceModel { get; set; }
        public DbSet<EmployeeDetails> EmployeeModel { get; set; }
        public DbSet<LeaveDetails> LeaveModel { get; set; }
        public DbSet<FileAttachmentModel> FileAttachment { get; set; }
        public DbSet<TaskDetails> TaskDetails { get; set; }
        public DbSet<TokenRequest> TokenDetails { get; set; }
        public DbSet<ProjectDetails> ProjectDetail { get; set; }
        public DbSet<ProjectMemberModel> ProjectMemberModel { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBulider)

        {
            modelBulider.Entity<Login>().ToTable("usermanagement");
            modelBulider.Entity<AttendanceDetails>().ToTable("attendance_details");
            modelBulider.Entity<EmployeeDetails>().ToTable("employee_details");
            modelBulider.Entity<LeaveDetails>().ToTable("leave_details");
            modelBulider.Entity<FileAttachmentModel>().ToTable("attachment_file");
            modelBulider.Entity<TaskDetails>().ToTable("task_details");
            modelBulider.Entity<TokenRequest>().ToTable("token_details");
            modelBulider.Entity<ProjectDetails>().ToTable("project_details");
            modelBulider.Entity<ProjectMemberModel>().ToTable("project_members");
        }
    }
}
