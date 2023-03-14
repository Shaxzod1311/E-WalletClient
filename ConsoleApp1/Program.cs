using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp 
{
    class Program
    {

        static async Task Main(string[] args)
        {
            // Replace with your own values
            string baseUrl = "https://localhost:7024/";
            string userId = "026d4cf1-048e-4c09-9f1c-2ad074997843";
            string secretKey = "1KpT1XxtIm+PYLn98xbFtAmrw/ihD7QrBImsA/WFdjU=";


            using var httpClient = new HttpClient();

            
            httpClient.BaseAddress = new Uri(baseUrl);

            string authHeader = "HMAC";
            httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
            httpClient.DefaultRequestHeaders.Add("X-UserId", userId);

            string currentMonth = DateTime.Now.ToString("yyyy-MM");


            var rechargeData = await GetRechargeData(httpClient, secretKey, Guid.Parse("996c36c0-a38d-473e-9816-6320056881f9"));
            //Console.WriteLine($"Total recharge operations: {rechargeData.TotalCount}, Total recharge amount: {rechargeData.TotalAmount}");


            //bool accountExists = await CheckAccountExists(httpClient, secretKey);
            //Console.WriteLine($"Account exists: {accountExists}");


            //await ReplenishAccount(httpClient, secretKey, "example-account-id", 5000);


            //var balance = await GetBalance(httpClient, secretKey, Guid.Parse("2bc99099-55a8-4838-bbab-bd78decf2e7f"));
            //Console.WriteLine($"Balance: {balance}");
        }

        static async Task<bool> CheckAccountExists(HttpClient httpClient, string secretKey)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, "Ewallet/CheckToAccountExists");


            request.Headers.Add("X-Digest", ComputeHmacSha1(secretKey, await request.Content.ReadAsStringAsync()));


            using var response = await httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadFromJsonAsync<bool>();

            return responseJson;
        }

        static async Task ReplenishAccount(HttpClient httpClient, string secretKey, string accountId, decimal amount)
        {
            var url = new Uri("Ewallet/TopUp");
            var request = new HttpRequestMessage(HttpMethod.Post, url);


            var requestBody = new { WalletId = accountId, Amount = amount };
            new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");


            request.Headers.Add("X-Digest", ComputeHmacSha1(secretKey, await request.Content.ReadAsStringAsync()));


            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        static async Task<HttpResponseMessage> GetRechargeData(HttpClient httpClient, string secretKey, Guid id)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, Uri.UnescapeDataString("ewallet​/getRechargeInfo"));


            var requestBody = id ;
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");


            request.Headers.Add("X-Digest", ComputeHmacSha1(secretKey, await request.Content.ReadAsStringAsync()));


            using var response = httpClient.Send(request);

            return response;
        }

        static async Task<decimal> GetBalance(HttpClient httpClient, string secretKey, Guid accountNumber)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, "Ewallet/GetBalance");


            var requestBody = accountNumber;
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");


            request.Headers.Add("X-Digest", ComputeHmacSha1(secretKey, await request.Content.ReadAsStringAsync()));


            using var response = httpClient.Send(request);
            var responseJson = await response.Content.ReadFromJsonAsync<decimal>();

            return responseJson;
        }

        static string ComputeHmacSha1(string key, string data)
        {
            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }
    }
}
