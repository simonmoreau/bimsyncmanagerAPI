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
using System.Data;

namespace bimsyncManagerAPI.Controllers
{
    [Route("")]
    public class bimsyncController
    {
        string access_token;
        bool useProxy = true;
        private IConfiguration Configuration { get; set; }

        private List<Data.PropertyDefinition> propertyDefinitions = new List<Data.PropertyDefinition>();

        private string header = "revisionId;objectId;ifcType;type";
        public bimsyncController(IConfiguration configuration)
        {
            this.access_token = "vrlvxruX42d01cXdQG2Xxr";
            Configuration = configuration;
        }

        public async Task GetProducts()
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "log.txt");

            //string[] revisionIds = { "41462bb6ee9943739813cc5faa509790", "6d4f0c182ba343ee958ef0320eab47d6", "b107a1fe85c94fa49c60f7cd3f708c6f", "02fe5e8b4d0c480286791a06b2f99ba2", "0c730b4fb8cd445b8fe343feba680086", "111787cf86214bfb80e604066e1320fb", "5c59855fe3b745938aba5ef25ffaa2d3", "03018cbb576745329efec8e593827b84", "9d699227b45745f7b0e821df25229af0", "c28f5bf1266e4d65862d645b90393227", "19405a4742c0443cbf46fac9c8e6acfd", "c1e32d6b3c1046288c53a826d6140e8b", "cc893efdc30e4ab6a84c488793e93f43", "5d670e7d792546e384b2889b11cc9967", "0b530c7d153f457aba01228af44d0033", "6a74a0fac1be4776a6e107baf067210b", "7ed057bedab94ecdbf2b690f3cdf6731", "d494f696ef09477a9a99058f1adc19a6", "fb2dfe7e44ae4b4aac00a8b38c022280", "5932328a810f481d97bc6aeccd074f60", "831f7810a6714e9db5c7782cdf747a38", "8cf01d3e2ce34d49844c3f7a39f18538", "acdc011bf5af43388df5369b723536e0", "c334bc5c474545dd928f47111e0f1b02", "c63a4f3afa4b46dba8e4a4f999fa61d9", "5c0a6cbdcee6420784ce9a7710eb4c99", "f4dd40ee0c434b379a57dfe0cfb11664", "351fbe83739c4f268aabb86525cbf491", "5113aea6f33f44719638257e4b47d4da", "59e3a7c30582415f874c13856dd7ce01", "7708f4535987425c8245424d903ef98f", "671968a27e51457ea001c23c86283f6c", "ad1769755e9a4592addaa8a21a60dc54", "c5a76a1a46144c26b31007159ddca531", "10f06d01f0784af99509355e7770f2b9", "6ab9e2f23dee46409979780cc6b7c4b7", "6d798f7383d94cd0b10a9403d5018df8", "f678803b5375400c80b111f28987e963", "21ced4bfe1564fe38757c6402355fe41", "48e358bf3fcd4efbb2738f6c784efa5e", "9e55b0d44c284974b9f4b329895dfd77", "be4c168c4d384d72ac39954380b47ed0", "186f492878ca40798641c2ed31bf04b3", "4987a4827ef14e1187046b417f748dde", "d5540fd37d084a178dcaf47a5671cb86", "dd718c3ef6274df08b37ef677172cbd2" };
            string[] revisionIds = { "6d4f0c182ba343ee958ef0320eab47d6", "b107a1fe85c94fa49c60f7cd3f708c6f", "02fe5e8b4d0c480286791a06b2f99ba2", "0c730b4fb8cd445b8fe343feba680086", "111787cf86214bfb80e604066e1320fb", "5c59855fe3b745938aba5ef25ffaa2d3", "03018cbb576745329efec8e593827b84", "9d699227b45745f7b0e821df25229af0", "c28f5bf1266e4d65862d645b90393227", "19405a4742c0443cbf46fac9c8e6acfd", "c1e32d6b3c1046288c53a826d6140e8b", "cc893efdc30e4ab6a84c488793e93f43", "5d670e7d792546e384b2889b11cc9967", "0b530c7d153f457aba01228af44d0033", "6a74a0fac1be4776a6e107baf067210b", "7ed057bedab94ecdbf2b690f3cdf6731", "d494f696ef09477a9a99058f1adc19a6", "fb2dfe7e44ae4b4aac00a8b38c022280", "831f7810a6714e9db5c7782cdf747a38", "8cf01d3e2ce34d49844c3f7a39f18538", "acdc011bf5af43388df5369b723536e0", "c334bc5c474545dd928f47111e0f1b02", "c63a4f3afa4b46dba8e4a4f999fa61d9", "5c0a6cbdcee6420784ce9a7710eb4c99", "f4dd40ee0c434b379a57dfe0cfb11664", "351fbe83739c4f268aabb86525cbf491", "5113aea6f33f44719638257e4b47d4da", "59e3a7c30582415f874c13856dd7ce01", "7708f4535987425c8245424d903ef98f", "671968a27e51457ea001c23c86283f6c", "ad1769755e9a4592addaa8a21a60dc54", "c5a76a1a46144c26b31007159ddca531", "10f06d01f0784af99509355e7770f2b9", "6ab9e2f23dee46409979780cc6b7c4b7", "6d798f7383d94cd0b10a9403d5018df8", "f678803b5375400c80b111f28987e963", "21ced4bfe1564fe38757c6402355fe41", "48e358bf3fcd4efbb2738f6c784efa5e", "9e55b0d44c284974b9f4b329895dfd77", "be4c168c4d384d72ac39954380b47ed0", "186f492878ca40798641c2ed31bf04b3", "4987a4827ef14e1187046b417f748dde", "d5540fd37d084a178dcaf47a5671cb86", "dd718c3ef6274df08b37ef677172cbd2" };

            //string[] revisionIds = { "5932328a810f481d97bc6aeccd074f60" };

            using (StreamWriter logFile = new StreamWriter(logPath))
            {
                foreach (string revisionId in revisionIds)
                {
                    await GetProductsForARevision(revisionId, logFile);
                }
            }
        }


        public async Task GetProductsForARevision(string revisionId, StreamWriter logFile)
        {
            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), revisionId + "_output.txt");

            string baseUrl = @"https://api.bimsync.com/v2/projects/134c9142631f4b9c8cb905653f54e44b/ifc/products?pageSize=1000&revision=" + revisionId;
            int pageNumber = await GetPageNumber(baseUrl);
            int taskNumber = 50;

            //Create a list of elements
            List<Data.SortedIfcElement> elements = new List<Data.SortedIfcElement>();
            //Create a list of property definitions (=columns)
            List<Data.PropertyDefinition> propertyDefinitions = new List<Data.PropertyDefinition>();

            List<Task<List<bimsync.IfcElement>>> downloadIfcElementsTasks = new List<Task<List<bimsync.IfcElement>>>();
            List<Data.Task> customTasks = new List<Data.Task>();

            //Create a dataTable
            DataSet pictureMetadata = new DataSet();
            DataTable metadataTable = pictureMetadata.Tables.Add("metadataTable");

            for (int j = 1; j < pageNumber + 1; j = j + taskNumber)
            {
                for (int i = j; i < pageNumber + 1 & i < j + taskNumber; i++)
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
                        logFile.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "\t" + revisionId + "\t" + customTask.url + "\t" + "Start task");
                    }

                    // ***Remove the selected task from the list so that you don't  
                    // process it more than once.  
                    downloadIfcElementsTasks.Remove(firstFinishedTask);

                    // Await the completed task.  
                    List<bimsync.IfcElement> ifcElements = await firstFinishedTask;

                    logFile.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "\t" + revisionId + "\t" + customTask.url + "\t" + "End task");

                    //Process the received elements
                    foreach (bimsync.IfcElement ifcElement in ifcElements)
                    {
                        //Create a sorted IfcElement
                        Data.SortedIfcElement element = new Data.SortedIfcElement(ifcElement);
                        //Add it to the datatable
                        element.AddToDataTable(metadataTable);
                    }

                    //Write the result to a text file

                    logFile.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "\t" + revisionId + "\t" + customTask.url + "\t" + "Process elements");

                }
            }

            //Write down the content of the metatable
            using (StreamWriter sw = File.CreateText(outputPath))
            {
                sw.WriteLine(String.Join("\t", metadataTable.Columns.Cast<DataColumn>().Select(item => item.ColumnName).ToList()));

                foreach (DataRow row in metadataTable.Rows)
                {
                    sw.WriteLine(String.Join("\t", row.ItemArray));
                }
            }

        }

        private async Task<List<bimsync.IfcElement>> GetRessource(string url)
        {
            using (HttpResponseMessage response = await GetResponseFromBimsync(url))
            {
                Stream responseStream = await response.Content.ReadAsStreamAsync();

                var serializer = new JsonSerializer();

                using (var sr = new StreamReader(responseStream))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    List<bimsync.IfcElement> objects = serializer.Deserialize(jsonTextReader, typeof(List<bimsync.IfcElement>)) as List<bimsync.IfcElement>;

                    return objects;
                }
            }
        }

        private async Task<int> GetPageNumber(string url)
        {
            int pageNumber = 1;

            HttpResponseMessage response = await GetResponseFromBimsync(url);

            // parse response headers
            KeyValuePair<string, IEnumerable<string>> link = response.Headers
               .FirstOrDefault(q => string.Compare(q.Key, "Link", true) == 0);

            if (link.Value != null)
            {
                string linkValue = link.Value.FirstOrDefault().ToString();
                string[] values = linkValue.Split(',');
                string lastPage = values.FirstOrDefault(x => x.Contains("rel=\"last\""));

                Match match = new Regex(@"page=.*?[>&]").Match(lastPage);
                if (match.Success)
                {
                    int.TryParse(match.Value.Replace("page=", "").Trim('>').Trim('&'), out pageNumber);
                }
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
    }
}
