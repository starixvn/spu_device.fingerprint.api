using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Dto
{
    public class TokenDto
    {
        public int Id { get; set; }
        public string Token { get; set; }

        public string Script { get; set; }
    }
}
