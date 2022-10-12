using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ConsoleApp1
{
    public class HMACDelegatingHandler : DelegatingHandler
    {
        // First obtained the APP ID and API Key from the server
        // The APIKey MUST be stored securely in db or in the App.Config
        string ClientId = "099aaecc-fb0e-4b27-bea9-882d320bff1d";
        string SecretKey = "1KpT1XxtIm+PYLn98xbFtAmrw/ihD7QrBImsA/WFdjU=";
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            HttpResponseMessage response = null;
            string requestContentBase64String = string.Empty;

            //Get the Request URI
            string requestUri = HttpUtility.UrlEncode(request.RequestUri.AbsoluteUri.ToLower());

            //Get the Request HTTP Method type
            string requestHttpMethod = request.Method.Method;

            //Calculate UNIX time
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = DateTime.UtcNow - epochStart;

            string requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();
            //Create the random nonce for each request
            string nonce = Guid.NewGuid().ToString("N");

            //Checking if the request contains body, usually will be null wiht HTTP GET and DELETE
            if (request.Content != null)
            {
                requestContentBase64String = ToHmacSha1(request.Content.ToString(), SecretKey) ;
            }

            //Creating the raw signature string by combinging
            //APPId, request Http Method, request Uri, request TimeStamp, nonce, request Content Base64 String
            string signatureRawData = String.Format("{0}{1}{2}{3}{4}{5}", ClientId, requestHttpMethod, requestUri, requestTimeStamp, nonce, requestContentBase64String);

            string hashSunRequest = ToHmacSha1(signatureRawData, SecretKey);

            request.Headers.Add("X-UserId", $"{ClientId}");
            request.Headers.Add("X-Digest", $"{hashSunRequest}");
            request.Headers.Add("nonce", $"{nonce}");
            request.Headers.Add("timeSpan", $"{timeSpan}");


            response = await base.SendAsync(request, cancellationToken);
            return response;
        }

        public string ToHmacSha1(string input, string key)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            byte[] keyEncoded = Convert.FromBase64String(key);

            using (var myhmacsha1 = new HMACSHA1(keyEncoded))
            {
                var hashArray = myhmacsha1.ComputeHash(byteArray);

                return hashArray.Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
            }
        }
    }
}