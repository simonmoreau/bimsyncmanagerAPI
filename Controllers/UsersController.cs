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
        public IActionResult Create([FromQuery]string code)
        {
            if (code == null)
            {
                return BadRequest();
            }

            AccessToken accessToken = ObtainAccessToken(code).Result;

            User user = new User
            {
                Name = "Item1",
                bimsync_id = "test",
                PowerBiSecret = System.Guid.NewGuid().ToString(),
                AccessToken = accessToken.access_token,
                TokenExpireIn = accessToken.expires_in,
                TokenType = accessToken.token_type,
                RefreshToken = accessToken.refresh_token
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtRoute("GetUser", new { id = user.Id }, user);
        }

        private async Task<AccessToken> ObtainAccessToken(string authorization_code)
        {
            string callbackUri = "http://localhost:4200/callback";

            HttpClient client = new HttpClient();
            //client.BaseAddress =  new Uri("https://api.bimsync.com/oauth2/token");
            client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            string test = Configuration["client_id"];

            string bodyContent = $"grant_type=authorization_code" +
                    $"&code={authorization_code}" +
                    $"&redirect_uri={callbackUri}" +
                    $"&client_id={Configuration["client_id"]}" +
                    $"&client_secret={Configuration["client_secret"]}";

            HttpContent body = new StringContent(bodyContent, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

            //"grant_type=authorization_code&code=I6MEOBKZyL&redirect_uri=http://localhost:4200/callback&client_id=6E63g0C2zVOwlNm&client_secret=gGYTHliWio6LBzZ"

            HttpResponseMessage response = await client.PostAsync("https://api.bimsync.com/oauth2/token", body);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AccessToken));

            response.EnsureSuccessStatusCode();

            Stream responseStream = await response.Content.ReadAsStreamAsync();

            AccessToken accessToken = serializer.ReadObject(responseStream) as AccessToken;

            return accessToken;
        }
    }
}