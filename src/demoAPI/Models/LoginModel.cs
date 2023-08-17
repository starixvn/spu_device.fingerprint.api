namespace demoAPI.Models
{
    public class LoginModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string RealIP { get; set; }

        public string XActualIP { get; set; }

        public string FakeIP { get; set; }

        public string XForwarded { get; set; }
    }
}