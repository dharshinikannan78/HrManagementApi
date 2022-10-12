using System;
using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class TaskDetails
    {
        [Key]
        public int TaskId { get; set; }
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public string TaskStatus { get; set; }
        public int AssigingId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Priority { get; set; }
        public bool IsTaskArchieved { get; set; }

    }
}
