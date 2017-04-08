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
            string httpResponseBody = await httpClientRequest.doeHttpRequestYoutubeMetZoektermEnGeefResults(zoekterm); //await = wacht totdat antwoord is

            //haal de body uit de response
            httpResponseBody = CrawlerRegex.regexBody(httpResponseBody);
            httpResponseBody = CrawlerRegex.regexContent(httpResponseBody);
            httpResponseBody = CrawlerRegex.regexResults(httpResponseBody);

            //bestandpicker in downloadsmap
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
            savePicker.FileTypeChoices.Add("Simple Line Files", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "httpResponseBody.txt";
            StorageFile file = await savePicker.PickSaveFileAsync(); //await = wacht totdat antwoord is
                                                                     //schrijf httpResponseBody naar txt bestand
            await FileIO.WriteTextAsync(file, httpResponseBody);

            Task crawlZoekterm = new Task(async () => {
                //haal uit results urls
                string[] urls = CrawlerRegex.regexUrls(httpResponseBody);


                //ga over de gevonden urls
                foreach (String tu in urls)
                {
                    //haal uit urls bodys
                    string body = "";

                    try
                    {
                        //getResponseBody url
                        httpClientRequest = new MaakHttpClientAan();

                        //welke url crawlen
                        //Debug.WriteLine("url in getResponseBody() = " + url);

                        string antwoord = await httpClientRequest.doeHttpRequestYoutubeVoorScrawlerEnGeefResults(tu); //await = wacht totdat antwoord is
                        //Debug.WriteLine(antwoord);

                        //haal body uit string
                        body = CrawlerRegex.regexBody(antwoord);
                        body = CrawlerRegex.regexContent(body);
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.WriteLine("getResponseBody() geeft NullReferenceException: " + e.Message);
                    }




                    //haal uit bodys keys
                    //haal keywords uit body
                    string keywords = "";
                    string pattern = "title=\"(.*?)</a>";
                    MatchCollection collection2;
                    try
                    {
                        collection2 = Regex.Matches(body, pattern);

                        foreach (Match m in collection2)
                        {
                            //spuug uit van je gevonden hebt
                            keywords = m.Value;
                            //keywords in database!!!!!!!!!
                            Debug.WriteLine("Gevonden keywords: " + keywords);
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.WriteLine("getKeywords() geeft NullReferenceException: " + e.Message);
                    }
                } //gevonden urls gedaan





                //show resultaten uit database
            });
            crawlZoekterm.Start();

            //return string van httpResponseBody
            return httpResponseBody;
        }
    }
}
