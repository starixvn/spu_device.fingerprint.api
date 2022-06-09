namespace Device.FingerPrint.JobManagement.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class Job
    {
        public string JobName { get; set; }

        public string JobType { get; set; }

        public string APIUrl { get; set; }

        public string MethodName { get; set; }

        public string Controller { get; set; }

        public bool Active { get; set; }

        public IEnumerable<Trigger> Triggers { get; set; }        
        
    }
}
