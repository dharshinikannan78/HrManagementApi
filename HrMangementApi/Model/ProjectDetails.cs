using System;
using System.ComponentModel.DataAnnotations;

namespace HrMangementApi.Model
{
    public class ProjectDetails
    {
        [Key]
        public int ProjectId { get; set; }
        public int AssiginedId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectTitle { get; set; }
        public string ProjectDescription { get; set; }

        public DateTime StartDate { get; set; }

        public string ProjectStatus { get; set; }

        public DateTime EndDate { get; set; }
        public string TodayDays { get; set; }
        public string CreatedBy { get; set; }


    }
}
