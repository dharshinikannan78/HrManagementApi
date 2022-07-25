using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class Login
    {
        [Key]
        public int UserId { get; set; }
        public string MailId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
    }
}
