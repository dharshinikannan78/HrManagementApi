using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class TaskDetails
    {
        [Key]
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string Summary { get; set; }
        public int EmployeeId { get; set; }
    }
}
