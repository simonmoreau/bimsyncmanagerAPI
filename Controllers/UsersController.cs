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

        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetById(long id)
        {
            var item = _context.Users.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        [HttpPost]
        public IActionResult Create([FromQuery]string code, string BCF)
        {
            if (code == null)
            {
                return BadRequest();
            }

            AccessToken accessToken = ObtainAccessToken(code).Result;
            bimsyncUser bsUser = GetCurrentUser(accessToken).Result;
            //https://api.bimsync.com/1.0/oauth/authorize?client_id=6E63g0C2zVOwlNm&response_type=code&redirect_uri=http://localhost:4200/callback"
            string codeBCF = "GpZSEANs63GBbSE";
            BCFToken bcfAccessToken = ObtainBCFToken(codeBCF).Result;

            var user = _context.Users.FirstOrDefault(t => t.bimsync_id == bsUser.id);
            if (user == null)
            {
                user = new User
                {
                    Name = bsUser.name,
                    bimsync_id = bsUser.id,
                    PowerBiSecret = System.Guid.NewGuid().ToString(),
                    AccessToken = accessToken.access_token,
                    TokenExpireIn = accessToken.expires_in,
                    TokenType = accessToken.token_type,
                    RefreshToken = accessToken.refresh_token,
                    BCFToken = "bcfTokenPlaceholder"
                };

                _context.Users.Add(user);
                _context.SaveChanges();
            }

            return CreatedAtRoute("GetUser", new { id = user.Id }, user);
        }

        private async Task<AccessToken> ObtainAccessToken(string authorization_code)
        {
            string callbackUri = "http://localhost:4200/callback";

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

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AccessToken));

            response.EnsureSuccessStatusCode(); 

            Stream responseStream = await response.Content.ReadAsStreamAsync();

            AccessToken accessToken = serializer.ReadObject(responseStream) as AccessToken;

            return accessToken;
        }

        private async Task<BCFToken> ObtainBCFToken(string authorization_code)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            string bodyContent = $"client_id={Configuration["client_id"]}" +
                    $"&client_secret={Configuration["client_secret"]}" +
                    $"&code={authorization_code}" +
                    $"&grant_type=authorization_code";

            bodyContent = "client_id=6E63g0C2zVOwlNm&client_secret=gGYTHliWio6LBzZ&code=GpZSEANs63GBbSE&grant_type=authorization_code";

            HttpContent body = new StringContent(bodyContent, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

            string clientURL = "https://api.bimsync.com/1.0/oauth/access_token";

            HttpResponseMessage response = await client.PostAsync(clientURL, body);

            response.EnsureSuccessStatusCode();

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

            response.EnsureSuccessStatusCode();

            Stream responseStream = await response.Content.ReadAsStreamAsync();

            bimsyncUser bimsyncUser = serializer.ReadObject(responseStream) as bimsyncUser;

            return bimsyncUser;
        }
    }
}