using CreateCurrencyWalletFromDBO.Extensions.AnorHubModels.NciAccountCreate;
using CreateCurrencyWalletFromDBO.Extensions.AnorHubModels.NciAccountGet;
using CreateCurrencyWalletFromDBO.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Extensions
{
    public class AnorHubService
    {
        private AnorHubService()
        {
            configuration = ConfigurationManager.Configuration;
        }

        private static AnorHubService instance = null;
        private static readonly HttpClient client = new HttpClient();
        private IConfigurationRoot configuration = null;
        public static AnorHubService Instance
        {
            get
            {
                instance ??= new AnorHubService();
                return instance;
            }
        }

        private DateTime? AccessTokenExpiryDate = null;
        private string? AccessToken = null;

        private async Task<string> GetToken()
        {
            var now = DateTime.Now;
            if (AccessTokenExpiryDate == null) return await RequestToToken();
            if (now.AddMinutes(-5) < AccessTokenExpiryDate)
            {
                return await RequestToToken();
            }
            else return AccessToken;
        }

        private async Task<string> RequestToToken()
        {
            var anorHubconf = configuration.GetSection("AnorHub");
            var url = string.Format("{0}:{1}{2}", anorHubconf["Host"], anorHubconf["Port"], "/auth/login");
            var request = new
            {
                username = anorHubconf["Username"],
                password = anorHubconf["Password"]
            };

            var result = await HttpSender.RequestByPostAsync<object>(new RequestToApi
            {
                Url = url,
                Request = JsonConvert.SerializeObject(request)
            });
            var response = JObject.Parse(result.Response);
            AccessToken = response["access_token"].ToString();
            AccessTokenExpiryDate = DateTime.Now.AddSeconds(Convert.ToUInt32(response["expires_in"].ToString()));
            return AccessToken;
        }
        //public async Task<string> RequestByPostAsync(string jsonContent, string url, string bearerToken = null)
        //{
        //    LoggerManager.LogInfo("Request to ANORHUB: " + jsonContent);
        //    try
        //    {
        //        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        //        if (!string.IsNullOrEmpty(bearerToken))
        //        {
        //            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        //        }

        //        HttpResponseMessage response = await client.PostAsync(url, content);
        //        response.EnsureSuccessStatusCode();
        //        string responseBody = await response.Content.ReadAsStringAsync();
        //        LoggerManager.LogInfo("Response from ANORHUB: " + responseBody);
        //        return responseBody;
        //    }
        //    catch (HttpRequestException e)
        //    {
        //        LoggerManager.LogError($"Request to ANORHUB error: {e.Message}", e);
        //        return null;
        //    }
        //}

        public async Task<ResponseFromAPI<NciAccountGetResponseDTO>> NciAccountGet(NciAccountGetRequestDTO model)
        {
            var anorHubconf = configuration.GetSection("AnorHub");
            var url = string.Format("{0}:{1}{2}", anorHubconf["Host"], anorHubconf["Port"], "/services/anorhubms/api/anor-hub");
            return await HttpSender.RequestByPostAsync<NciAccountGetResponseDTO>(new RequestToApi
            {
                Url = url,
                JwtToken = await GetToken(),
                Request = model.ToJson(),
            });
        }
        public async Task<ResponseFromAPI<NciAccountCreateResponseDTO>> NciAccountCreate(NciAccountCreateRequestDTO model)
        {
            var anorHubconf = configuration.GetSection("AnorHub");
            var url = string.Format("{0}:{1}{2}", anorHubconf["Host"], anorHubconf["Port"], "/services/anorhubms/api/anor-hub");
            return await HttpSender.RequestByPostAsync<NciAccountCreateResponseDTO>(new RequestToApi
            {
                Url = url,
                JwtToken = await GetToken(),
                Request = model.ToJson(),
            });
        }
    }
}
