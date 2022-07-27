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
        public DateTime Date { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public string WorkDuration { get; set; }
        public string OverTimeDuration { get; set; }
        public string Location { get; set; }


    }
}
