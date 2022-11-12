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

        [HttpGet("AddAttendance")]
        public IActionResult AddAttendance(int id)
        {
            var data = DateTime.Now;
            var Attd = new AttendanceDetails();
            if (dataContext.LeaveModel.Any(x => x.StartDate.Date == data.Date.Date && x.LeaveDay == "HalfDay" || x.LeaveDay == "Permission"))
            {
                if (!dataContext.AttendanceModel.Any(x => x.EmployeeId == id && x.Date.Date == data.Date.Date))
                {
                    Attd.IsChecked = true;
                    Attd.Date = data.Date;
                    Attd.Status = "Present";
                    Attd.InTime = data;
                    Attd.WorkDuration = "0";
                    Attd.EmployeeId = id;
                    dataContext.AttendanceModel.Add(Attd);
                    dataContext.SaveChanges();
                    return Ok(Attd);
                }
                return NotFound();
            }

            else if (!dataContext.AttendanceModel.Any(x => x.EmployeeId == id && x.Date.Date == data.Date.Date))
            {
                var attendance = dataContext.LeaveModel.Any(x => x.EmployeeId == id && x.StartDate.Date <= data.Date.Date && x.EndDate.Date >= data.Date.Date && x.AdminApprovalStatus != "Rejected");
                if (!attendance)
                {
                    Attd.IsChecked = true;
                    Attd.Date = data.Date;
                    Attd.Status = "Present";
                    Attd.InTime = data;
                    Attd.EmployeeId = id;
                    Attd.WorkDuration = "0";
                    dataContext.AttendanceModel.Add(Attd);
                    dataContext.SaveChanges();
                    return Ok(Attd);
                }
                return BadRequest();
            }
            return NotFound();
        }
                
        [HttpGet("updateAttendance")]
        public IActionResult updateAttendance(int id)
        {
            var data = DateTime.Now;
            var res = dataContext.AttendanceModel.ToList().LastOrDefault(a => a.EmployeeId == id && a.IsChecked == true);
            if (res == null)
            {
                return NotFound();
            }
            res.IsChecked = false;
            res.OutTime = DateTime.Now;
            var diff = data.TimeOfDay.TotalMinutes - res.InTime.TimeOfDay.TotalMinutes;
            var hour = (int)diff / 60;
            var minute = (int)diff % 60;
            res.WorkDuration = hour.ToString() + " Hrs " + minute.ToString() + " Min";
            dataContext.SaveChanges();
            return Ok(res);
        }

        [HttpGet("CheckAttendanceState")]
        public IActionResult CheckAttendanceState(int id)
        {
            if (dataContext.AttendanceModel.Any(a => a.EmployeeId == id && a.IsChecked == true)) { return Ok(true); }
            return Ok(false);
        }
        
        [HttpGet("AttendanceDetails")]
        public IActionResult AttendanceDetails(int data)
        {
           var user = dataContext.LoginModels.Where(x => x.EmployeeId == data).FirstOrDefault();
           /* if (user != null && user.Role !="Admin")
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
                                          }).ToList().GroupBy(a => a.Date.Month);
                return Ok(EmployeeAttendance);*/
            
           if (user != null && user.Role == "Admin" || user.Role == "Manager")
            {
                var AdminAttendance = (from a in dataContext.EmployeeModel
                                   join l in dataContext.AttendanceModel on a.EmployeeId equals l.EmployeeId
                                   orderby a.FirstName ascending
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

                                   }).ToList().OrderByDescending(a=>a.Date).GroupBy(a => a.Date);
          
           
                return Ok(AdminAttendance);
            }
            if (user != null && user.Role =="TeamLead")
            {
                var EmployeeAttendance = (from x in dataContext.LoginModels
                                          join a in dataContext.EmployeeModel on x.EmployeeId equals a.EmployeeId
                                          join l in dataContext.AttendanceModel on x.EmployeeId equals l.EmployeeId
                                       orderby a.FirstName ascending
                                       where x.ReportingId==data 
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

                                       }).ToList().OrderByDescending(a => a.Date).GroupBy(a => a.Date);


                return Ok(EmployeeAttendance);
            }

            return BadRequest();
        }
        [HttpGet("UserAttendance")]
        public IActionResult UserAttendance(int data)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == data).FirstOrDefault();
            if (user != null && user.Role != "Admin")
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
                                          }).ToList().Take(7);
                return Ok(EmployeeAttendance);
            }
            return NotFound();
        }
        [HttpGet("MyAttendance")]
        public IActionResult Myattendance(int data)
        {
        var user = dataContext.LoginModels.Where(x => x.EmployeeId == data).FirstOrDefault();
        if (user != null && user.Role != "Admin")
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
                                      }).ToList().GroupBy(a => a.Date.Month);
            return Ok(EmployeeAttendance); 
            }
            return NotFound();
        }

        [HttpPost("FilteredItems")]
        public IActionResult GetFilterDetails(int id, [FromBody] RequestModel requestModel)
        {
            var user = dataContext.LoginModels.Where(x => x.EmployeeId == id).FirstOrDefault();

            if (user != null && (user.Role == "Admin" || user.Role == "Manager") && requestModel.User != 0)
            {
                var AttendanceFilter = (from l in dataContext.LoginModels
                                        join a in dataContext.AttendanceModel on l.EmployeeId equals a.EmployeeId
                                        join e in dataContext.EmployeeModel on l.EmployeeId equals e.EmployeeId
                                        where l.EmployeeId == requestModel.User && a.Date.Date >= requestModel.FromDate.Date && a.Date.Date <= requestModel.ToDate.Date
                                        orderby a.Date
                                        select new
                                        {
                                            e.FirstName,
                                            e.LastName,
                                            l.EmployeeId,
                                            a.Date,
                                            a.InTime,
                                            a.OutTime,
                                            a.WorkDuration,
                                            a.Status,
                                        }).ToList();

                var LeaveFilter = (from a in dataContext.LoginModels
                                   join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                                   join e in dataContext.EmployeeModel on l.EmployeeId equals e.EmployeeId
                                   where (l.StartDate <= requestModel.ToDate && l.EndDate >= requestModel.FromDate) || (l.StartDate.Date >= requestModel.FromDate.Date && l.StartDate.Date <= requestModel.ToDate.Date) && (l.EndDate.Date >= requestModel.FromDate.Date || l.EndDate.Date <= requestModel.ToDate.Date)
                                   where l.EmployeeId == requestModel.User && l.AdminApprovalStatus == "Approved"
                                   orderby l.StartDate descending

                                   select new
                                   {
                                       e.FirstName,
                                       e.LastName,
                                       l.EmployeeId,
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

                var Filter = new
                {
                    AttendanceFilter,
                    LeaveFilter
                };

                return Ok(Filter);
            }
            if (user != null && (user.Role == "Admin" || user.Role == "Manager") && requestModel.User == 0)
            {
                var AttendanceFilter = (from l in dataContext.LoginModels
                                        join a in dataContext.AttendanceModel on l.EmployeeId equals a.EmployeeId
                                        join e in dataContext.EmployeeModel on l.EmployeeId equals e.EmployeeId
                                        where a.Date.Date >= requestModel.FromDate.Date && a.Date.Date <= requestModel.ToDate.Date
                                        orderby e.FirstName ascending
                                        select new
                                        {
                                            e.FirstName,
                                            e.LastName,
                                            l.EmployeeId,
                                            a.Date,
                                            a.InTime,
                                            a.OutTime,
                                            a.WorkDuration,
                                            a.Status,
                                        }).ToList().OrderByDescending(a=>a.Date);

                var LeaveFilter = (from a in dataContext.LoginModels
                                   join l in dataContext.LeaveModel on a.EmployeeId equals l.EmployeeId
                                   join e in dataContext.EmployeeModel on l.EmployeeId equals e.EmployeeId
                                   where (l.StartDate <= requestModel.ToDate && l.EndDate >= requestModel.FromDate) || (l.StartDate.Date >= requestModel.FromDate.Date && l.StartDate.Date <= requestModel.ToDate.Date) && (l.EndDate.Date >= requestModel.FromDate.Date || l.EndDate.Date <= requestModel.ToDate.Date)
                                   where l.AdminApprovalStatus == "Approved"
                                   orderby e.FirstName ascending

                                   select new
                                   {
                                       e.FirstName,
                                       e.LastName,
                                       l.EmployeeId,
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
                                   }).ToList().OrderByDescending(a=>a.StartDate);

                var Filter = new
                {
                    AttendanceFilter,
                    LeaveFilter
                };

                return Ok(Filter);
            }
            return BadRequest();
        }

    }
}