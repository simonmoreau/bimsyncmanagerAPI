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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace bimsyncManagerAPI.Controllers
{
    [Route("")]
    public class bimsyncController
    {
        string access_token;
        bool useProxy = false;
        private IConfiguration Configuration { get; set; }

        private List<Data.PropertyDefinition> propertyDefinitions = new List<Data.PropertyDefinition>();
        private string header = "revisionId;objectId;ifcType;type";
        public bimsyncController(IConfiguration configuration)
        {
            this.access_token = "XYD2G24HTlPgfw6aet6ZbX";
            Configuration = configuration;
        }

        public async Task GetProjects()
        {
            string path = @"C:\Users\smoreau\Documents\log.txt";
            string outputPath = @"C:\Users\smoreau\Documents\output.txt";

            File.AppendAllText(outputPath, header + "\r\n");
            string baseUrl = @"https://api.bimsync.com/v2/projects/134c9142631f4b9c8cb905653f54e44b/ifc/products?pageSize=1000";
            int pageNumber = await GetPageNumber(baseUrl);

            //Create a list of elements
            List<Data.Element> elements = new List<Data.Element>();
            //Create a list of property definitions (=columns)
            List<Data.PropertyDefinition> propertyDefinitions = new List<Data.PropertyDefinition>();

            for (int j = 1; j < pageNumber; j = j + 10)
            {
                List<Task<List<bimsync.IfcElement>>> downloadIfcElementsTasks = new List<Task<List<bimsync.IfcElement>>>();
                List<Data.Task> customTasks = new List<Data.Task>();

                for (int i = j; i < j + 10; i++)
                {
                    string requestUrl = baseUrl + "&page=" + i.ToString();
                    Task<List<bimsync.IfcElement>> downloadIfcElementsTask = GetRessource(requestUrl);
                    downloadIfcElementsTasks.Add(downloadIfcElementsTask);
                    customTasks.Add(new Data.Task { AsyncTask = downloadIfcElementsTask, url = requestUrl });
                }

                // ***Add a loop to process the tasks one at a time until none remain.  
                while (downloadIfcElementsTasks.Count > 0)
                {
                    // Identify the first task that completes.  
                    Task<List<bimsync.IfcElement>> firstFinishedTask = await Task.WhenAny(downloadIfcElementsTasks);

                    //Write it
                    Data.Task customTask = customTasks.Where(t => t.AsyncTask == firstFinishedTask).FirstOrDefault();
                    if (customTask != null)
                    {
                        File.AppendAllText(path, customTask.url + "\t" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
    CultureInfo.InvariantCulture) + "\r\n");
                    }

                    // ***Remove the selected task from the list so that you don't  
                    // process it more than once.  
                    downloadIfcElementsTasks.Remove(firstFinishedTask);

                    // Await the completed task.  
                    List<bimsync.IfcElement> ifcElements = await firstFinishedTask;

                    //Process the received elements
                    foreach (bimsync.IfcElement ifcElement in ifcElements)
                    {
                        Data.Element element = new Data.Element();

                        element.ifcType = ifcElement.ifcType;
                        element.revisionId = ifcElement.revisionId;
                        element.objectId = ifcElement.objectId;
                        element.type = "";

                        if (ifcElement.type != null)
                        {
                            element.type = ifcElement.type.objectId.ToString();
                        }

                        element.properties = GetIfcElementProperties(ifcElement);
                        propertyDefinitions = UpdatePropertyDefinitions(element.GetPropertyDefinitions(), propertyDefinitions);
                        //elements.Add(element);

                        File.AppendAllText(outputPath, element.ToString(propertyDefinitions) + "\r\n");
                    }

                    File.AppendAllText(path, ifcElements.Count.ToString() + " have been processed\t" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                 CultureInfo.InvariantCulture) + "\r\n");
                }
            }


            File.AppendAllText(outputPath, header + "\r\n");
        }

        private List<Data.PropertyDefinition> UpdatePropertyDefinitions(List<Data.PropertyDefinition> newPropertyDefinitionsValues, List<Data.PropertyDefinition> initialPropertyDefinitions)
        {
            Data.PropertyDefinitionEqualityComparer comparer = new Data.PropertyDefinitionEqualityComparer();

            foreach (Data.PropertyDefinition newValue in newPropertyDefinitionsValues)
            {
                if (!initialPropertyDefinitions.Contains(newValue, comparer))
                {
                    initialPropertyDefinitions.Add(newValue);
                    header = header + ";" + newValue.Name;
                    if (!string.IsNullOrEmpty(newValue.Unit))
                    {
                        header = header + " (" + newValue.Unit + ")";
                    }
                }
            }

            return initialPropertyDefinitions;
        }

        private async Task<List<bimsync.IfcElement>> GetRessource(string url)
        {
            HttpResponseMessage response = await GetResponseFromBimsync(url);

            Stream responseStream = await response.Content.ReadAsStreamAsync();

            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(responseStream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                List<bimsync.IfcElement> objects = serializer.Deserialize(jsonTextReader, typeof(List<bimsync.IfcElement>)) as List<bimsync.IfcElement>;
                return objects;
            }
        }

        private async Task<int> GetPageNumber(string url)
        {
            int pageNumber = 1;

            HttpResponseMessage response = await GetResponseFromBimsync(url);

            // parse response headers
            KeyValuePair<string, IEnumerable<string>> link = response.Headers
               .FirstOrDefault(q => string.Compare(q.Key, "Link", true) == 0);

            string linkValue = link.Value.FirstOrDefault().ToString();
            string[] values = linkValue.Split(',');
            string lastPage = values.FirstOrDefault(x => x.Contains("rel=\"last\""));

            Match match = new Regex(@"page=.*?>").Match(lastPage);
            if (match.Success)
            {
                int.TryParse(match.Value.Replace("page=", "").Trim('>'), out pageNumber);
            }

            return pageNumber;
        }

        private async Task<HttpResponseMessage> GetResponseFromBimsync(string url)
        {
            MyProxy proxy = new MyProxy("http://proxymon.bouygues-immobilier.com:8080", Configuration);

            HttpClientHandler httpClientHandler = new HttpClientHandler();

            if (useProxy)
            {
                httpClientHandler.UseProxy = true;
                httpClientHandler.Proxy = proxy;
            };

            HttpClient client = new HttpClient(httpClientHandler);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + access_token);

            HttpResponseMessage response = await client.GetAsync(url);
            try
            {
                 response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " - " + url + " - " + response.StatusCode + " - " + response.ReasonPhrase);
            }
           
            return response;
        }

        private List<Data.Property> GetIfcElementProperties(bimsync.IfcElement IfcElement)
        {
            //Create a list of property
            List<Data.Property> dataProperties = new List<Data.Property>();

            //Loop on property sets
            JObject propertySets = IfcElement.propertySets as JObject;
            if (propertySets != null) { dataProperties.AddRange(GetProperties(propertySets)); }

            //Loop on quantity sets
            JObject quantitySets = IfcElement.quantitySets as JObject;
            if (quantitySets != null)
            {
                if (quantitySets.Children().Count() != 0)
                {
                    dataProperties.AddRange(GetProperties(quantitySets));
                }
            }

            //if (quantitySets != null) {  }


            //Loop on attributes
            JObject attributes = IfcElement.attributes as JObject;
            if (attributes != null)
            {
                foreach (JToken jTokenattribute in attributes.Children())
                {
                    bimsync.AttributeProperty property = jTokenattribute.Children().First().ToObject<bimsync.AttributeProperty>();

                    property.Name = jTokenattribute.Path;

                    //Create a new data property
                    Data.Property dataProperty = new Data.Property();
                    dataProperty.Name = property.Name;
                    dataProperty.Unit = "string";
                    dataProperty.Value = (property.value == null) ? "" : property.value.ToString();
                    dataProperty.Type = property.ifcType;

                    dataProperties.Add(dataProperty);
                }
            }

            return dataProperties;
        }

        private List<Data.Property> GetProperties(JObject sets)
        {
            List<Data.Property> dataProperties = new List<Data.Property>();

            foreach (JToken jToken in sets.Children())
            {
                bimsync.Set set = jToken.Children().First().ToObject<bimsync.Set>();
                set.Name = jToken.Path;

                JObject properties = set.properties as JObject;
                if (properties == null) { properties = set.quantities as JObject; }

                if (properties != null)
                {
                    //Loop on properties
                    foreach (JToken jTokenProperty in properties.Children())
                    {
                        bimsync.Property property = jTokenProperty.Children().First().ToObject<bimsync.Property>();

                        property.Name = jTokenProperty.Path;

                        //Create a new data property
                        Data.Property dataProperty = new Data.Property();
                        dataProperty.Name = set.Name + "." + property.Name;
                        dataProperty.Type = property.ifcType;

                        bimsync.Value value = null;
                        if (property.value != null)
                        {
                            value = property.value;
                        }
                        else if (property.nominalValue != null)
                        {
                            value = property.nominalValue;
                        }

                        if (value != null)
                        {
                            dataProperty.Unit = value.unit;
                            dataProperty.Value = value.value;
                        }


                        dataProperties.Add(dataProperty);
                    }

                }
            }

            return dataProperties;
        }
    }
}
