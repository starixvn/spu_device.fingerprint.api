using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceFingerprint.WebAPI.Models
{
    public class TokenResultModel
    {
        public string EncryptedKey { get; set; }

        public string JsScript { get; set; }

        public string Token { get; set; }
    }
}
