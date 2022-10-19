using System;
using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class LeaveDetails
    {
        [Key]
        public int LeaveId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveDay { get; set; }
        public string LeaveType { get; set; }
        public DateTime AppliedOn { get; set; }
        public string TeamLeadApprovalStatus { get; set; }
        public string ManagerApprovalStatus { get; set; }
        public string AdminApprovalStatus { get; set; }
        public string LeaveReason { get; set; }
        public string NoOfDays { get; set; }
        public string TeamLeadRejectReason { get; set; }
        public string ManagerRejectReason { get; set; }
        public string AdminRejectReason { get; set; }

    }
}
