using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            dataContext.TaskDetails.Add(addTask);
            dataContext.SaveChanges();
            return Ok(addTask);
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
                                    b.ProjectTitle,
                                    b.EmployeeId
                                }).ToList();
            return Ok(allemployess);
        }

        [HttpGet("employeeId")]
        public IActionResult getParticulaDetails(int id)
        {
            var allemployess = (from a in dataContext.EmployeeModel
                                join b in dataContext.TaskDetails on a.EmployeeId equals b.EmployeeId
                                where b.EmployeeId == id
                                select new
                                {
                                    a.FirstName,
                                    b.ProjectTitle,
                                    b.EmployeeId
                                }).ToList();
            return Ok(allemployess);
        }

        [HttpGet("projectTitle")]
        public IActionResult projectTitle(string projectTitle)
        {
            var allemployess = (from a in dataContext.EmployeeModel
                                join b in dataContext.TaskDetails on a.EmployeeId equals b.EmployeeId
                                where b.ProjectTitle == projectTitle
                                select new
                                {
                                    a.FirstName,
                                    b.ProjectTitle,
                                    b.EmployeeId
                                }).ToList();
            return Ok(allemployess);

        }
    }
}
