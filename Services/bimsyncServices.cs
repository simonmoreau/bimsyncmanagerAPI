using Microsoft.Extensions.Configuration;

namespace bimsyncManagerAPI.Services
{
    public class bimsyncServices
    {
        public static IConfiguration Configuration { get; set; }
        //https://api.bimsync.com/oauth2/authorize?client_id=6E63g0C2zVOwlNm&response_type=code&redirect_uri=http://localhost:4200/callback

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
}
