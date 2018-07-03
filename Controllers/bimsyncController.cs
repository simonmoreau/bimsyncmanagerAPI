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
using System.Text;
using System.Text.RegularExpressions;

namespace bimsyncManagerAPI.Controllers
{
    [Route("")]
    public class bimsyncController
    {
        string access_token;
        private IConfiguration Configuration { get; set; }
        public bimsyncController(IConfiguration configuration)
        {
            this.access_token = "sYGMUBnxOdz2VQgjblY3TH";
            Configuration = configuration;
        }

        public async Task GetProjects()
        {
            string url = @"https://api.bimsync.com/v2/projects/134c9142631f4b9c8cb905653f54e44b/ifc/products?pageSize=1000";
            List<bimsync.IfcElement> test = await GetRessource(url);
        }

        private async Task<List<bimsync.IfcElement>> GetRessource(string url)
        {


            MyProxy proxy = new MyProxy("http://proxymon.bouygues-immobilier.com:8080",Configuration);


            HttpClientHandler httpClientHandler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = proxy
            };


            HttpClient client = new HttpClient(httpClientHandler);
            //client.BaseAddress = new Uri("https://api.bimsync.com/v2/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + access_token);

            HttpResponseMessage response = await client.GetAsync(url);

            //Get the response as a list
            var byteArray = await response.Content.ReadAsByteArrayAsync();
            var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);

            Stream responseStream = await response.Content.ReadAsStreamAsync();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<bimsync.IfcElement>));
            List<bimsync.IfcElement> objects = serializer.ReadObject(responseStream) as List<bimsync.IfcElement>;

            // parse response headers
            KeyValuePair<string, IEnumerable<string>> link = response.Headers
               .FirstOrDefault(q => string.Compare(q.Key, "Link", true) == 0);

            string linkValue = link.Value.FirstOrDefault().ToString();
            string[] values = linkValue.Split(',');
            string nextValue = values.FirstOrDefault(x => x.Contains("rel=\"next\""));


            Match match = new Regex(@"<.*?>").Match(nextValue);
            if (match.Success)
            {
                string nextUrl = match.Value.Trim('<').Trim('>');
                objects.AddRange(await GetRessource(nextUrl));
            }

            return objects;

        }
    }
}
