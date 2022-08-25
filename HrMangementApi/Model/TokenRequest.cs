using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class TokenRequest
    {
        [Key]
        public int TokenId { get; set; }
        public string AdminUserName { get; set; }
        public string AdminPassword { get; set; }
        public string Token { get; set; }
    }
}
