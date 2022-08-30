using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class FileAttachmentModel
    {
        [Key]

        public int AttachmentId { get; set; }
        [MaxLength(100)]
        public string PhotoName { get; set; }
        [MaxLength(100)]
        public string PhotoPath { get; set; }
        [MaxLength(500)]
        public string ResumeName { get; set; }
        public string ResumePath { get; set; }
        public string CertificateName { get; set; }
        public string CertificatePath { get; set; }
    }
}
