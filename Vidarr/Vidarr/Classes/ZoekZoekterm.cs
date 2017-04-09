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

            /* BELANGRIJKE CODE OM OP TE SLAAN IN TXT BESTAND
             * ************************************************** *
            //bestandpicker in downloadsmap
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
            savePicker.FileTypeChoices.Add("Simple Line Files", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "httpResponseBody.txt";
            StorageFile file = await savePicker.PickSaveFileAsync();
            //schrijf httpResponseBody naar txt bestand
            await FileIO.WriteTextAsync(file, httpResponseBody);*/



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
                    //await Task.Delay(1000);
                    //Debug.WriteLine(antwoord);

                    //haal content uit string
                    body = CrawlerRegex.regexContent(antwoord);
                    //await Task.Delay(1000);

                    //haal keywords uit body
                    CrawlerRegex.regexKeywords(body);

                } //gevonden urls gedaan
            });

            //return string van httpResponseBody
            return httpResponseBody;
        }
    }
}
