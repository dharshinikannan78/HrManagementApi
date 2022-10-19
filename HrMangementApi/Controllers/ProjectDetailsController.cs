using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            taskDetail.EmployeeIds = string.Join(',', taskDetail.EmployeeIds);

            /*var total = taskDetail.EndDate - taskDetail.StartDate.Date;
            taskDetail.TodayDays = (int)total.TotalDays;*/
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
                var diff = projectDetail.EndDate.Date - projectDetail.StartDate.Date;
                var data = (int)diff.Days;
                projectDetail.TotalDays = data.ToString();
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
                          where t1.ProjectName == projectTitle || t1.ProjectStatus == "InProgress" || t1.ProjectStatus == "Completed"
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


        [HttpGet("projectId")]
        public IActionResult GetProjectDetails(int projectId)
        {
            var details = from t in dataContext.TaskDetails
                          join t1 in dataContext.ProjectDetail on t.ProjectId equals t1.ProjectId
                          join t2 in dataContext.EmployeeModel on t.EmployeeId equals t2.EmployeeId
                          where t1.ProjectId == projectId
                          select new
                          {
                              t2.FirstName,
                              t1.ProjectId,
                              t1.ProjectName,
                              t.TaskName,
                              t.TaskDescription
                          };
            return Ok(details);
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

        [HttpGet("GetTaskDetails")]
        public IActionResult GetTaskDetails(int id, int? EmpId)           // project overview cmp source
        {

            if (EmpId != null)
            {
                var details = (from t in dataContext.ProjectDetail
                               where t.ProjectId == id
                               where t.IsArchived == false
                               select new
                               {
                                   t.StartDate,
                                   t.EndDate,
                                   t.ProjectName,
                                   t.ProjectDescription,
                                   t.TotalDays,
                                   t.Priority,
                                   t.ProjectStatus,
                                   t.ProjectTitle,
                                   t.ProjectId,
                                   t.AssiginedId,
                                   t.CreatedBy,

                               }).ToList();

                var TaskDetails = (from task in dataContext.TaskDetails
                                   join b in dataContext.EmployeeModel on task.EmployeeId equals b.EmployeeId
                                   where task.ProjectId == id && task.EmployeeId == EmpId
                                   where task.IsTaskArchieved == false
                                   select new
                                   {
                                       b.FirstName,
                                       b.LastName,
                                       task.TaskName,
                                       task.TaskId,
                                       task.EmployeeId,
                                       task.TaskDescription,
                                       task.TaskStatus,
                                       task.StartDate,
                                       task.EndDate,
                                       task.Priority,
                                       task.ProjectId,
                                       task.AssigingId,
                                       DueDate = (task.EndDate - DateTime.Now).Days + 1 >= 0 ? (task.EndDate - DateTime.Now).Days + 1 : 0,
                                   }).ToList().OrderByDescending(a => a.TaskStatus).GroupBy(x => x.TaskStatus).ToList();

                var final = new
                {
                    details,
                    TaskDetails,
                };
                return Ok(final);
            }
            else
            {
                var details = (from t in dataContext.ProjectDetail
                               where t.ProjectId == id
                               where t.IsArchived == false
                               select new
                               {
                                   t.StartDate,
                                   t.EndDate,
                                   t.ProjectName,
                                   t.ProjectDescription,
                                   t.TotalDays,
                                   t.Priority,
                                   t.ProjectStatus,
                                   t.ProjectTitle,
                                   t.ProjectId,
                                   t.AssiginedId,
                                   t.CreatedBy,

                               }).ToList();

                var TaskDetails = (from task in dataContext.TaskDetails
                                   join t in dataContext.ProjectDetail on task.ProjectId equals t.ProjectId
                                   join b in dataContext.EmployeeModel on task.EmployeeId equals b.EmployeeId
                                   where task.ProjectId == id
                                   where task.IsTaskArchieved == false
                                   select new
                                   {
                                       b.FirstName,
                                       b.LastName,
                                       t.ProjectName,
                                       task.TaskName,
                                       task.TaskId,
                                       task.EmployeeId,
                                       task.TaskDescription,
                                       task.TaskStatus,
                                       task.StartDate,
                                       task.EndDate,
                                       task.Priority,
                                       task.ProjectId,
                                       task.AssigingId,
                                       DueDate = (task.EndDate - DateTime.Now).Days + 1 >= 0 ? (task.EndDate - DateTime.Now).Days + 1 : 0,
                                   }).ToList().OrderByDescending(a => a.TaskStatus).GroupBy(x => x.TaskStatus).ToList();

                /*   var detailsStatus = (from task in dataContext.TaskDetails
                                        join t in dataContext.ProjectDetail on task.ProjectId equals t.ProjectId
                                        join b in dataContext.EmployeeModel on task.EmployeeId equals b.EmployeeId
                                        where t.ProjectStatus == "archived"
                                        select new
                                        { });*/



                var final = new
                {
                    details,
                    TaskDetails,
                };
                return Ok(final);
            }
        }

        [HttpGet("GetAllProjectDetails")]

        public IActionResult GetAllProjectDetails(int? EmpId)    //list component source
        {
            if (EmpId != null)
            {
                var projectsDetails = (from a in dataContext.ProjectDetail
                                       join b in dataContext.ProjectMemberModel on a.ProjectId equals b.ProjectId
                                       where b.EmpId == EmpId
                                       where a.IsArchived == false
                                       select new
                                       {
                                           a.ProjectId,
                                           a.ProjectName,
                                           a.ProjectTitle,
                                           a.StartDate,
                                           a.AssiginedId,
                                           a.CreatedBy,
                                           a.EndDate,
                                           a.ProjectDescription,
                                           a.ProjectStatus,
                                           a.Priority,

                                       }).ToList().GroupBy(x => x.ProjectName).ToList();

                return Ok(projectsDetails);
            }

            var projects = (from a in dataContext.ProjectDetail
                            where a.IsArchived == false
                            select new
                            {
                                a.ProjectId,
                                a.ProjectName,
                                a.ProjectTitle,
                                a.StartDate,
                                a.AssiginedId,
                                a.CreatedBy,
                                a.EndDate,
                                a.ProjectDescription,
                                a.ProjectStatus,
                                a.Priority,

                            }).ToList();
            var proj = projects.GroupBy(x => x.ProjectName).ToList();
            return Ok(proj);
        }



        [HttpGet("GetEmployeeDetails")]

        public IActionResult GetAttachmentDetails(int projectIds)
        {
            var userData = dataContext.ProjectDetail.Where(a => a.ProjectId == projectIds)
                .FirstOrDefault();
            var attachmentList = new List<EmployeeDetails>();
            if (userData != null)
            {
                var attamenctIds = userData.EmployeeIds.Split(',');

                if (attamenctIds.Any())
                {
                    foreach (var attamenctId in attamenctIds)
                    {
                        var attachment = dataContext.EmployeeModel.Where(n => n.EmployeeId.ToString() == attamenctId).FirstOrDefault();
                        attachmentList.Add(attachment);

                    }

                }
            }
            return Ok(attachmentList);
        }

        [HttpGet("AddEmployeeToProject")]
        public ActionResult AddEmployeeToProject(int ProjId, string EmpIds)
        {
            var EmpId = EmpIds.Split(',');
            if (EmpIds.Any())
            {
                foreach (var Emp in EmpId)
                {
                    var Ispresent = dataContext.ProjectMemberModel.Any(a => a.EmpId == int.Parse(Emp) && a.ProjectId == ProjId);
                    if (Ispresent) continue;
                    ProjectMemberModel ProjMem = new ProjectMemberModel();
                    ProjMem.ProjectId = ProjId;
                    ProjMem.EmpId = int.Parse(Emp);
                    dataContext.ProjectMemberModel.Add(ProjMem);
                }
                dataContext.SaveChanges();
            }
            return Ok(new
            {
                message = "potachu poda venna"
            });
        }

        [HttpGet("GetAddedEmpToProject")]
        public ActionResult GetAddedEmpToProject(int id)
        {
            var Data = (from a in dataContext.ProjectMemberModel
                        join p in dataContext.ProjectDetail on a.ProjectId equals p.ProjectId
                        join x in dataContext.EmployeeModel on a.EmpId equals x.EmployeeId
                        join f in dataContext.FileAttachment on x.AttachmentIds equals f.AttachmentId.ToString()
                        where a.ProjectId == id

                        select new
                        {
                            x.FirstName,
                            x.LastName,
                            x.EmployeeId,
                            f.PhotoPath
                        }).ToList();
            return Ok(Data);
        }

    }
}



