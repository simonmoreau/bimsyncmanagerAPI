using System;
using Microsoft.Extensions.Configuration;

namespace bimsyncManagerAPI.Services
{
    public class bimsyncServices
    {
        public static IConfiguration Configuration { get; set; }
        public bimsyncServices(IConfiguration configuration)
        {
            Configuration = configuration;
        }
    }


    public class AccessToken
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }
    public class BCFToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
    }
    public class bimsyncUser
    {
        public string createdAt { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
    }
}
