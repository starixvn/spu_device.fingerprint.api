using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.FingerPrint.Model
{
   public class CookieEntity
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }
    }
}
