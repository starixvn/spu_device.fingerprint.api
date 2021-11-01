using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMVC.Models
{
    public class TokenModel
    {
        public int id { get; set; }
        public string token { get; set; }

        public string script { get; set; }
    }
}
