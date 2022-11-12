using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace HrMangementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class EmployeeController : ControllerBase
    {
        private readonly UserdbContext dataContext;
        public EmployeeController(UserdbContext _dataContext)
        {
            dataContext = _dataContext;
        }

        [HttpGet("AllEmployee")]
        public IActionResult AllEmployee()
        {
            var details = dataContext.EmployeeModel.AsQueryable();
            return Ok(details);
        }
        [HttpGet("getIndividualEmployeeDetailsById")]
        public IActionResult getIndividualEmployeeDetailsById(int id)
        {
            var details = dataContext.EmployeeModel.Where(a => a.EmployeeId == id).AsNoTracking().FirstOrDefault();
            return Ok(details);
        }

        [HttpPost("AddEmployee")]
        public IActionResult AddEmployee(string login, int RepId, [FromBody] EmployeeDetails employeeData)
        {
            if (login == "none")
            {
                dataContext.EmployeeModel.Add(employeeData);
                dataContext.SaveChanges();
                return Ok(employeeData);
            }
            dataContext.EmployeeModel.Add(employeeData);
            dataContext.SaveChanges();
            string RandomPass = RandomString();
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(RandomPass);
            var data = new Login { EmployeeId = employeeData.EmployeeId, Password = passwordHash, MailId = employeeData.EmailId, Role = login, IsFirstLogin = true, ReportingId = RepId };
            dataContext.LoginModels.Add(data);
            dataContext.SaveChanges();
            SendMail(data.MailId, RandomPass);
            return Ok(employeeData);
        }
        [HttpGet("ValidateEmail")]

        public IActionResult ValidateEmail(string data)
        {
            var email = dataContext.LoginModels.Where(e => e.MailId == data).FirstOrDefault();
            if (email == null) return Ok("No User Found");
            return Ok("User Found");

        }

        private void SendMail(string to, string password)
        {
            string from = "info.rsinfosolution.in@gmail.com"; //From address
            MailMessage message = new MailMessage(from, to);

            string mailbody = "Thank you for registering, use your registered mail and below password for access " + "<b>" + password + "</b><br/>Use Below link to Access for access <br/>http://3.110.106.202/app/user/login";
            message.Subject = "Login Credentials for Registered User";
            message.Body = mailbody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587); //Gmail smtp    
            NetworkCredential basicCredential1 = new
            NetworkCredential("info.rsinfosolution.in@gmail.com", "mrpkyxbcdcyjdezy");
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

        [HttpPut("Update")]
        public IActionResult UpdateEmployee([FromBody] EmployeeDetails EmployeeData)
        {
            var res = dataContext.EmployeeModel.AsNoTracking().FirstOrDefault(a => a.EmployeeId == EmployeeData.EmployeeId);
            if (res == null)
            {
                return NotFound();
            }
            else
            {
                EmployeeData.AttachmentIds = res.AttachmentIds;
                dataContext.Entry(EmployeeData).State = EntityState.Modified;
                dataContext.SaveChanges();
                return Ok(EmployeeData);
            }
        }

        [HttpGet("GetEmployeeDetailsById")]
        public object GetEmployeeDetailsById(int id)
        {
            var res = (from a in dataContext.EmployeeModel
                       join p in dataContext.FileAttachment on a.AttachmentIds equals p.AttachmentId.ToString()
                       where a.EmployeeId == id
                       select new
                       {
                           a.FirstName,
                           a.LastName,
                           a.Gender,
                           a.Designation,
                           a.Address,
                           a.Number,
                           a.EmailId,
                           a.DOB,
                           a.JoiningDate,
                           a.EmployeeReferenceNo,
                           a.WorkMode,
                           p.PhotoName,
                           p.PhotoPath
                       });
            return res;
        }

        [HttpGet("getEmployeeDetailsWithPhoto")]
        public IActionResult GetEmployees(int projId)
        {
            var employee = dataContext.EmployeeModel.ToList();
            var project = dataContext.ProjectMemberModel.ToList();
            var result = employee.Where(p => !project.Any(p2 => p2.EmpId == p.EmployeeId && p2.ProjectId == projId));
            return Ok(result);
        }

        [HttpGet("getEmployeesName")]
        public IActionResult getEmployeesName()
        {
            var data = dataContext.EmployeeModel.Select(x => new { x.EmployeeId, x.FirstName, x.LastName });
            return Ok(data);
        }

        [HttpGet("GetUser")]
        public IActionResult GetUser(int data)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == data).FirstOrDefault();
            var employee = dataContext.EmployeeModel.Where(a => a.EmployeeId == data).FirstOrDefault();
            if (user != null && user.Role == "Admin")
            {
                var res = (from a in dataContext.EmployeeModel
                           join p in dataContext.FileAttachment on a.AttachmentIds equals p.AttachmentId.ToString()
                           where a.IsDeleted == false
                           orderby a.EmployeeId ascending
                           select new
                           {
                               a.EmployeeId,
                               a.FirstName,
                               a.LastName,
                               p.PhotoName,
                               p.PhotoPath,
                               a.TeamName,
                               a.Gender,
                               a.Address,
                               a.AttachmentIds,
                               a.Designation,
                               a.DOB,
                               a.EmailId,
                               a.JoiningDate,
                               a.Number,
                               a.WorkMode
                           }).ToList();
                var result = res.GroupBy(x => x.TeamName).ToList();
                return Ok(result);
            }
            if (user != null && user.Role != "Admin")
            {
                var res = (from a in dataContext.EmployeeModel
                           join p in dataContext.FileAttachment on a.AttachmentIds equals p.AttachmentId.ToString()
                           where a.EmployeeId == data && a.IsDeleted == false
                           select new
                           {
                               a.EmployeeId,
                               a.FirstName,
                               a.LastName,
                               a.Gender,
                               a.Designation,
                               a.Address,
                               a.Number,
                               a.EmailId,
                               a.DOB,
                               a.JoiningDate,
                               a.EmployeeReferenceNo,
                               a.WorkMode,
                               p.PhotoName,
                               p.PhotoPath,
                               a.TeamName
                           }).ToList();
                return Ok(res);
            }
            return BadRequest();
        }

        [HttpDelete("DeleteEmployee")]
        public IActionResult DeleteEmployee(int Id)
        {
            var employee = dataContext.EmployeeModel.Where(a => a.EmployeeId == Id).FirstOrDefault();
            var user = dataContext.LoginModels.Where(a => a.EmployeeId == Id).FirstOrDefault();
            if ((employee != null && employee.IsDeleted == false) && user == null)
            {
                employee.IsDeleted = true;
                dataContext.SaveChanges();
                return Ok();
            }
            if ((employee != null && employee.IsDeleted == false))
            {
                employee.IsDeleted = true;
                user.IsDeleted = true;
                dataContext.SaveChanges();
                return Ok();
            }
            return BadRequest();
        }
    }
}
