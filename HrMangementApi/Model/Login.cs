using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class Login
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
