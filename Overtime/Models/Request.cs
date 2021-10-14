using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Overtime.Models
{
    public class Request
    {
        public int Id { get; set; }
        public string userId { get; set; }
        public string job_Task { get; set; }
        public string Desc { get; set; }
        public DateTime Date { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public enum Status
        {
            Accepted,
            Decline,
            Proses
        }
        public Status StatusName { get; set; }
        public DateTime RequestDate { get; set; }
        public string ApproverName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalaryOvertime { get; set; }
        public virtual User User { get; set; }
    }
}
