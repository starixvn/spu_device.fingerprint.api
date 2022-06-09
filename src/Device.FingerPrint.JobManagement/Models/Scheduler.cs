namespace Device.FingerPrint.JobManagement.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class Scheduler
    {
        public IEnumerable<GroupJob> GroupJobs { get; set; }
    }
}
