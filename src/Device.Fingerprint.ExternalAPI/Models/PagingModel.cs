namespace Device.Fingerprint.ExternalAPI.Models
{
    public class PagingModel
    {
        public int Length { get; set; }

        public int Take { get; set; }

        public int Skip { get; set; }

        public int TotalItem { get; set; }
    }
}
