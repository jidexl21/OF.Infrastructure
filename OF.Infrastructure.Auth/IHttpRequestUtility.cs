using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OF.Infrastructure.Auth
{
    public interface IHttpRequestUtility
    {
        Task<T> GetAsync<T>(string url, bool Authenticate);
        Task<Tout> PostAsync<Tin, Tout>(string url, Tin item, bool Authenticate);
        Task<bool> Authenticate();
        Task<bool> Authenticate(string account, string password);
        Task<(bool, string)> Authenticate(HttpClient httpClient);
        void AddAuthenticatedToken(HttpClient httpClient, string token);
        bool IsAuthenticated();
    }
}
