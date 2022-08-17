using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
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

        [HttpGet]
        public IActionResult GetEmployeeDetailsById(int id)
        {
            var res = dataContext.EmployeeModel.AsNoTracking().FirstOrDefault(a => a.EmployeeId == id);
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
                                    p.AttachmentName,
                                    p.AttachmentType,
                                    p.AttachmentPath,
                                    p.AttachmentId,
                                    a.AttachmentIds,
                                    a.Number,
                                    a.EmailId,
                                }).ToList();
            var employees = allemployess.ToList();
            return Ok(employees);

        }
        [HttpGet("EmployeeDetails")]
        public IActionResult GetAttendance(int id)
        {

            var allemployess = (from a in dataContext.LeaveModel
                                join p in dataContext.EmployeeModel on a.EmployeeId equals p.EmployeeId
                                join l in dataContext.AttendanceModel on a.EmployeeId equals l.EmployeeId into groupcls
                                from gc in groupcls.DefaultIfEmpty()
                                where a.EmployeeId == id

                                group gc by new
                                {
                                    id = p.EmployeeId == null ? 0 : p.EmployeeId,
                                    name = a.StartDate,
                                    h = gc.Status,
                                    

                                } into g
                                select new
                                {
                                    name = g.Key.id,
                                    h1 = g.Key.name,
                                    h2 = g.Key.h,
                                    
                                }).ToList();
            return Ok(allemployess);
            /* var employees = allemployess.ToList();
             return Ok(employees);*/

        }
    }
}
