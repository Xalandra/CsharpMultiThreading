using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Vidarr.Classes
{
    class MaakHttpClientAan
    {
        public HttpClient httpClient;

        public MaakHttpClientAan() {
            //httpClient = maakHttpClient();
            Task.Factory.StartNew(()=> {
                httpClient = maakHttpClient();
            });
        }

        public HttpClient maakHttpClient()
        {
            //Create an HTTP client object
            HttpClient httpClient = new HttpClient();
            //Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;
            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }
            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }
            return httpClient;
        }

        public async Task<string> doeHttpRequestYoutubeMetZoektermEnGeefResults(string zoekterm)
        {
            Uri requestUri = new Uri("https://www.youtube.com/results?search_query=" + zoekterm);
            //Send the GET request asynchronously and retrieve the response as a string.
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";
            try
            {
                //Send the GET request
                //krijgen text/html terug, await = wacht totdat antwoord is
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            return httpResponseBody;
        }

        public async Task<string> doeHttpRequestYoutubeVoorScrawlerEnGeefResults(string url)
        {
            Uri requestUri = new Uri(url);
            //Send the GET request asynchronously and retrieve the response as a string.
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";
            try
            {
                //Send the GET request
                //krijgen text/html terug, await = wacht totdat antwoord is
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            return httpResponseBody;
        }
    }
}
