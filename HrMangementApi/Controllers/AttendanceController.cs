using HrMangementApi.Model;
using HrMangementApi.UserDbContext;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
namespace HrMangementApi.Controllers
{
    [EnableCors("AllowOrigin")]
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
        public IActionResult AddAttendance([FromBody] AttendanceDetails data)
        {
            if (dataContext.LeaveModel.Any(x => x.StartDate.Date == data.Date.Date && x.LeaveDay == "HalfDay" || x.LeaveDay == "Permission"))
            {
                if (!dataContext.AttendanceModel.Any(x => x.EmployeeId == data.EmployeeId && x.Date.Date == data.Date.Date))
                {
                    
                    data.InTime = DateTime.Now;
                    dataContext.AttendanceModel.Add(data);
                    dataContext.SaveChanges();
                    return Ok(data);
                }
                return NotFound();
            }
            else if (!dataContext.AttendanceModel.Any(x => x.EmployeeId == data.EmployeeId && x.Date.Date == data.Date.Date))
            {
                var attendance = dataContext.LeaveModel.Any(x => x.EmployeeId == data.EmployeeId && x.StartDate.Date <= data.Date.Date && x.EndDate.Date >= data.Date.Date);
                if (!attendance)
                {
                    data.Status = "Present";
                    data.InTime = DateTime.Now;
                    dataContext.AttendanceModel.Add(data);
                    dataContext.SaveChanges();
                    return Ok(data);
                }
                return BadRequest();
            }
            return NotFound();

        }

        [HttpPut("updateAttendance")]
        public IActionResult updateAttendance([FromBody] AttendanceDetails data)
        {
            var res = dataContext.AttendanceModel.FirstOrDefault(a => a.AttendanceId == data.AttendanceId);
            if (res == null)
            {
                return NotFound();
            }
            else
            {
                /*data.Status = "Present";
                data.OutTime = DateTime.Now;
                var date = DateTime.Now;
                data.Date = date.Date;
                var diff = data.OutTime - data.InTime;
                var noofDays = (int)diff.Seconds;
                data.WorkDuration = noofDays;
                dataContext.Entry(data).State = EntityState.Modified;
                dataContext.SaveChanges();
                return Ok(data);*//*
                res.OutTime = data.OutTime;
                var date = DateTime.Now;
                res.Date = date.Date;
                var diff = data.OutTime - res.InTime;
                var noofDays = (int)diff.Seconds;
                res.WorkDuration = noofDays;
                dataContext.SaveChanges();
                return Ok(res);
                *//*res.Status = "Present";
                res.OutTime = DateTime.Now;
                var date = DateTime.Now;
                res.Date = date.Date;
                var diff = data.OutTime - res.InTime;
                var noofDays = (int)diff.Seconds;
                res.WorkDuration = noofDays;
                dataContext.Entry(data).State = EntityState.Modified;
                dataContext.SaveChanges();
                return Ok(data);*/
                /*res.Status = "Present";*/
                res.OutTime = DateTime.Now;
                var date = DateTime.Now;
                res.Date = date.Date;
                var diff = data.OutTime.TimeOfDay.TotalMinutes - res.InTime.TimeOfDay.TotalMinutes;
                var hour = (int)diff / 60;
                var minute = (int)diff % 60;
                res.WorkDuration = hour.ToString() + " Hrs " + minute.ToString() + " Min";
                dataContext.SaveChanges();
                return Ok(data);
            }

            /*try
            {
                var update = dataContext.AttendanceModel.FirstOrDefault(a => a.EmployeeId == data.EmployeeId);
                if (update != null)
                {
                    update.OutTime = data.OutTime;
                    dataContext.AttendanceModel.Update(update);
                    dataContext.SaveChanges();
                }
                return Ok(update);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }*/
        }


        [HttpPost("AttendanceByEmployee")]
        public IActionResult AttendanceByEmployee(int id, [FromBody] FilterModel filter)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == id).FirstOrDefault();
            if (user != null && user.Role == "Admin")
            {
                var data = (from a in dataContext.AttendanceModel
                            join e in dataContext.EmployeeModel on a.EmployeeId equals e.EmployeeId
                            where a.Date.Date >= filter.FromDate.Date && a.Date.Date <= filter.ToDate.Date
                            select new
                            {
                                a.Date,
                                a.EmployeeId,
                                e.FirstName,
                                a.Status,

                            }).ToList();
                return Ok(data);
            }
            if (user != null && user.Role == "TeamLeader" || user.Role == "TeamMember")
            {
                var datas = (from a in dataContext.AttendanceModel
                             join e in dataContext.EmployeeModel on a.EmployeeId equals e.EmployeeId
                             where a.Date.Date >= filter.FromDate.Date && a.Date.Date <= filter.ToDate.Date && a.EmployeeId == id
                             select new
                             {
                                 a.Date,
                                 a.EmployeeId,
                                 e.FirstName,
                                 a.Status,
                             });
                return Ok(datas);

            }
            return BadRequest();
        }

        [HttpPost("LeaveByEmployee")]
        public IActionResult LeaveByEmployee(int id, [FromBody] FilterModel filter)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == id).FirstOrDefault();
            if (user != null && user.Role == "Admin")
            {
                var data = (from a in dataContext.LeaveModel
                            join e in dataContext.EmployeeModel on a.EmployeeId equals e.EmployeeId
                            where a.StartDate.Date >= filter.FromDate.Date && a.StartDate.Date <= filter.ToDate.Date
                            select new
                            {
                                a.StartDate,
                                a.EmployeeId,
                                e.FirstName,
                                a.ApprovalStatus,
                            }).ToList();
                return Ok(data);
            }
            if (user != null && user.Role == "TeamLeader" || user.Role == "TeamMember")
            {
                var datas = (from a in dataContext.LeaveModel
                             join e in dataContext.EmployeeModel on a.EmployeeId equals e.EmployeeId
                             where a.StartDate.Date >= filter.FromDate.Date && a.StartDate.Date <= filter.ToDate.Date && a.EmployeeId == id
                             select new
                             {
                                 a.StartDate,
                                 a.EmployeeId,
                                 e.FirstName,
                                 a.ApprovalStatus,
                             });
                return Ok(datas);
            }
            return BadRequest();
        }


        [HttpGet("GetAttendanceDetails")]
        public IActionResult GetDetails()
        {

            List<AttendanceDetails> comment = new List<AttendanceDetails>();
            List<LeaveDetails> Leave = new List<LeaveDetails>();


            var output = comment.Select(q => new
            {
                id = q.EmployeeId,
                date = q.Date,
                type = "comment"


            }).Concat(Leave.Select(q => new
            {
                id = q.EmployeeId,
                date = q.Date,
                type = "Leave"

            })).OrderByDescending(q => q.date).Take(10);


            return Ok(output);
        }

        [HttpGet("AttendanceDetails")]
        public IActionResult AttendanceDetails(int data)
        {
            var EmployeeAttendance = (from a in dataContext.EmployeeModel
                                      join l in dataContext.AttendanceModel on a.EmployeeId equals l.EmployeeId
                                      where l.EmployeeId == data
                                      orderby l.Date descending
                                      select new
                                      {
                                          a.EmployeeId,
                                          a.FirstName,
                                          a.LastName,
                                          a.Designation,
                                          l.InTime,
                                          l.OutTime,
                                          l.AttendanceId,
                                          l.Date,
                                          l.Status,
                                          l.WorkDuration
                                      }).Take(5);

            var AdminAttendance = (from a in dataContext.EmployeeModel
                                   join l in dataContext.AttendanceModel on a.EmployeeId equals l.EmployeeId
                                   orderby l.Date descending
                                   select new
                                   {
                                       a.EmployeeId,
                                       a.FirstName,
                                       a.LastName,
                                       a.Designation,
                                       l.InTime,
                                       l.OutTime,
                                       l.AttendanceId,
                                       l.Date,
                                       l.Status,
                                       l.WorkDuration

                                   }).ToList();
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == data).FirstOrDefault();
            if (user != null && user.Role == "Admin")
            {
                return Ok(AdminAttendance);
            }
            if (user != null && user.Role == "Employee")
            {
                return Ok(EmployeeAttendance);
            }

            return BadRequest();

        }


    }
}











/*
        [HttpGet("GetAttendance")]
        public IActionResult GetLeave(int data)
        {
            var Details = (from a in dataContext.EmployeeModel
                           join l in dataContext.AttendanceModel on a.EmployeeId equals l.EmployeeId
                           where l.EmployeeId == data
                           select new
                           {
                               a.EmployeeId,
                               a.FirstName,
                               a.LastName,
                               a.Designation,
                               l.InTime,
                               l.OutTime,
                               l.AttendanceId

                           }).ToList();
            var Detail = (from a in dataContext.EmployeeModel
                          join l in dataContext.AttendanceModel on a.EmployeeId equals l.EmployeeId
                          select new
                          {
                              a.EmployeeId,
                              a.FirstName,
                              a.LastName,
                              a.Designation,
                              l.InTime,
                              l.OutTime,
                              l.AttendanceId



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
        [HttpGet("GetTimeOff")]
        public IActionResult GetTimeOff(int id)
        {
            var attendance = from l in dataContext.LoginModels
                             join e in dataContext.EmployeeModel on l.EmployeeId equals e.EmployeeId
                             join a in dataContext.AttendanceModel on l.EmployeeId equals a.EmployeeId


                             select new
                             {
                                 l.EmployeeId,
                                 e.FirstName,
                                 e.LastName,
                                 e.Designation,
                                 a.InTime,
                                 a.OutTime,
                                 a.WorkDuration,

                             };
            var leave = from l in dataContext.LoginModels
                        join e in dataContext.EmployeeModel on l.EmployeeId equals e.EmployeeId

                        join le in dataContext.LeaveModel on l.EmployeeId equals le.EmployeeId
                        where l.EmployeeId == id && le.ApprovalStatus == "Approved"

                        select new
                        {
                            l.EmployeeId,
                            e.FirstName,
                            e.LastName,
                            e.Designation,
                            le.ApprovalStatus,
                            le.LeaveType,
                            le.StartDate,
                            le.EndDate,
                            le.LeaveId


                        };


            return Ok(attendance);


        }*/
