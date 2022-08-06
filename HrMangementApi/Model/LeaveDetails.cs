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
        
        public string LeaveType { get; set; }
        public DateTime AppliedOn { get; set; }
        public string ApprovalStatus { get; set; }
        public string Reason { get; set; }
        public int NoofDaysLeave { get; set; }

    }
}
