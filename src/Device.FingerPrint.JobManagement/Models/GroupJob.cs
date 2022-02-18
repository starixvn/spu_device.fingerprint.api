namespace Device.FingerPrint.JobManagement.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class GroupJob
    {
        public string Name { get; set; }

        public string HealthCheck { get; set; }

        public IEnumerable<Job> Jobs { get; set; }        
    }
}
