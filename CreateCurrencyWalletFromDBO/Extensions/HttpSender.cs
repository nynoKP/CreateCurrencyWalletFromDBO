using CreateCurrencyWalletFromDBO.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Extensions
{
    public class HttpSender
    {
        private static readonly HttpClient client = new HttpClient();
        public static async Task<ResponseFromAPI<T>> RequestByPostAsync<T>(RequestToApi requestToApi)
        {
            Log.ForContext<T>().Debug($"Request : {requestToApi.Request}");
            try
            {
                HttpRequestMessage message = new HttpRequestMessage();
                message.RequestUri = new Uri(requestToApi.Url);

                message.Content = new StringContent(requestToApi.Request, Encoding.UTF8, requestToApi.ContentType);

                if (!string.IsNullOrEmpty(requestToApi.JwtToken))
                {
                    message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", requestToApi.JwtToken);
                }

                message.Method = requestToApi.Method;

                HttpResponseMessage response = await client.SendAsync(message);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                Log.ForContext<T>().Debug($"Response : {responseBody}");

                return new ResponseFromAPI<T>
                {
                    Response = responseBody,
                    IsSuccess = true,
                    RequestData = requestToApi
                };
            }
            catch (Exception ex)
            {
                Log.ForContext<T>().Error($"Request error: ", ex);
                return new ResponseFromAPI<T>
                {
                    IsSuccess = false,
                    Exception = ex,
                    RequestData = requestToApi
                };
            }
        }
    }
}
