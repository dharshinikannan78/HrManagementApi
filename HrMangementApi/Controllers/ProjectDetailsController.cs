using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        [HttpGet("ProjectId")]
        public IActionResult GetByProjectId(int projectId)
        {

            var details = dataContext.ProjectDetail.AsNoTracking().FirstOrDefault(q => q.ProjectId == projectId);
            return Ok(details);

        }

        [HttpPut("UpdateTaskDetails")]
        public IActionResult UpdateProjectDetails([FromBody] ProjectDetails projectDetail)
        {
            var details = dataContext.ProjectDetail.AsNoTracking().FirstOrDefault(q => q.ProjectId == projectDetail.ProjectId);
            if (details == null)
            {
                return NotFound();
            }
            else
            {

                var diff = projectDetail.EndDate - projectDetail.StartDate;
                var data = (int)diff.Days;
                projectDetail.TodayDays = data.ToString();
                dataContext.Entry(projectDetail).State = EntityState.Modified;
                dataContext.SaveChanges();
                return Ok(projectDetail);
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
                          where t1.ProjectName == projectTitle
                          group gc by new
                          {
                              ProjectId = t1.ProjectId == null ? 0 : t1.ProjectId,
                              ProjectTitle = t1.ProjectTitle == null ? "no value" : t1.ProjectTitle,
                              ProjectName = t1.ProjectName == null ? "no value" : t1.ProjectName,
                              AssignedId = t1.AssiginedId == null ? 0 : t1.AssiginedId,
                              CreatedBy = t1.CreatedBy == null ? "no value" : t1.CreatedBy,
                              StartDate = t1.StartDate,
                              ProjectStatus = t1.ProjectStatus == null ? "no value" : t1.ProjectStatus,
                              ProjectDescription = t1.ProjectDescription == null ? "no value" : t1.ProjectDescription,
                          } into g
                          select new
                          {
                              projectId = g.Key.ProjectId,
                              ProjectTitle = g.Key.ProjectTitle,
                              ProjectName = g.Key.ProjectName,
                              AssignedId = g.Key.AssignedId,
                              CreatedBy = g.Key.CreatedBy,
                              StartDate = g.Key.StartDate,
                              ProjectStatus = g.Key.ProjectStatus,
                              ProjectDescription = g.Key.ProjectDescription,
                          };
            return Ok(details);
        }

        [HttpGet("TeamMembers")]
        public IActionResult projectTitle(string team)
        {
            var allemployess = (from a in dataContext.EmployeeModel
                                join b in dataContext.TaskDetails on a.EmployeeId equals b.EmployeeId

                                where b.TaskName == team
                                select new
                                {
                                    a.FirstName,
                                    a.LastName,
                                    a.Position,
                                    a.TeamName,
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
                               b.TaskId,
                               b.ProjectId,
                               b.EmployeeId,
                               b.AssigingId,
                               a.FirstName,
                               a.LastName,
                               b.TaskName,
                               a.Position,
                               b.TaskDescription,
                               b.TaskStatus,
                               a.TeamName,

                           });
            return Ok(teamate);
        }

        [HttpGet("TaskProjectStatus")]
        public IActionResult getProjectStatus(string TaskStatus)
        {
            List<TaskDetails> TaskDetails = new List<TaskDetails>();
            var taskStatus = from t in dataContext.TaskDetails
                             join p in dataContext.ProjectDetail on t.TaskName equals p.ProjectName
                             join e in dataContext.EmployeeModel on t.EmployeeId equals e.EmployeeId
                             where t.TaskName == p.ProjectName
                             select new
                             {
                                 p.ProjectName,
                                 t.TaskName,
                             };
            if (TaskStatus == "pending")
            {
                TaskDetails = dataContext.TaskDetails.Where(x => x.TaskStatus == "pending").Select(x => x).ToList();
                return Ok(taskStatus);
            }

            return Ok(TaskDetails);

        }

        /* [HttpGet("TaskProjectStatus")]
         public IActionResult getProjectStatus(string Status)
         {
             var taskStatus = from t in dataContext.TaskDetails
                              join p in dataContext.ProjectDetail on t.TaskName equals p.ProjectName
                              join e in dataContext.EmployeeModel on t.EmployeeId equals e.EmployeeId
                              where t.TaskName == p.ProjectName
                              select new
                              {
                                  p.ProjectName,
                                  t.TaskName,


                              };
             if (Status == "pending")
             {

                 return Ok(taskStatus);
             }
             else if (Status == "Completed")
             {
                 return Ok(taskStatus);
             }
             return BadRequest();
         }*/



    }
}
