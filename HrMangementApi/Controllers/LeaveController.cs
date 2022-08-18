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
            var diff = (leaveData.EndDate - leaveData.StartDate).TotalDays;
            var noofDays = (int)diff + 1;
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
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == data).FirstOrDefault();
            var leave = dataContext.LeaveModel.Where(x => x.EmployeeId == data).AsQueryable();
            if (user != null && user.Role == "Admin")
            {
                var AllUser = dataContext.LeaveModel.AsQueryable();
                return Ok(AllUser);

            }
            if (user != null && user.Role == "Employee")
            {
                return Ok(leave);

            }

            return BadRequest();
        }   
        }
    }

