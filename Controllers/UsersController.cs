using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using bimsyncManagerAPI.Models;
using bimsyncManagerAPI.Services;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using System.Runtime.Serialization.Json;
using System.IO;

namespace bimsyncManagerAPI.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly UserContext _context;
        private IConfiguration Configuration { get; set; }
        public UsersController(UserContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;

            if (_context.Users.Count() == 0)
            {
                _context.Users.Add(new User { Name = "Item1" });
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public IEnumerable<User> GetAll()
        {
            return _context.Users.ToList();
        }

        /*                 [HttpGet]
                public string GetAll()
                {
                    return $"You have {_context.Users.ToList().Count} registred users";
                } */

        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetById(string id)
        {
            var item = _context.Users.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        [HttpPost]
        public IActionResult Create([FromQuery]string code, [FromBody]string callbackUri)
        {
            if (code == null)
            {
                return BadRequest();
            }
            if (callbackUri == null)
            {
                return BadRequest();
            }

            AccessToken accessToken = accessToken = ObtainAccessToken(code, callbackUri).Result;
            bimsyncUser bsUser = GetCurrentUser(accessToken).Result;
            //BCFToken bcfAccessToken = ObtainBCFToken(codeBCF).Result;

            var user = _context.Users.FirstOrDefault(t => t.bimsync_id == bsUser.id);
            if (user == null)
            {
                user = new User
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Name = bsUser.name,
                    bimsync_id = bsUser.id,
                    PowerBiSecret = System.Guid.NewGuid().ToString(),
                    AccessToken = accessToken.access_token,
                    TokenExpireIn = accessToken.expires_in,
                    RefreshDate = System.DateTime.Now + new System.TimeSpan(0, 0, accessToken.expires_in),
                    TokenType = accessToken.token_type,
                    RefreshToken = accessToken.refresh_token,
                    BCFToken = ""
                };

                _context.Users.Add(user);
                _context.SaveChanges();
            }
            else
            {
                user.AccessToken = accessToken.access_token;
                user.TokenExpireIn = accessToken.expires_in;
                user.RefreshDate = System.DateTime.Now + new System.TimeSpan(0, 0, accessToken.expires_in);
                user.TokenType = accessToken.token_type;
                user.RefreshToken = accessToken.refresh_token;

                _context.Users.Update(user);
                _context.SaveChanges();
            }

            return CreatedAtRoute("GetUser", new { id = user.Id }, user);
        }

        [HttpGet("bcf/{id}")]
        public IActionResult GetBCFToken([FromQuery]string code, string id)
        {
            User user = _context.Users.FirstOrDefault(t => t.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (code == null)
            {
                return BadRequest();
            }

            user.BCFToken = ObtainBCFToken(code).Result.access_token;

            _context.Users.Update(user);
            _context.SaveChanges();

            return new ObjectResult(user);
        }

        [HttpGet("refresh/{id}")]
        public IActionResult RefrechToken(string id)
        {
            User user = _context.Users.FirstOrDefault(t => t.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            AccessToken accessToken = RefreshAccessToken(user.RefreshToken).Result;

            user.AccessToken = accessToken.access_token;
            user.TokenExpireIn = accessToken.expires_in;
            user.RefreshDate = System.DateTime.Now + new System.TimeSpan(0, 0, accessToken.expires_in);
            user.TokenType = accessToken.token_type;
            user.RefreshToken = accessToken.refresh_token;

            _context.Users.Update(user);
            _context.SaveChanges();

            return new ObjectResult(user);
        }

        [HttpGet("powerbi")]
        public IActionResult GetPBToken([FromQuery]string PBCode)
        {
            User user = _context.Users.FirstOrDefault(t => t.PowerBiSecret == PBCode);
            if (user == null)
            {
                return NotFound();
            }

            if (user.RefreshDate < DateTime.Now)
            {
                AccessToken accessToken = RefreshAccessToken(user.RefreshToken).Result;

                user.AccessToken = accessToken.access_token;
                user.TokenExpireIn = accessToken.expires_in;
                user.RefreshDate = System.DateTime.Now + new System.TimeSpan(0, 0, accessToken.expires_in);
                user.TokenType = accessToken.token_type;
                user.RefreshToken = accessToken.refresh_token;

                _context.Users.Update(user);
                _context.SaveChanges();
            }

            return new ObjectResult(user.AccessToken);
        }

        private async Task<AccessToken> ObtainAccessToken(string authorization_code, string callbackUri)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            string bodyContent = $"grant_type=authorization_code" +
                    $"&code={authorization_code}" +
                    $"&redirect_uri={callbackUri}" +
                    $"&client_id={Configuration["client_id"]}" +
                    $"&client_secret={Configuration["client_secret"]}";

            HttpContent body = new StringContent(bodyContent, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

            string clientURL = "https://api.bimsync.com/oauth2/token";

            HttpResponseMessage response = await client.PostAsync(clientURL, body);

            //response.EnsureSuccessStatusCode();

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Stream errorStream = await response.Content.ReadAsStreamAsync();
                StreamReader reader = new StreamReader(errorStream);
                string text = reader.ReadToEnd();
                throw new Exception(text);
            }

            Stream responseStream = await response.Content.ReadAsStreamAsync();

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AccessToken));

            AccessToken accessToken = serializer.ReadObject(responseStream) as AccessToken;

            return accessToken;
        }

        private async Task<AccessToken> RefreshAccessToken(string refresh_token)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            string bodyContent = $"grant_type=refresh_token" +
                    $"&refresh_token={refresh_token}" +
                    $"&client_id={Configuration["client_id"]}" +
                    $"&client_secret={Configuration["client_secret"]}";

            HttpContent body = new StringContent(bodyContent, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

            string clientURL = "https://api.bimsync.com/oauth2/token";

            HttpResponseMessage response = await client.PostAsync(clientURL, body);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AccessToken));

            //response.EnsureSuccessStatusCode();

            Stream responseStream = await response.Content.ReadAsStreamAsync();

            AccessToken accessToken = serializer.ReadObject(responseStream) as AccessToken;

            return accessToken;
        }

        private async Task<BCFToken> ObtainBCFToken(string authorization_code)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();

            string bodyContent = $"client_id={Configuration["client_id"]}" +
                    $"&client_secret={Configuration["client_secret"]}" +
                    $"&code={authorization_code}" +
                    "&grant_type=authorization_code";

            HttpContent body = new StringContent(bodyContent, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

            string clientURL = "https://api.bimsync.com/1.0/oauth/access_token";

            HttpResponseMessage response = await client.PostAsync(clientURL, body);

            //response.EnsureSuccessStatusCode();

            Stream responseStream = await response.Content.ReadAsStreamAsync();

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BCFToken));
            BCFToken accessToken = serializer.ReadObject(responseStream) as BCFToken;

            //"client_id=6E63g0C2zVOwlNm&client_secret=gGYTHliWio6LBzZ&code=GMe9iPHclsn8XNY&grant_type=authorization_code"

            return accessToken;
        }

        private async Task<bimsyncUser> GetCurrentUser(AccessToken accessToken)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.access_token);

            HttpResponseMessage response = await client.GetAsync("https://api.bimsync.com/v2/user");

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(bimsyncUser));

            //response.EnsureSuccessStatusCode();

            Stream responseStream = await response.Content.ReadAsStreamAsync();

            bimsyncUser bimsyncUser = serializer.ReadObject(responseStream) as bimsyncUser;

            return bimsyncUser;
        }

        // GET api/users/pages
        [HttpGet("pages")]
        public IActionResult Get([FromQuery]string PBCode, [FromQuery]string ressource, [FromQuery]string revision)
        {
            if (ressource == null)
            {
                return BadRequest();
            }

            User user = _context.Users.FirstOrDefault(t => t.PowerBiSecret == PBCode);
            if (user == null)
            {
                return NotFound();
            }

            if (user.RefreshDate < DateTime.Now)
            {
                AccessToken accessToken = RefreshAccessToken(user.RefreshToken).Result;

                user.AccessToken = accessToken.access_token;
                user.TokenExpireIn = accessToken.expires_in;
                user.RefreshDate = System.DateTime.Now + new System.TimeSpan(0, 0, accessToken.expires_in);
                user.TokenType = accessToken.token_type;
                user.RefreshToken = accessToken.refresh_token;

                _context.Users.Update(user);
                _context.SaveChanges();
            }

            string test = GetPageNumber(ressource, "", user.AccessToken).Result;

            return new ObjectResult(test);
        }

        private async Task<string> GetPageNumber(string ressource, string revision, string access_token)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.bimsync.com/v2/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + access_token);

            string query = "";
            if (ressource.Contains("?"))
            {
                query = ressource + "&page=1&pageSize=1";
            }
            else
            {
                query = ressource + "?page=1&pageSize=1";
            }
            if (revision != null && revision != "") query = query + "&revision=" + revision;

            HttpResponseMessage response = await client.GetAsync(query);

            // parse response headers 
            KeyValuePair<string, IEnumerable<string>> link = response.Headers
               .FirstOrDefault(q => string.Compare(q.Key, "Link", true) == 0);

            if (link.Key == null) return "999";

            string linkValue = link.Value.FirstOrDefault().ToString();
            string[] values = linkValue.Split(',');
            string url = values.FirstOrDefault(x => x.Contains("rel=\"last\""));

            int pFrom = url.IndexOf("&page=") + "&page=".Length;
            int pTo = 0;
            if (url.IndexOf("&", pFrom) != -1)
            {
                pTo = url.IndexOf("&", pFrom);
            }
            else
            {
                pTo = url.IndexOf(">", pFrom);
            }

            if (pTo == 0) return null;

            String result = url.Substring(pFrom, pTo - pFrom);

            return result;

        }
    }
}