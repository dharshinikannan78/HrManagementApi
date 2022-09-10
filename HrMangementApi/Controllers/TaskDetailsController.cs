using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace HrMangementApi.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TaskDetailsController : ControllerBase
    {
        private readonly UserdbContext dataContext;
        public TaskDetailsController(UserdbContext _dataContext)
        {
            dataContext = _dataContext;
        }

        [HttpPost("AddTaskDeatils")]
        public IActionResult AddTaskDeatils([FromBody] TaskDetails addTask)
        {
            addTask.TaskStatus = "InProgress";
            dataContext.TaskDetails.Add(addTask);
            dataContext.SaveChanges();
            return Ok(addTask);
        }

        [HttpPut("Update")]
        public IActionResult UpdateEmployee([FromBody] TaskDetails addTask)
        {
            var res = dataContext.TaskDetails.AsNoTracking().FirstOrDefault(a => a.ProjectId == addTask.ProjectId);
            if (res == null)
            {
                return NotFound();
            }
            else
            {
                
                dataContext.Entry(addTask).State = EntityState.Modified;
                dataContext.SaveChanges();
                return Ok(addTask);
            }
        }

        [HttpGet("GetAllTask")]
        public IActionResult GetTaskDeatils()
        {
            var details = dataContext.TaskDetails.AsQueryable();
            return Ok(details);
        }

        [HttpGet]
        public IActionResult get()
        {
            var allemployess = (from a in dataContext.EmployeeModel
                                join b in dataContext.TaskDetails on a.EmployeeId equals b.EmployeeId
                                select new
                                {
                                    a.FirstName,
                                    b.TaskName,
                                    b.EmployeeId
                                }).ToList();
            return Ok(allemployess);
        }

        [HttpGet("teamName")]
        public IActionResult team(string teamName)
        {
            var teamate = (from a in dataContext.EmployeeModel
                           where a.TeamName == teamName
                           select new
                           {
                               a.FirstName,
                               a.TeamName,

                           });
            return Ok(teamate);

        }

        [HttpGet("AssigingId")]
        public IActionResult getTeamTaskDetails(int AssigingId)
        {
            var allemployess = (from a in dataContext.EmployeeModel
                                join b in dataContext.TaskDetails on a.EmployeeId equals b.EmployeeId
                                where b.AssigingId == AssigingId
                                select new
                                {
                                    a.FirstName,
                                    b.TaskName,
                                    b.TaskDescription,
                                    b.TaskStatus,
                                    b.EmployeeId,
                                    b.AssigingId

                                }).ToList();
            return Ok(allemployess);
        }

        [HttpGet("EmployeeId")]
        public IActionResult getParticulaDetails(int EmployeeId)
        {
            var allemployess = (from a in dataContext.EmployeeModel
                                join b in dataContext.TaskDetails on a.EmployeeId equals b.EmployeeId
                                where b.EmployeeId == EmployeeId
                                select new
                                {
                                    a.FirstName,
                                    b.TaskName,
                                    b.TaskDescription,
                                    b.TaskStatus,
                                    b.EmployeeId,
                                    b.AssigingId

                                }).ToList();
            return Ok(allemployess);
        }
    }
}
