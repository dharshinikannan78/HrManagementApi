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
                    data.Status = "Present";
                    data.InTime = DateTime.Now;
                    data.WorkDuration = "0";
                    dataContext.AttendanceModel.Add(data);
                    dataContext.SaveChanges();
                    return Ok(data);
                }
                return NotFound();
            }

            else if (!dataContext.AttendanceModel.Any(x => x.EmployeeId == data.EmployeeId && x.Date.Date == data.Date.Date))
            {
                var attendance = dataContext.LeaveModel.Any(x => x.EmployeeId == data.EmployeeId && x.StartDate.Date <= data.Date.Date && x.EndDate.Date >= data.Date.Date && x.AdminApprovalStatus != "Rejected");
                if (!attendance)
                {
                    data.Status = "Present";
                    data.InTime = DateTime.Now;
                    data.WorkDuration = "0";
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

            var res = dataContext.AttendanceModel.AsNoTracking().FirstOrDefault(a => a.AttendanceId == data.AttendanceId);
            if (res == null)
            {
                return NotFound();
            }
            else
            {
                res.Status = "Present";
                data.OutTime = DateTime.Now;
                var date = DateTime.Now;
                data.Date = date.Date;
                var diff = data.OutTime?.TimeOfDay.TotalMinutes - res.InTime.TimeOfDay.TotalMinutes;
                var hour = (int)diff / 60;
                var minute = (int)diff % 60;
                data.WorkDuration = hour.ToString() + " Hrs " + minute.ToString() + " Min";
                res.OutTime = data.OutTime;
                res.WorkDuration = data.WorkDuration;
                dataContext.Entry(res).State = EntityState.Modified;
                dataContext.SaveChanges();
                return Ok(data);
            }
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
            if (user != null && user.Role == "Employee" || user.Role == "Manager" || user.Role == "TeamLead")
            {
                return Ok(EmployeeAttendance);
            }
            return BadRequest();
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
                                   where l.AdminApprovalStatus == "Approved"
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
            return BadRequest();
        }
    }
}