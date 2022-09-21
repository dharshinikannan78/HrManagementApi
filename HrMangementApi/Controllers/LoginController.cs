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

                /* if (user != null)
                 {
                     var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(SIGNING_KEY, SecurityAlgorithms.HmacSha256);
                     var header = new JwtHeader(credentials);
                     DateTime Exp = DateTime.UtcNow.AddDays(60);
                     int ts = (int)(Exp - new DateTime(1970, 1, 1)).TotalSeconds;
                     var payload = new JwtPayload()
             {
                 {"sub", "testsubject" },
                 {"Name", user.MailId },
                 {"email", user.Password },
                 {"exp" , ts },
                 {"iss" , "https://localhost:44394" },
                 {"aud" , "https://localhost:44394" }

             };
                     var secToken = new JwtSecurityToken(header, payload);
                     var handler = new JwtSecurityTokenHandler();
                     var adminUserName = user.MailId;
                     var adminUserPassword = user.Password;
                     var token = handler.WriteToken(secToken).ToString();
                     Console.WriteLine(token);
                     var finalToken = savetoDb(token, adminUserName, adminUserPassword);
                     return Ok(user);
                 }
                 else
                 {
                     return NotFound(new
                     {
                         StatusCode = 404,
                         Message = "Unauthorized"
                     });
                 }*/


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
