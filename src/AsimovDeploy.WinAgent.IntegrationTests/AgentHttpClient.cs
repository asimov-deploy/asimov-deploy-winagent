using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap.Pipeline;

namespace AsimovDeploy.WinAgent.IntegrationTests
{
    public class AgentHttpClient
    {
        private readonly int _port;

        public AgentHttpClient(int port)
        {
            _port = port;
        }

        public T Get<T>(string url)
        {
            var httpClient = new HttpClient();
            
            var result = httpClient.GetAsync(GetFullAgentUrl(url));
            EnsureSuccess(url,result,"GET",null);
            var strTask = result.Result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(strTask.Result);
        }

        public dynamic Get(string url)
        {
            var httpClient = new HttpClient();
            var result = httpClient.GetAsync(GetFullAgentUrl(url));
            EnsureSuccess(url,result,"GET",null);
            var strTask = result.Result.Content.ReadAsStringAsync();
            return JObject.Parse(strTask.Result);
        }

        private string GetFullAgentUrl(string url) => $"http://localhost:{_port}{url}";

        public void Post(string url, string apiKey, object data)
        {
            var httpClient = new HttpClient();
	        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(apiKey);
            var jsonString = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var post = httpClient.PostAsync(GetFullAgentUrl(url), content);
            Task.WaitAll(post);
            EnsureSuccess(url, post, "POST", jsonString);
        }

        private static void EnsureSuccess(string url, Task<HttpResponseMessage> response, string method, string data)
        {
            if (!response.Result.IsSuccessStatusCode)
            {
                Assert.Fail(
                    $"{(int) response.Result.StatusCode} {response.Result.StatusCode}: {method} {url} {data}{Environment.NewLine}Response:{response.Result.Content.ReadAsStringAsync().Result}");
            }
        }
    }
}