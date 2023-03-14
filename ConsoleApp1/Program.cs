using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            string userId = "2ba5597b-5a8b-45c3-a838-c886a4873acf";
            string secretKey = "1KpT1XxtIm+PYLn98xbFtAmrw/ihD7QrBImsA/WFdjU=";


            using var httpClient = new HttpClient();


            httpClient.BaseAddress = new Uri(baseUrl);

            string authHeader = "HMAC";
            httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
            httpClient.DefaultRequestHeaders.Add("X-UserId", userId);

            string currentMonth = DateTime.Now.ToString("yyyy-MM");


            var response = await GetRechargeData(httpClient, secretKey, "9d887904-794d-4535-924c-fe44696bcaec");

            if (response.Data != null)
            {
                foreach (var transaction in response.Data)
                {
                    Console.WriteLine($"Total recharge operations: {transaction.WalletId}, Total recharge amount: {transaction.Amount}");
                }
            }
            else
            {
                Console.WriteLine($"Error: {response.Error.Message} | Code: {response.Error.Code}");
            }



            var response1 = await CheckAccountExists(httpClient, secretKey, userId);

            if (response1.Data != null)
            {
                Console.WriteLine($"Account exists: {response1.Data.Id}");

            }
            else
            {
                Console.WriteLine($"Error: {response1.Error.Message} | Code: {response1.Error.Code}");
            }


            var response2 = await ReplenishAccount(httpClient, secretKey, "9d887904-794d-4535-924c-fe44696bcaec", 5000);

            if (response2.Data != null)
            {
                Console.WriteLine($"WalletId = {response2.Data}");

            }
            else
            {
                Console.WriteLine($"Error: {response2.Error.Message} | Code: {response2.Error.Code}");
            }


            var response3 = await GetBalance(httpClient, secretKey, Guid.Parse("9d887904-794d-4535-924c-fe44696bcaec"));
            
            if (response3.Data != null)
            {
                Console.WriteLine($"Balance: {response3.Data.Balance}");

            }
            else
            {
                Console.WriteLine($"Error: {response3.Error.Message} | Code: {response3.Error.Code}");
            }
        }


        static async Task<BaseResponse<WalletDTO>> CheckAccountExists(HttpClient httpClient, string secretKey, string userId)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, "Ewallet/CheckToAccountExists");

            var requestBody = userId;
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            request.Headers.Add("X-Digest", ComputeHmacSha1(secretKey, await request.Content.ReadAsStringAsync()));


            using var response = await httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadFromJsonAsync<BaseResponse<WalletDTO>>();

            return responseJson;
        }


        static async Task<BaseResponse<Guid>> ReplenishAccount(HttpClient httpClient, string secretKey, string accountId, decimal amount)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, "Ewallet/TopUp");


            var requestBody = new { WalletId = accountId, Amount = amount };
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");


            request.Headers.Add("X-Digest", ComputeHmacSha1(secretKey, await request.Content.ReadAsStringAsync()));


            using var response = httpClient.Send(request);

            var responseMessage = await response.Content.ReadFromJsonAsync<BaseResponse<Guid>>();
            return responseMessage;
        }


        static async Task<BaseResponse<List<TransactionDTO>>> GetRechargeData(HttpClient httpClient, string secretKey, string id)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, "Ewallet/GetRechargeInfo");


            var requestBody = id ;
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");


            request.Headers.Add("X-Digest", ComputeHmacSha1(secretKey, await request.Content.ReadAsStringAsync()));


            using var response = httpClient.Send(request);
            var responeMessage = await response.Content.ReadFromJsonAsync<BaseResponse<List<TransactionDTO>>>();

            return responeMessage;
        }


        static async Task<BaseResponse<WalletDTO>> GetBalance(HttpClient httpClient, string secretKey, Guid accountNumber)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, "Ewallet/GetBalance");


            var requestBody = accountNumber;
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");


            request.Headers.Add("X-Digest", ComputeHmacSha1(secretKey, await request.Content.ReadAsStringAsync()));


            using var response = httpClient.Send(request);
            var responseJson = await response.Content.ReadFromJsonAsync<BaseResponse<WalletDTO>>();

            return responseJson;
        }


        static string ComputeHmacSha1(string key, string data)
        {
            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }
    }

    #region DTOs
    public class WalletDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
    }


    public class TransactionDTO
    {
        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public class BaseResponse<TSource>
    {
        public int? Code { get; set; } = 200;
        public TSource? Data { get; set; }
        public ErrorResponse? Error { get; set; }

    }

    public class ErrorResponse
    {

        public int? Code { get; set; }
        public string? Message { get; set; }

    }
    #endregion
}
