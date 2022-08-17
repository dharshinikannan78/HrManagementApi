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
        public IActionResult GetAttendance(int data)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == data).FirstOrDefault();
            var attendance = dataContext.AttendanceModel.Where(x => x.EmployeeId == data).AsQueryable();
            if (user != null && user.Role == "Admin")
            {
                var AllUser = dataContext.AttendanceModel.AsQueryable();
                return Ok(AllUser);

            }
            if (user != null && user.Role == "Employee")
            {
                return Ok(attendance);

            }

            return BadRequest();
        }

    }
}
