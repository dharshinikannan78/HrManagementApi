using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class ProjectMemberModel
    {
        [Key]
        public int projectMembersId { get; set; }
        public int ProjectId { get; set; }
        public int EmpId { get; set; }

    }
}
