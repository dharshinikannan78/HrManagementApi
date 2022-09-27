using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class Login
    {
        [Key]
        public int UserId { get; set; }
        public int EmployeeId { get; set; }
        public string MailId { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool IsFirstLogin { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
