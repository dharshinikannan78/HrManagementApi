using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class FileAttachmentModel
    {
        [Key]

        public int AttachmentId { get; set; }
        [MaxLength(100)]
        public string AttachmentName { get; set; }
        [MaxLength(100)]
        public string AttachmentType { get; set; }
        [MaxLength(500)]
        public string AttachmentPath { get; set; }
        public string EmployeePhoto { get; set; }
    }
}
