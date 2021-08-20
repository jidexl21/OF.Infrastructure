using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OF.Infrastructure.Auth
{
    public class HttpRequestUtility : IHttpRequestUtility
    {
        private HttpClient ActiveClient;
        private readonly Uri baseUri;
        private Dictionary<string, string> loggedIn;
        private readonly string AppID;

        public HttpRequestUtility(IAuthSettings settings)
        {
            baseUri = new Uri(settings.AuthAppURL);
            AppID = settings.AppID; ;
        }
        public async Task<(bool, string)> Authenticate(string username, string pass, HttpClient CurrClient)
        {

            var obj = new { login = username, passwordHash = pass };
            var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                var res = await client.PostAsync($"{baseUri}api/Login/{AppID}", content);
                if (res.StatusCode != HttpStatusCode.OK) { return (false, string.Empty); };
                string str = await res.Content.ReadAsStringAsync();
                loggedIn = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
            }
            CurrClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", loggedIn["token"]);
            return (true, loggedIn["token"]);
        }

        public async Task<bool> Authenticate(string account, string password)
        {
            bool Result = false;
            if (ActiveClient == null) { ActiveClient = new HttpClient() { BaseAddress = baseUri }; };
            using (var client = new HttpClient() { BaseAddress = baseUri })
            {
                var result = await Authenticate(account, password, ActiveClient);
                Result = result.Item1;
            };
            return Result;
        }

        public async Task<bool> Authenticate()
        {
            bool Result = false;
            if (ActiveClient == null) { ActiveClient = new HttpClient() { BaseAddress = baseUri }; };
            using (var client = new HttpClient() { BaseAddress = baseUri })
            {
                var result = await Authenticate("000000", "123456#", ActiveClient);
                Result = result.Item1;
            };
            return Result;
        }
        public async Task<(bool, string)> Authenticate(HttpClient CurrClient)
        {
            var Result = (false, "");
            //if (ActiveClient == null) { ActiveClient = new HttpClient() { BaseAddress = baseUri }; };
            using (var client = new HttpClient() { BaseAddress = baseUri })
            {
                var result = await Authenticate("000000", "123456#", CurrClient);
                Result = result;
            };
            return Result;
        }

        public async Task<T> GetAsync<T>(string url, bool authenticate = false)
        {
            T result;
            using (var client = new HttpClient() { BaseAddress = baseUri })
            {
                ActiveClient = client;
                if (authenticate) await Authenticate();
                var data = await client.GetAsync(url);
                var status = (int)data.StatusCode;
                var resultStr = await data.Content.ReadAsStringAsync();
                if (!((200 >= status) && (status < 300))) { throw new HttpRequestException(data.StatusCode.ToString(), new Exception(resultStr)); }
                result = JsonConvert.DeserializeObject<T>(resultStr);
            }
            ActiveClient = null;
            return result;
        }

        public async Task<Tout> PostAsync<Tin, Tout>(string url, Tin DataObject, bool authenticate = false)
        {
            Tout result;
            var content = new StringContent(JsonConvert.SerializeObject(DataObject), Encoding.UTF8, "application/json");
            using (var client = new HttpClient() { BaseAddress = baseUri })
            {
                ActiveClient = client;
                if (authenticate) await Authenticate();

                var data = await client.PostAsync(url, content);
                var status = (int)data.StatusCode;
                var resultStr = await data.Content.ReadAsStringAsync();
                if (!((200 >= status) && (status < 300))) { throw new HttpRequestException(data.StatusCode.ToString(), new Exception(resultStr)); }
                result = JsonConvert.DeserializeObject<Tout>(resultStr);
                ActiveClient = null;
            }
            return result;
        }

        /// <summary>
        /// Checks if an authorization header has been adde to the active client
        /// </summary>
        /// <returns></returns>
        public bool IsAuthenticated()
        {
            if (ActiveClient == null) return false;
            return ActiveClient.DefaultRequestHeaders.Contains("Authorization");
        }

        public void AddAuthenticatedToken(HttpClient httpClient, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", token);
        }

     }

}
