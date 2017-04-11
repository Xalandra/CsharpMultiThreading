using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Web.Http;

namespace Vidarr.Classes
{
    static class ZoekZoekterm
    {

        //zoek op userinput
        static public async Task<string> crawlZoekterm(string zoekterm)
        {
            MaakHttpClientAan httpClientRequest = new MaakHttpClientAan();
            string httpResponseBody = await httpClientRequest.doeHttpRequestYoutubeMetZoektermEnGeefResults(zoekterm);

            //haal de results uit de response
            httpResponseBody = CrawlerRegex.regexResults(httpResponseBody);
            
            await Task.Factory.StartNew(async() =>
            {
                //haal uit results urls
                List<string> urls = CrawlerRegex.regexUrls(httpResponseBody);

                //ga over de gevonden urls
                foreach (String url in urls)
                {
                    //haal uit urls bodys
                    string body = "";
                    string antwoord = "";

                    //getResponseBody url
                    httpClientRequest = new MaakHttpClientAan();
                    await Task.Delay(1000);

                    //welke url crawlen
                    //Debug.WriteLine("url in getResponseBody() = " + url);
                    antwoord = await httpClientRequest.doeHttpRequestYoutubeVoorScrawlerEnGeefResults(url);

                    //haal content uit string
                    body = CrawlerRegex.regexContent(antwoord);

                    //haal keywords uit body
                    CrawlerRegex.regexKeywords(body);

                } //gevonden urls gedaan
            });

            //return string van httpResponseBody
            return httpResponseBody;
        }
    }
}
