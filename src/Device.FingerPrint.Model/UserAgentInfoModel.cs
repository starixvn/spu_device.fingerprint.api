using System;

namespace Device.FingerPrint.Model
{
    public class UserAgentInfoModel : IEquatable<UserAgentInfoModel>
    {
        public long Id { get; set; }

        public string ClientType { get; set; }

        public string ClientName { get; set; }

        public string ClientVersion { get; set; }

        public string OSName { get; set; }

        public string OSShortName { get; set; }

        public string OSVersion { get; set; }

        public string OSPlatform { get; set; }

        public string Device { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public string BotName { get; set; }

        public string BotCategory { get; set; }

        public bool Equals(UserAgentInfoModel other)
        {
            if (Object.ReferenceEquals(other, null)) return false;
            if (Object.ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }
        public override int GetHashCode()
        {
            int hashProductCode = Id.GetHashCode();
            return hashProductCode;
        }
    }
}
