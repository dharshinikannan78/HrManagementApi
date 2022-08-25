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
            addTask.TaskStatus = "Progress";
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
        [HttpGet("teamates")]
        public IActionResult team(string teamates)
        {
            var teamate = (from a in dataContext.EmployeeModel
                           where a.TeamName == teamates
                           select new
                           {
                               a.FirstName,
                               a.TeamName
                           });
            return Ok(teamate);

        }


        [HttpGet("employeeId")]
        public IActionResult getTeamTaskDetails(int id)
        {
            var allemployess = (from a in dataContext.EmployeeModel
                                join b in dataContext.TaskDetails on a.EmployeeId equals b.EmployeeId
                                where b.AssigingId == id
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
        [HttpGet("getemployeeId")]
        public IActionResult getParticulaDetails(int id)
        {
            var allemployess = (from a in dataContext.EmployeeModel
                                join b in dataContext.TaskDetails on a.EmployeeId equals b.EmployeeId
                                where b.EmployeeId == id
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


        [HttpGet("Team")]
        public IActionResult projectTitle(string team)
        {
            var allemployess = (from a in dataContext.EmployeeModel
                                where a.Position == team
                                select new
                                {
                                    a.FirstName,

                                }).ToList();
            return Ok(allemployess);

        }
    }
}
