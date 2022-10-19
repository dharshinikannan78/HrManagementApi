using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
                var user = dataContext.LoginModels.Where(q => q.MailId == userObj.MailId).FirstOrDefault();
                if (user != null && BCrypt.Net.BCrypt.Verify(userObj.Password, user.Password))
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

        public class LoginDataType
        {
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
            public int UserId { get; set; }
        }

        [HttpPost("EditLogin")]
        public IActionResult EditLogin(LoginDataType Data)
        {
            var user = dataContext.LoginModels.Where(s => s.UserId == Data.UserId).FirstOrDefault();
            bool verified = BCrypt.Net.BCrypt.Verify(Data.OldPassword, user.Password);
            var LoginData = dataContext.LoginModels.Where(s => s.UserId == Data.UserId && verified).FirstOrDefault();
            if (LoginData == null)
            {
                return BadRequest();
            }
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(Data.NewPassword);
            LoginData.Password = passwordHash;
            LoginData.IsFirstLogin = false;
            dataContext.SaveChanges();
            return Ok(LoginData);
        }

        [HttpGet("ForgotPassword")]
        public IActionResult ForgotPassword(string Data)
        {
            var LoginData = dataContext.LoginModels.Where(s => s.MailId == Data).FirstOrDefault();
            if (LoginData == null)
            {
                return BadRequest();
            }
            LoginData.ResetToken = randomTokenString();
            LoginData.ResetTokenExpires = DateTime.UtcNow.AddDays(1);
            dataContext.SaveChanges();
            string body = "Your Password is resetted successfully<br/> Use below link to reset your password <b>" + LoginData.ResetToken + "</b>";
            const string subject = "Reset Password";
            SendMail(LoginData.MailId, body, subject);
            return Ok(LoginData);
        }
        public class ResetPasswordModel
        {
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
            public string Token { get; set; }
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword(ResetPasswordModel Data)
        {
            var account = dataContext.LoginModels.Where(s => s.ResetToken == Data.Token && s.ResetTokenExpires > DateTime.UtcNow).FirstOrDefault();
            if (account == null)
            {
                return BadRequest();
            }
            account.Password = BCrypt.Net.BCrypt.HashPassword(Data.ConfirmPassword);
            account.ResetToken = null;
            account.ResetTokenExpires = null;

            dataContext.LoginModels.Update(account);
            dataContext.SaveChanges();
            return Ok(new { message = "Password reset successful, you can now login" });
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

        private void SendMail(string to, string body, string subject)
        {
            string from = "mohamedsalmankhan509@gmail.com"; //From address
            MailMessage message = new MailMessage(from, to);

            string mailbody = body;
            message.Subject = subject;
            message.Body = mailbody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("smtp.gmail.com ", 587); //Gmail smtp    
            NetworkCredential basicCredential1 = new
            NetworkCredential("mohamedsalmankhan509@gmail.com", "mlxyocraxfotmeva");
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;
            try
            {
                client.Send(message);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string RandomString()
        {
            Random random = new Random();
            const string chars = "AhghghgkBCDjEjShFWrGwHFvHmIlJpKuUWtsLaWqQxMvNnGhBgLtOLjPQPKaQfQdfRSsBTUYIUVWIAXCYZS01Q2C34H56789";
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string randomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }
        [HttpGet("GetLoginDetails")]
        public IActionResult GetLoginDetails()
        {

            var Employee = (from a in dataContext.LoginModels
                            join x in dataContext.EmployeeModel on a.EmployeeId equals x.EmployeeId
                            where a.Role == "TeamLead"
                            select new
                            {
                                x.FirstName,
                                a.Role,
                                a.UserId
                            }).ToList();
            var TeamLead = (from a in dataContext.LoginModels
                            join x in dataContext.EmployeeModel on a.EmployeeId equals x.EmployeeId
                            where a.Role == "Manager"
                            select new
                            {
                                x.FirstName,
                                a.Role,
                                a.UserId
                            }).ToList();
            var Manager = (from a in dataContext.LoginModels
                           join x in dataContext.EmployeeModel on a.EmployeeId equals x.EmployeeId
                           where a.Role == "Admin"
                           select new
                           {
                               x.FirstName,
                               a.Role,
                               a.UserId
                           }).ToList();


            var ReportingPerson = new
            {
                Employee,
                TeamLead,
                Manager
            };







            return Ok(ReportingPerson);
        }
    }


}




