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
    [Route("")]
    public class HomeController : Controller
    {
        private readonly UserContext _context;
        private IConfiguration Configuration { get; set; }
        public HomeController(UserContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Welcome to the bimsyncManager API" };
        }

        [HttpGet("check")]
        public string Check()
        {
            try
            {

                //Configuration["client_id"] = Environment.GetEnvironmentVariable("APPSETTING_client_id");
                //Configuration["client_secret"] = Environment.GetEnvironmentVariable("APPSETTING_client_secret");
                string client_id = Configuration["client_id"];
                string client_secret = Configuration["client_secret"];
                return client_id + " - " + client_secret;
            }  
            catch
            {
                return "Failed";
            }
        }
    }
}
