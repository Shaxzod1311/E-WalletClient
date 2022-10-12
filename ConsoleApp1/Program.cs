using ConsoleApp1;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ConsoleApp 
{
    class Program
    {
        static void Main(string[] args)
        {

            string ClientId = "099aaecc-fb0e-4b27-bea9-882d320bff1d";
            string SecretKey = "1KpT1XxtIm+PYLn98xbFtAmrw/ihD7QrBImsA/WFdjU=";


        }

        static async Task BalanceOfWalletAsyncAsync(Guid walletId)
        {
            Console.WriteLine("Calling the back-end API");
            //Need to change the port number
            //provide the port number where your api is running
            string apiBaseAddress = "http://localhost:5001/";
            HMACDelegatingHandler customDelegatingHandler = new HMACDelegatingHandler();

            HttpClient client = HttpClientFactory.Create(customDelegatingHandler);

            HttpResponseMessage response = await client.PostAsJsonAsync(apiBaseAddress + "api/orders", walletId);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
                Console.WriteLine("HTTP Status: {0}, Reason {1}. Press ENTER to exit", response.StatusCode, response.ReasonPhrase);
            }
            else
            {
                Console.WriteLine("Failed to call the API. HTTP Status: {0}, Reason {1}", response.StatusCode, response.ReasonPhrase);
            }
        }
    }
}
