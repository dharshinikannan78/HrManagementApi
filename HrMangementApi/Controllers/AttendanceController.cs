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
    [EnableCors("AllowOrigin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly UserdbContext dataContext;
        public AttendanceController(UserdbContext _dataContext)
        {
            dataContext = _dataContext;
        }

        [HttpGet("AllAttendance")]
        public IActionResult AllAttendance()
        {
            var details = dataContext.AttendanceModel.AsQueryable();
            return Ok(details);
        }
        [HttpPost("AddAttendance")]
        public IActionResult AddAttendance([FromBody] AttendanceDetails data)
        {

            data.Date = DateTime.Now;
            data.Status = "Present";
            var diff = data.OutTime - data.InTime;
            var TotalDuration = (int)diff.TotalHours;
            data.WorkDuration = TotalDuration;
            dataContext.AttendanceModel.Add(data);
            dataContext.SaveChanges();
            return Ok(data);
        }



        [HttpGet("GetAttendance")]
        public IActionResult GetLeave(int data)
        {
            var Details = (from a in dataContext.EmployeeModel
                           join l in dataContext.AttendanceModel on a.EmployeeId equals l.EmployeeId
                           where l.EmployeeId == data
                           select new
                           {
                               a.EmployeeId,
                               a.FirstName,
                               a.LastName,
                               a.Designation,
                               l.InTime,
                               l.OutTime,
                               l.WorkDuration,
                               l.Status,
                               l.Date
                           }).ToList();
            var Detail = (from a in dataContext.EmployeeModel
                          join l in dataContext.AttendanceModel on a.EmployeeId equals l.EmployeeId
                          select new
                          {
                              a.EmployeeId,
                              a.FirstName,
                              a.LastName,
                              a.Designation,
                              l.InTime,
                              l.OutTime,
                              l.WorkDuration,
                              l.Status,
                              l.Date
                          }).ToList();
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == data).FirstOrDefault();
            if (user != null && user.Role == "Admin")
            {
                return Ok(Detail);
            }
            if (user != null && user.Role == "Employee")
            {
                return Ok(Details);
            }

            return BadRequest();
        }

    }
}
