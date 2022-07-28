using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult ApplyLeave([FromBody] LeaveDetails LeaveData)
        {
            dataContext.LeaveModel.Add(LeaveData);
            dataContext.SaveChanges();
            return Ok(LeaveData);
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

    }
}
