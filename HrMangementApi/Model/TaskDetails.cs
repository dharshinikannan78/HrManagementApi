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


    }
}
