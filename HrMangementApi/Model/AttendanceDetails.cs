using System;
using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class AttendanceDetails
    {
        [Key]
        public int AttendanceId { get; set; }
        public int EmployeeId { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public int WorkDuration { get; set; }
        public int OverTimeDuration { get; set; }
        public string Location { get; set; }


    }
}
