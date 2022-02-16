namespace Device.FingerPrint.JobManagement.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class GroupJob
    {
        public string Name { get; set; }

        public string HealthCheck { get; set; }

        public IEnumerable<Job> Jobs { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as GroupJob;

            if (other == null) return false;

            if (other.Name != Name || other.HealthCheck != HealthCheck || !EqualJobs(Jobs, other.Jobs))
            {

                return false;
            }

            return true;
        }

        private bool EqualJobs(IEnumerable<Job> jobs1, IEnumerable<Job> jobs2)
        {
            return jobs1.Count() == jobs2.Count() && jobs1.All(jobs2.Contains);
        }

        public override int GetHashCode() => (Name, HealthCheck, Jobs).GetHashCode();
    }
}
