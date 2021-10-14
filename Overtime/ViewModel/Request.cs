using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Overtime.ViewModel
{
    public class Request
    {
        public int? RequestId { get; set; }
        public string jobTask { get; set; }
        public string Desc { get; set; }
        public DateTime Date { get; set; }
        public DateTime startTime { get; set; }
        public int MyProperty { get; set; }
    }
}
