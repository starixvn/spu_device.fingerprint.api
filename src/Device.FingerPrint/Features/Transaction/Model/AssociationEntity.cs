using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class AssociationEntity
    {
        public long AssociationID { get; set; }               

        public string FirstDeviceCode { get; set; }

        public long DeviceID { get; set; }                

        public long AccountID { get; set; }

        public string LoginName { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
