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
    public class LeaveController : ControllerBase
    {
        private readonly UserdbContext dataContext;
        public LeaveController(UserdbContext _dataContext)
        {
            dataContext = _dataContext;
        }
        [HttpPost("ApplyLeave")]
        public IActionResult ApplyLeave([FromBody] LeaveDetails leaveData)
        {
            leaveData.ApprovalStatus = "Pending";
            var diff = leaveData.EndDate - leaveData.StartDate;
            var noofDays = (int)diff.Days + 1;
            leaveData.NoOfDays = noofDays;
            leaveData.AppliedOn = DateTime.UtcNow.Date;
            dataContext.LeaveModel.Add(leaveData);
            dataContext.SaveChanges();
            return Ok(leaveData);
        }



        [HttpGet("GetAllLeaveDetails")]
        public IActionResult AllLeaveDetails()
        {
            var LeaveDetails = dataContext.LeaveModel.AsQueryable();
            return Ok(LeaveDetails);
        }

        [HttpGet("GetUserLeaveDetails")]
        public IActionResult UserLeaveDetails(int LeaveData)
        {
            var LeaveDetails = dataContext.LeaveModel.Where(a => a.EmployeeId == LeaveData);
            if (LeaveDetails == null)
            {
                return NotFound();
            }
            return Ok(LeaveDetails);
        }

        [HttpPut("UpdateLeaveDetails")]
        public IActionResult UpdateLeaveDetails([FromBody] LeaveDetails LeaveData)
        {
            var res = dataContext.LeaveModel.AsNoTracking().FirstOrDefault(a => a.LeaveId == LeaveData.LeaveId);
            if (res == null)
            {
                return NotFound();
            }
            else
            {
                dataContext.Entry(LeaveData).State = EntityState.Modified;
                dataContext.SaveChanges();
                return Ok();
            }
        }

        [HttpGet("GetLeave")]
        public IActionResult GetLeave(int data)
        {
            var Details = (from a in dataContext.EmployeeModel
                           join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                           where l.EmployeeId == data
                           select new
                           {
                               a.EmployeeId,
                               a.FirstName,
                               a.LastName,
                               a.Designation,
                               l.StartDate,
                               l.EndDate,
                               l.NoOfDays,
                               l.AppliedOn,
                               l.Reason,
                               l.LeaveType,
                               l.LeaveId,
                               l.ApprovalStatus
                           });
            var Detail = (from a in dataContext.EmployeeModel
                          join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                          select new
                          {
                              a.EmployeeId,
                              a.FirstName,
                              a.LastName,
                              a.Designation,
                              l.StartDate,
                              l.EndDate,
                              l.NoOfDays,
                              l.AppliedOn,
                              l.Reason,
                              l.LeaveType,
                              l.LeaveId,
                              l.ApprovalStatus
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
