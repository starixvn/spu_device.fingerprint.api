using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;
using WebApi.Dto;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public TokenController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public ActionResult<TokenDto> Get()
        {
            TokenDto dto = new TokenDto();
            string token = GetHashMAC(1);
            string script = GetScript(1);
            dto.Id = 1;
            dto.Token = token;
            dto.Script = script;
            return dto;
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult<TokenDto> GetToken(int id)
        {
            TokenDto dto = new TokenDto();
            string token = GetHashMAC(id);
            string script = GetScript(id);
            dto.Id = id;
            dto.Token = token;
            dto.Script = script;
            return dto;
        }

        static string GetHashMAC(int subscriberId)
        {
            var key = new StringBuilder();
            var rand = new Random();

            key.Append(subscriberId);
            key.Append(DateTimeOffset.Now.ToUnixTimeMilliseconds());
            key.Append(rand.Next(1000, 999999));

            return key.ToString();
        }

        private string GetScript(int subscriberId)
        {
            var path = GetFilePath();
            path += "\\js\\gfp.min.js";
            return System.IO.File.ReadAllText(path);
        }
        private string GetFilePath()
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string contentRootPath = _hostingEnvironment.ContentRootPath;
            return contentRootPath;
        }
    }
}
