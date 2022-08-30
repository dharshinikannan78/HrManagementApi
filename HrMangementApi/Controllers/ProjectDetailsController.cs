using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;


namespace HrMangementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectDetailsController : ControllerBase
    {
        private readonly UserdbContext dataContext;
        public ProjectDetailsController(UserdbContext _dataContext)
        {
            dataContext = _dataContext;
        }

        [HttpPost("AddEmployeeDetails")]
        public IActionResult AddProjectDetails([FromBody] ProjectDetails taskDetail)
        {
            taskDetail.ProjectStatus = "InProgress";
            taskDetail.StartDate = DateTime.Now;
            dataContext.ProjectDetail.Add(taskDetail);
            dataContext.SaveChanges();
            return Ok(taskDetail);
        }

        [HttpPut("UpdateTaskDetails")]
        public IActionResult UpdateProjectDetails([FromBody] ProjectDetails taskDetail)
        {
            var details = dataContext.ProjectDetail.AsNoTracking().FirstOrDefault(q => q.ProjectId == taskDetail.ProjectId);
            if (details == null)
            {
                return NotFound();
            }
            else
            {
                dataContext.Entry(taskDetail).State = EntityState.Modified;
                dataContext.SaveChanges();
                return Ok(taskDetail);
            }
        }
        [HttpDelete("DeleteProjectDetails")]
        public IActionResult DeletProjectDetails(int id)
        {
            var delete = dataContext.ProjectDetail.Find(id);
            if (delete == null)
            {
                return NotFound();
            }
            else
            {
                dataContext.ProjectDetail.Remove(delete);
                dataContext.SaveChanges();
                return Ok();
            }
        }


        [HttpGet("getDetails")]
        public IActionResult ProjectDetails(string projectTitle)
        {
            var details = from t1 in dataContext.ProjectDetail
                          join t2 in dataContext.EmployeeModel on t1.ProjectTitle equals t2.TeamName into groupcls
                          from gc in groupcls.DefaultIfEmpty()
                          where t1.ProjectTitle == projectTitle
                          group gc by new
                          {
                              ProjectId = t1.ProjectId == null ? 0 : t1.ProjectId,
                              ProjectTitle = t1.ProjectTitle == null ? "no value" : t1.ProjectTitle,
                              AssignedId = t1.AssiginedId == null ? 0 : t1.AssiginedId,
                              CreatedBy = t1.CreateBy == null ? "no value" : t1.CreateBy,
                              ProjectDescription = t1.ProjectDescription == null ? "no value" : t1.ProjectDescription,
                          } into g
                          select new
                          {
                              projectId = g.Key.ProjectId,
                              ProjectTitle = g.Key.ProjectTitle,
                              AssignedId = g.Key.AssignedId,
                              CreatedBy = g.Key.CreatedBy,
                              ProjectDescription = g.Key.ProjectDescription,

                          };
            return Ok(details);
        }

        [HttpGet("TeamMembers")]
        public IActionResult projectTitle(string team)
        {
            var allemployess = (from a in dataContext.EmployeeModel
                                where a.TeamName == team
                                select new
                                {
                                    a.FirstName,
                                    a.TeamName

                                }).ToList();
            return Ok(allemployess);
        }
        [HttpGet("TaskName")]
        public IActionResult team(string taskName)
        {
            var teamate = (from a in dataContext.EmployeeModel
                           join b in dataContext.TaskDetails on a.EmployeeId equals b.EmployeeId
                           where b.TaskName == taskName
                           select new
                           {
                               a.FirstName,
                               b.TaskName,
                               a.Position,
                               b.TaskDescription,
                               b.TaskStatus,
                               a.TeamName,

                           });
            return Ok(teamate);

        }
    }
}
