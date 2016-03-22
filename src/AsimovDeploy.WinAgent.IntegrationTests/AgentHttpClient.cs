using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            
            var result = httpClient.GetAsync($"http://localhost:{_port}{url}");
            var strTask = result.Result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(strTask.Result);
        }

        public dynamic Get(string url)
        {
            var httpClient = new HttpClient();
            var result = httpClient.GetAsync(GetFullAgentUrl(url));
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
            var fullUrl = GetFullAgentUrl(url);
            var post = httpClient.PostAsync(fullUrl, content);
            Task.WaitAll(post);
        }
    }
}