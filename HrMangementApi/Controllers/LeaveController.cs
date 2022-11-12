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
            if (leaveData.StartDate <= leaveData.EndDate && leaveData.EndDate >= leaveData.StartDate)
            {

                var user = dataContext.LoginModels.Where(x => x.EmployeeId == leaveData.EmployeeId).FirstOrDefault();

                if (user.Role == "Employee")
                {
                    leaveData.TeamLeadApprovalStatus = "Pending";
                    leaveData.ManagerApprovalStatus = "Pending";
                    leaveData.AdminApprovalStatus = "Pending";
                }
                if (user.Role == "TeamLead")
                {
                    leaveData.TeamLeadApprovalStatus = "Approved";
                    leaveData.ManagerApprovalStatus = "Pending";
                    leaveData.AdminApprovalStatus = "Pending";
                }
                if (user.Role == "Manager")

                {
                    leaveData.TeamLeadApprovalStatus = "Approved";
                    leaveData.ManagerApprovalStatus = "Approved";
                    leaveData.AdminApprovalStatus = "Pending";
                }

                if (leaveData.LeaveDay == "Day")
                {
                    var data = dataContext.AttendanceModel.Any(x => x.EmployeeId == leaveData.EmployeeId && x.Date.Date == leaveData.StartDate.Date);
                    if (!data)
                    {
                        var leave = dataContext.LeaveModel.Any(x => x.EmployeeId == leaveData.EmployeeId && x.StartDate.Date <= leaveData.StartDate.Date && x.EndDate.Date >= leaveData.StartDate.Date && x.AdminApprovalStatus != "Rejected");

                        if (!leave)
                        {
                            var diff = leaveData.EndDate - leaveData.StartDate;
                            var noofDays = diff.Days + 1;
                            leaveData.NoOfDays = noofDays.ToString() + " Day";
                            leaveData.AppliedOn = DateTime.UtcNow.Date;
                            dataContext.LeaveModel.Add(leaveData);
                            dataContext.SaveChanges();
                            return Ok(leaveData);
                        }
                    }
                    return BadRequest();
                }

                else if (leaveData.LeaveDay == "HalfDay")
                {
                    if (leaveData.StartDate.Date == leaveData.EndDate.Date)
                    {
                        var halfday = dataContext.LeaveModel.Any(x => x.EmployeeId == leaveData.EmployeeId && x.StartDate.Date <= leaveData.StartDate.Date && x.EndDate.Date >= leaveData.StartDate.Date && x.AdminApprovalStatus != "Rejected");
                        if (!halfday)
                        {
                            leaveData.NoOfDays = "0.5 Day";
                            leaveData.AppliedOn = DateTime.UtcNow.Date;
                            dataContext.LeaveModel.Add(leaveData);
                            dataContext.SaveChanges();
                            return Ok(leaveData);
                        }

                        return BadRequest();
                    }
                    return Forbid();
                }
                else if (leaveData.LeaveDay == "Permission" && leaveData.StartDate.Date == leaveData.EndDate.Date)
                {
                    var dataLeave = dataContext.LeaveModel.Any(x => x.EmployeeId == leaveData.EmployeeId && x.StartDate.Date <= leaveData.StartDate.Date && x.EndDate.Date >= leaveData.StartDate.Date && x.AdminApprovalStatus != "Rejected");
                    if (!dataLeave)
                    {
                        var permission = leaveData.EndDate.TimeOfDay.TotalMinutes - leaveData.StartDate.TimeOfDay.TotalMinutes;
                        var hour = (int)permission / 60;
                        var minute = (int)permission % 60;
                        leaveData.NoOfDays = hour.ToString() + " Hrs " + minute.ToString() + " Min";
                        leaveData.AppliedOn = DateTime.UtcNow.Date;
                        dataContext.LeaveModel.Add(leaveData);
                        dataContext.SaveChanges();
                        return Ok(leaveData);
                    }
                }
                return BadRequest();
            }

            return Forbid();
        }


        [HttpPut("UpdateLeaveDetails")]
        public IActionResult UpdateLeaveDetails(int id, string response, [FromBody] LeaveDetails LeaveData)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == id).FirstOrDefault();
            var res = dataContext.LeaveModel.AsNoTracking().FirstOrDefault(a => a.LeaveId == LeaveData.LeaveId);

            if (res != null && user.Role == "TeamLead" && res.TeamLeadApprovalStatus == "Pending")
            {
                if (response == "Approved")
                {

                    res.TeamLeadApprovalStatus = response;
                    res.ManagerApprovalStatus = res.ManagerApprovalStatus;
                    res.AdminApprovalStatus = res.AdminApprovalStatus;
                    res.TeamLeadRejectReason = "Not Rejected";
                    res.ManagerRejectReason = res.ManagerRejectReason;
                    res.AdminRejectReason = res.AdminRejectReason;
                    dataContext.Entry(res).State = EntityState.Modified;
                    dataContext.SaveChanges();
                    return Ok();
                }
                if (response == "Rejected")
                {
                    res.TeamLeadApprovalStatus = response;
                    res.ManagerApprovalStatus = "Rejected";
                    res.AdminApprovalStatus = "Rejected";
                    res.TeamLeadRejectReason = LeaveData.TeamLeadRejectReason;
                    res.ManagerRejectReason = "Reject By TeamLeader,Get Approval From TeamLeader";
                    res.AdminRejectReason = "Reject By TeamLeader,Get Approval From TeamLeader";
                    dataContext.Entry(res).State = EntityState.Modified;
                    dataContext.SaveChanges();
                    return Ok();
                }
                return BadRequest();
            }
            if (res != null && user.Role == "Manager" && res.ManagerApprovalStatus == "Pending")
            {
                if (response == "Approved")
                {
                    res.TeamLeadRejectReason = res.TeamLeadRejectReason;
                    res.ManagerRejectReason = "Not Rejected";
                    res.AdminRejectReason = res.AdminRejectReason;
                    res.ManagerApprovalStatus = response;
                    res.TeamLeadApprovalStatus = res.TeamLeadApprovalStatus;
                    res.AdminApprovalStatus = res.AdminApprovalStatus;
                    dataContext.Entry(res).State = EntityState.Modified;
                    dataContext.SaveChanges();
                    return Ok();
                }
                if (response == "Rejected")
                {
                    res.TeamLeadApprovalStatus = res.TeamLeadApprovalStatus;
                    res.ManagerApprovalStatus = response;
                    res.AdminApprovalStatus = "Rejected";
                    res.ManagerRejectReason = LeaveData.ManagerRejectReason;
                    res.TeamLeadRejectReason = "Reject By Manager";
                    res.AdminRejectReason = "Reject By Manager,Get Approval From Manager";
                    dataContext.Entry(res).State = EntityState.Modified;
                    dataContext.SaveChanges();
                    return Ok();
                }
                return BadRequest();
            }
            if (res != null && user.Role == "Admin" && res.AdminApprovalStatus == "Pending")
            {
                if (response == "Approved")
                {
                    res.TeamLeadRejectReason = res.TeamLeadRejectReason;
                    res.ManagerRejectReason = res.ManagerRejectReason;
                    res.AdminRejectReason = "Not Rejected";
                    res.AdminApprovalStatus = response;
                    res.TeamLeadApprovalStatus = res.TeamLeadApprovalStatus;
                    res.ManagerApprovalStatus = res.ManagerApprovalStatus;
                    dataContext.Entry(res).State = EntityState.Modified;
                    dataContext.SaveChanges();
                    return Ok();
                }
                if (response == "Rejected")
                {
                    res.AdminApprovalStatus = response;
                    res.AdminRejectReason = LeaveData.AdminRejectReason;
                    res.ManagerRejectReason = "Rejected By Admin";
                    res.TeamLeadRejectReason = "Reject By Admin";
                    dataContext.Entry(res).State = EntityState.Modified;
                    dataContext.SaveChanges();
                    return Ok();
                }
                return BadRequest();
            }

            return BadRequest();
        }

        [HttpGet("GetLeave")]
        public IActionResult GetLeave(int data)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == data).FirstOrDefault();

            if (user != null && user.Role == "Admin")
            {
                var Adminleave = (from a in dataContext.EmployeeModel
                                  join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                                  where l.TeamLeadApprovalStatus == "Approved" && l.ManagerApprovalStatus == "Approved" && l.AdminApprovalStatus == "Pending"
                                  select new
                                  {
                                      a.EmployeeId,
                                      a.FirstName,
                                      a.LastName,
                                      a.Designation,
                                      l.StartDate,
                                      l.EndDate,
                                      l.NoOfDays,
                                      l.AppliedOn,
                                      l.LeaveReason,
                                      l.LeaveType,
                                      l.LeaveId,
                                      l.LeaveDay,
                                      l.TeamLeadApprovalStatus,
                                      l.ManagerApprovalStatus,
                                      l.AdminApprovalStatus,
                                      l.TeamLeadRejectReason,
                                      l.ManagerRejectReason,
                                      l.AdminRejectReason
                                  }).ToList();
                return Ok(Adminleave);
            }
            if (user != null && user.Role == "Manager")
            {
                var ManagerLeave = (from a in dataContext.EmployeeModel
                                    join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                                    join x in dataContext.LoginModels on a.EmployeeId equals x.EmployeeId
                                    where x.ReportingId == data && l.TeamLeadApprovalStatus == "Approved" && l.ManagerApprovalStatus == "Pending"
                                    select new
                                    {
                                        a.EmployeeId,
                                        a.FirstName,
                                        a.LastName,
                                        a.Designation,
                                        l.StartDate,
                                        l.EndDate,
                                        l.NoOfDays,
                                        l.AppliedOn,
                                        l.LeaveReason,
                                        l.LeaveType,
                                        l.LeaveId,
                                        l.LeaveDay,
                                        l.TeamLeadApprovalStatus,
                                        l.ManagerApprovalStatus,
                                        l.AdminApprovalStatus,
                                        l.TeamLeadRejectReason,
                                        l.ManagerRejectReason,
                                        l.AdminRejectReason
                                    }).ToList();

                var ManagerOnlyLeave = (from a in dataContext.EmployeeModel
                                        join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                                        join x in dataContext.LoginModels on a.EmployeeId equals x.EmployeeId
                                        where x.Role == "Employee" && l.TeamLeadApprovalStatus == "Approved" && l.ManagerApprovalStatus == "Pending"
                                        select new
                                        {
                                            a.EmployeeId,
                                            a.FirstName,
                                            a.LastName,
                                            a.Designation,
                                            l.StartDate,
                                            l.EndDate,
                                            l.NoOfDays,
                                            l.AppliedOn,
                                            l.LeaveReason,
                                            l.LeaveType,
                                            l.LeaveId,
                                            l.LeaveDay,
                                            l.TeamLeadApprovalStatus,
                                            l.ManagerApprovalStatus,
                                            l.AdminApprovalStatus,
                                            l.TeamLeadRejectReason,
                                            l.ManagerRejectReason,
                                            l.AdminRejectReason
                                        }).ToList();
                ManagerLeave.AddRange(ManagerOnlyLeave);
                return Ok(ManagerLeave);
            }
            if (user != null && user.Role == "TeamLead")
            {
                var TeamLeadLeave = (from a in dataContext.EmployeeModel
                                     join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                                     join x in dataContext.LoginModels on a.EmployeeId equals x.EmployeeId
                                     where x.ReportingId == data && l.TeamLeadApprovalStatus == "Pending"
                                     select new
                                     {
                                         a.EmployeeId,
                                         a.FirstName,
                                         a.LastName,
                                         a.Designation,
                                         l.StartDate,
                                         l.EndDate,
                                         l.NoOfDays,
                                         l.AppliedOn,
                                         l.LeaveReason,
                                         l.LeaveType,
                                         l.LeaveId,
                                         l.LeaveDay,
                                         l.TeamLeadApprovalStatus,
                                         l.ManagerApprovalStatus,
                                         l.AdminApprovalStatus,
                                         l.TeamLeadRejectReason,
                                         l.ManagerRejectReason,
                                         l.AdminRejectReason
                                     }).ToList();
                return Ok(TeamLeadLeave);
            }

            return BadRequest();
        }
        [HttpGet("GetLeaveById")]
        public IActionResult GetLeaveById(int id)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == id).FirstOrDefault();
            if (user != null && user.Role != "Admin")
            {

                var Leave = (from a in dataContext.EmployeeModel
                                          join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                                          where l.EmployeeId == id
                                          orderby l.StartDate descending
                                          select new
                                          {
                                              a.FirstName,
                                              a.LastName,
                                              l.EmployeeId,
                                              l.StartDate,
                                              l.EndDate,
                                              l.LeaveId,
                                              l.LeaveReason,
                                              l.LeaveType,
                                              l.NoOfDays,
                                              l.AdminApprovalStatus,
                                              l.AdminRejectReason,
                                              l.AppliedOn,
                                              l.LeaveDay,
                                              l.TeamLeadApprovalStatus,
                                              l.TeamLeadRejectReason,

                                          }).ToList();
                return Ok(Leave);
            }
            return BadRequest();
        }

         [HttpGet("GetLeaveStatus")]
         public IActionResult GetLeaveStatus(int id)
         {
             var LeaveData = dataContext.LeaveModel.Where(x => x.LeaveId == id).AsNoTracking().FirstOrDefault();
             if (LeaveData != null) return Ok(LeaveData);
             return BadRequest();
         }
        [HttpGet("GetAllLeave")]
        public IActionResult GetAllLeave(int id)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == id).FirstOrDefault();
            if (user != null&&user.Role=="Admin")
            {
                var AdminAllLeave = (from a in dataContext.EmployeeModel
                                       join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                                       orderby l.StartDate descending
                                       select new
                                       {
                                           a.FirstName,
                                           a.LastName,
                                           l.EmployeeId,
                                           l.StartDate,
                                           l.EndDate,
                                           l.LeaveId,
                                           l.LeaveReason,
                                           l.LeaveType,
                                           l.NoOfDays,
                                           l.AdminApprovalStatus,
                                           l.AdminRejectReason,
                                           l.AppliedOn,
                                           l.LeaveDay,
                                           l.TeamLeadApprovalStatus,
                                           l.TeamLeadRejectReason,
                                       }).ToList();
                return Ok(AdminAllLeave);
            }
            if (user != null && user.Role == "Manager")
            {
                var ManagerAllLeave = (from a in dataContext.LoginModels
                                     join x in dataContext.EmployeeModel on a.EmployeeId equals x.EmployeeId
                                     join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                                     orderby l.StartDate descending
                                     where a.ReportingId == id||a.Role=="Employee"
                                     select new
                                     {
                                         x.FirstName,
                                         x.LastName,
                                         l.EmployeeId,
                                         l.StartDate,
                                         l.EndDate,
                                         l.LeaveId,
                                         l.LeaveReason,
                                         l.LeaveType,
                                         l.NoOfDays,
                                         l.AdminApprovalStatus,
                                         l.AdminRejectReason,
                                         l.AppliedOn,
                                         l.LeaveDay,
                                         l.TeamLeadApprovalStatus,
                                         l.TeamLeadRejectReason,
                                     }).ToList();
                return Ok(ManagerAllLeave);
            }
            if (user != null && user.Role == "TeamLead")
            {
                var TeamLeadAllLeave = (from a in dataContext.LoginModels
                                       join x in dataContext.EmployeeModel on a.EmployeeId equals x.EmployeeId
                                       join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                                       orderby l.StartDate descending
                                       where a.ReportingId == id 
                                       select new
                                       {
                                           x.FirstName,
                                           x.LastName,
                                           l.EmployeeId,
                                           l.StartDate,
                                           l.EndDate,
                                           l.LeaveId,
                                           l.LeaveReason,
                                           l.LeaveType,
                                           l.NoOfDays,
                                           l.AdminApprovalStatus,
                                           l.AdminRejectReason,
                                           l.AppliedOn,
                                           l.LeaveDay,
                                           l.TeamLeadApprovalStatus,
                                           l.TeamLeadRejectReason,
                                       }).ToList();
                return Ok(TeamLeadAllLeave);
            }
            return NotFound();
        }
        public class testType
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string Status { get; set; }
        }

        [HttpGet("AttdLeaveData")]
        public IActionResult AttdLeaveData()
        {
            var att = dataContext.AttendanceModel.Select(x => new testType { Id = x.AttendanceId, Date = x.Date, Status = x.Status }).ToList();
            var Leave = dataContext.LeaveModel.Select(x => new testType { Id = x.LeaveId, Date = x.StartDate, Status = x.LeaveReason }).ToList();
            att.AddRange(Leave);
            var test = att.OrderBy(a => a.Date).ToList();
            return Ok(test);
        }

        [HttpGet("GetTotalLeave")]
        public IActionResult GetTotalLeave(int id, DateTime month)
        {
            var user = dataContext.LoginModels.Any(x => x.EmployeeId == id);
            if (user == null)
            {
                return NotFound();
            }
            if (user)
            {
                var res = dataContext.LeaveModel.Where(a => a.EmployeeId == id && (a.LeaveDay == "Day" || a.LeaveDay == "HalfDay") && a.AdminApprovalStatus == "Approved" &&
              ((a.StartDate.Month >= month.Month && a.EndDate.Month <= month.Month) && (a.StartDate.Year == month.Year && a.EndDate.Year == month.Year))).Select(x => x.NoOfDays).ToList();
                var resp = res.Select(x => x.Replace(" Day", ""));
                float total = resp.Sum(x => float.Parse(x));
                return Ok(total);
            }
            return NotFound();

        }
        [HttpGet("GetTotalPermission")]
        public IActionResult GetTotalPermission(int id, DateTime month)
        {
            var user = dataContext.LoginModels.Any(x => x.EmployeeId == id);
            if (user == null)
            {
                return NotFound();
            }
            if (user)
            {
                var res = dataContext.LeaveModel.Where(a => a.EmployeeId == id && a.LeaveDay == "Permission" && a.AdminApprovalStatus == "Approved" &&
        ((a.StartDate.Month >= month.Month && a.EndDate.Month <= month.Month) && (a.StartDate.Year == month.Year && a.EndDate.Year == month.Year))).Select(x => x.NoOfDays).ToList();
                var hour = res.Select(x => x.Remove(2));
                var min = res.Select(x => x.Remove(0, 6));
                var resp = min.Select(x => x.Replace(" Min", ""));
                int totalhour = hour.Sum(x => int.Parse(x));
                int totalmin = resp.Sum(x => int.Parse(x));
                var hours = totalhour + (totalmin / 60);
                var minutes = totalmin % 60;
                var TotalPermission = hours.ToString() + " Hrs " + minutes.ToString() + " Mins";

                return Ok(TotalPermission);
            }
            return NotFound();


        }

        [HttpGet("GetNotification")]
        public IActionResult Getnotification(int id)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == id).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }
            if (user.Role == "Admin")
            {
                var team = (from a in dataContext.LoginModels
                            join x in dataContext.LeaveModel on a.EmployeeId equals x.EmployeeId
                            where x.TeamLeadApprovalStatus == "Approved" && x.ManagerApprovalStatus == "Approved" && x.AdminApprovalStatus == "Pending"
                            select new
                            {

                            });
                var Notify = team.Count();
                return Ok(Notify);
            }
            if (user.Role == "Manager")
            {
                var team = (from a in dataContext.LoginModels
                            join x in dataContext.LeaveModel on a.EmployeeId equals x.EmployeeId
                            where (a.ReportingId == id || a.Role == "Employee") && x.TeamLeadApprovalStatus == "Approved" && x.ManagerApprovalStatus == "Pending"
                            select new
                            {

                            });

                var Notify = team.Count();
                return Ok(Notify);
            }
            if (user.Role == "TeamLead")
            {
                var team = (from a in dataContext.LoginModels
                            join x in dataContext.LeaveModel on a.EmployeeId equals x.EmployeeId
                            where a.ReportingId == id && x.TeamLeadApprovalStatus == "Pending"
                            select new
                            {

                            });
                var Notify = team.Count();
                return Ok(Notify);
            }

            return BadRequest();
        }

    }
}

