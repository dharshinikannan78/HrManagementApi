using System;

namespace HrMangementApi.Model
{
    public class RequestModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int User { get; set; }
    }
}
