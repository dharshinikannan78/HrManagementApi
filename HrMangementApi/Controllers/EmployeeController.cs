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
using System.Security.Cryptography;
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

        [HttpPost("AddEmployee")]
        public IActionResult AddEmployee(string login, [FromBody] EmployeeDetails employeeData)
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
            var data = new Login { EmployeeId = employeeData.EmployeeId, Password = RandomPass, MailId = employeeData.EmailId, Role = login, IsFirstLogin = true };
            dataContext.LoginModels.Add(data);
            dataContext.SaveChanges();
            SendMail(data.MailId, data.Password);
            return Ok(employeeData);
        }

        private void SendMail(string to, string password)
        {
            string from = "mohamedsalmankhan509@gmail.com"; //From address
            MailMessage message = new MailMessage(from, to);

            string mailbody = "Thank you for registering, use your registered mail and below password for access " + "<b>" + password + "</b>";
            message.Subject = "Login Credentials for Registered User";
            message.Body = mailbody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("smtp.gmail.com	", 587); //Gmail smtp    
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

                dataContext.SaveChanges();
                return Ok();
            }

            return BadRequest();
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
                dataContext.Entry(EmployeeData).State = EntityState.Modified;
                dataContext.SaveChanges();
                return Ok(EmployeeData);
            }
        }

        [HttpGet("GetEmployeeDetailsById")]
        public IActionResult GetEmployeeDetailsById(int id)
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

                       }).ToList();
            return Ok(res);

        }


        [HttpGet("GetEmployeeDetails")]
        public IActionResult GetEmployees()
        {

            var allemployess = (from a in dataContext.EmployeeModel
                                join p in dataContext.FileAttachment on a.AttachmentIds equals p.AttachmentId.ToString()

                                select new
                                {
                                    a.FirstName,
                                    a.LastName,
                                    a.JoiningDate,
                                    a.Designation,
                                    p.PhotoName,
                                    p.PhotoPath,
                                    p.AttachmentId,
                                    a.AttachmentIds,
                                    a.Number,
                                    a.EmailId,
                                }).ToList();
            var employees = allemployess.ToList();
            return Ok(employees);

        }
        /*[HttpGet("EmployeeDetails")]
        public IActionResult GetAttendance(int id)
        {

            var allemployess = (from a in dataContext.LeaveModel
                                join p in dataContext.EmployeeModel on a.EmployeeId equals p.EmployeeId
                                join l in dataContext.AttendanceModel on a.EmployeeId equals l.EmployeeId into groupcls
                                from gc in groupcls.DefaultIfEmpty()
                                where a.EmployeeId == id

                                group gc by new
                                {
                                    name = a.StartDate



                                } into g
                                select new
                                {
                                    h1 = g.Key.name,


                                    h2 = g.Key.h,
                                }).ToList();
            return Ok(allemployess);
            *//* var employees = allemployess.ToList();
             return Ok(employees);*//*

        }*/
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
                               a.Position,
                               a.TeamName,
                               a.Gender,
                               a.Address,
                               a.AttachmentIds,
                               a.College,
                               a.Designation,
                               a.DOB,
                               a.EmailId,
                               a.JoiningDate,
                               a.Number,
                               a.PassedOut,
                               a.Qualification,
                               a.Skills,
                               a.WorkMode

                           }).ToList();

                var result = res.GroupBy(x => x.TeamName).ToList();

                return Ok(result);
            }


            if (user != null && (user.Role == "TeamLeader" || user.Role == "TeamMember"))
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
                               a.TeamName,
                               a.Position
                           }).ToList();
                return Ok(res);

            }

            return BadRequest();
        }
    }
}
