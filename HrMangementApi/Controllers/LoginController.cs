using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace HrMangementApi.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private const string SECRET_KEY = "DDFslkgdkgdlmlgkhlkghSDSDkdghjhgkhkglkasjdklajsfkljdsklgjsrjtoriupoeropterp";
        public static readonly SymmetricSecurityKey SIGNING_KEY = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SECRET_KEY));

        private readonly UserdbContext dataContext;
        public LoginController(UserdbContext _dataContext)
        {
            dataContext = _dataContext;
        }

        [HttpPost("Login")]
        public IActionResult GetLogin([FromBody] Login userObj)
        {
            if (userObj == null)
            {
                return BadRequest();
            }
            else
            {
                var user = dataContext.LoginModels.Where(q =>
                q.MailId == userObj.MailId
                && q.Password == userObj.Password).FirstOrDefault();

                if (user != null)
                {
                    return Ok(user);
                }
                else
                {
                    return NotFound(new
                    {
                        StatusCode = 404,
                        Message = "Unauthorized"
                    });
                }
            }
        }
        private TokenRequest savetoDb(string token, string mailId, string password)
        {
            var dataObj = new TokenRequest();
            {
                dataObj.Token = token;
                dataObj.AdminUserName = mailId;
                dataObj.AdminPassword = password;
            }
            dataContext.TokenDetails.Add(dataObj);
            dataContext.SaveChanges();
            return dataObj;
        }

        public class LoginDataType
        {
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
            public int UserId { get; set; }
        }

        [HttpPost("EditLogin")]
        public IActionResult EditLogin(LoginDataType Data)
        {
            var LoginData = dataContext.LoginModels.Where(s => s.UserId == Data.UserId && s.Password == Data.OldPassword).FirstOrDefault();
            if (LoginData == null)
            {
                return BadRequest();
            }
            LoginData.Password = Data.NewPassword;
            LoginData.IsFirstLogin = false;
            dataContext.SaveChanges();
            return Ok(LoginData);
        }

        [HttpPost("AddUser")]
        public IActionResult AddUserLogin([FromBody] Login loginData)
        {

            dataContext.LoginModels.Add(loginData);
            dataContext.SaveChanges();
            return Ok(loginData);
        }


        [HttpDelete("Delete")]
        public IActionResult DeletUser(int id)
        {
            var delete = dataContext.LoginModels.Find(id);
            if (delete == null)
            {
                return NotFound();
            }
            else
            {
                dataContext.LoginModels.Remove(delete);
                dataContext.SaveChanges();
                return Ok();
            }
        }

    }
}
