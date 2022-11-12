using System;
using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class EmployeeDetails
    {
        [Key]
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Designation { get; set; }
        public string Address { get; set; }
        public string Number { get; set; }
        public string EmailId { get; set; }
        public DateTime DOB { get; set; }
        public DateTime JoiningDate { get; set; }
        public string EmployeeReferenceNo { get; set; }
        public string WorkMode { get; set; }
        public string AttachmentIds { get; set; }
        public string TeamName { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string ModifiedBy { get; set; }
        public string DateOfModified { get; set; }
        public string Qualification { get; set; }
        public string College { get; set; }
        public string Skills { get; set; }
        public string PassedOut { get; set; }
    }
}
