using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult AddAttendance([FromBody] AttendanceDetails Data)
        {
            dataContext.AttendanceModel.Add(Data);
            dataContext.SaveChanges();
            return Ok(Data);
        }
    }
}
