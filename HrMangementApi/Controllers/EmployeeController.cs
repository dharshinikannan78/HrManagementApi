using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

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
        public IActionResult AddEmployee([FromBody] EmployeeDetails employeeData)
        {
            dataContext.EmployeeModel.Add(employeeData);
            dataContext.SaveChanges();
            return Ok(employeeData);
        }

        [HttpDelete("DeleteEmployee")]
        public IActionResult DeletEmployee(int id)
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
        /*[Authorize]*/
        public IActionResult GetUser(int data)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == data).FirstOrDefault();
            var employee = dataContext.EmployeeModel.Where(x => x.EmployeeId == data).FirstOrDefault();
            if (user != null && user.Role == "Admin")
            {
                var res = (from a in dataContext.EmployeeModel
                           join p in dataContext.FileAttachment on a.AttachmentIds equals p.AttachmentId.ToString()

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
            if (user != null && user.Role == "Employee")
            {
                var res = (from a in dataContext.EmployeeModel
                           join p in dataContext.FileAttachment on a.AttachmentIds equals p.AttachmentId.ToString()
                           where a.EmployeeId == data

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

            return BadRequest();
        }
    }
}
