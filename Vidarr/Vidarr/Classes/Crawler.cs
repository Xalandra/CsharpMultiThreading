using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Vidarr.Classes
{
    class Crawler
    {
        MaakHttpClientAan httpClientRequest;
        List<string> lijstUrls;
        List<string> lijstResponses;
        List<string> lijstResponsesKeywords;

        int aantalGecrawled;

        public Crawler()
        {
            lijstUrls = new List<string>();
            lijstResponses = new List<string>();
            lijstResponsesKeywords = new List<string>();

            Task beginpuntCrawl = new Task(async() => { await crawlBeginpunt(); });
            beginpuntCrawl.Start();

            //gaMaarCrawlen();
        }

        //zoek op userinput
        public async Task<string> crawlZoekterm(string zoekterm)
        {
            httpClientRequest = new MaakHttpClientAan();
            string httpResponseBody = await httpClientRequest.doeHttpRequestYoutubeMetZoektermEnGeefResults(zoekterm); //await = wacht totdat antwoord is

            //haal de body uit de response
            httpResponseBody = regexBody(httpResponseBody);
            httpResponseBody = regexContent(httpResponseBody);
            httpResponseBody = regexResults(httpResponseBody);

            //bestandpicker in downloadsmap
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
            savePicker.FileTypeChoices.Add("Simple Line Files", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "httpResponseBody.txt";
            StorageFile file = await savePicker.PickSaveFileAsync(); //await = wacht totdat antwoord is
                                                                     //schrijf httpResponseBody naar txt bestand
            await FileIO.WriteTextAsync(file, httpResponseBody);

            //return string van httpResponseBody
            return httpResponseBody;
        }

        //zoek zonder input beginpunt
        public async Task crawlBeginpunt()
        {
            //beginpunt
            try
            {
                //crawl beginpunt
                Debug.WriteLine("crawlBeginpunt gets results");
                httpClientRequest = new MaakHttpClientAan();
                string httpResponseBody = "";
                string url = "https://www.youtube.com/";
                Task probeer = new Task(async() => {
                    httpResponseBody = await httpClientRequest.doeHttpRequestYoutubeVoorScrawlerEnGeefResults(url);

                    //haal de body uit de response
                    httpResponseBody = regexBody(httpResponseBody);
                    httpResponseBody = regexContent(httpResponseBody);
                    httpResponseBody = regexResults(httpResponseBody);
                });
                probeer.Start();
                await probeer;
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine("crawlBeginpunt() geeft NullReferenceException: " + e.Message);
            }
        }


        public async void gaMaarCrawlen()
        {
            bool finished = false;

            aantalGecrawled = 0;

            //zolang true crawl maar door
            while (!finished)
            {
                //als beginpunt responses heeft gevonden
                if (lijstResponses.Count > 1)
                {
                    //voor testen max 50 rondes
                    if (aantalGecrawled < 50)
                    {
                        //pakt responsebody uit LijstResponses, urls komen in LijstUrls
                        await Task.Factory.StartNew(() =>
                        {
                            Debug.WriteLine("Task gets urls uit body uit lijstResponses");
                            try
                            {
                                getUrls(pakUitQueue("responses"));
                            }
                            catch (NullReferenceException e)
                            {
                                Debug.WriteLine("getUrls() in while geeft NullReferenceException: " + e.Message);
                            }
                        });


                        //pakt url van LijstUrls, Responsebody komt in LijstResponses
                        await Task.Factory.StartNew(async () =>
                        {
                            Debug.WriteLine("Task gets body uit url uit lijstUrls");
                            try
                            {
                                await getResponseBody(pakUitQueue("urls"));
                            }
                            catch (NullReferenceException e)
                            {
                                Debug.WriteLine("getResponseBody() in while geeft NullReferenceException: " + e.Message);
                            }
                        });


                        //pakt maar verwijdert niet responsebody uit lijstresponses, keywords komen in database
                        await Task.Factory.StartNew(() =>
                        {
                            Debug.WriteLine("Task gets keywords uit body uit lijstResponses");
                            try
                            {
                                getKeywords(pakUitQueue("keywords"));
                            }
                            catch (NullReferenceException e)
                            {
                                Debug.WriteLine("getKeywords() in while geeft NullReferenceException: " + e.Message);
                            }
                        });

                        aantalGecrawled++;

                    }
                    else
                    {
                        //stop met crawlen omdat aantal meer is dan 50
                        finished = true;

                        Debug.WriteLine("aantalGecrawled max bereikt");
                    }
                }
                else
                {
                    Debug.WriteLine("beginpunt heeft nog geen responses gevonden..");
                }

            }
        }


        //haal body uit httpResponseBody
        public string regexBody(string response)
        {
            //haal body uit string
            string body = "";
            string patternBody = @"<body\s(.*?)</body>";

            Match match = Regex.Match(response, patternBody, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                body = match.Value;
                Debug.WriteLine(body);
            }
            else
            {
                Debug.WriteLine("Geen body kunnen vinden.");
            }
            return body;
        }

        //haal content uit httpResponseBody
        public string regexContent(string response)
        {
            //haal body uit string
            string body = "";
            string patternBody = "id=[\"']content[\"'](.*?)footer";

            Match match = Regex.Match(response, patternBody, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                body = match.Value;
                Debug.WriteLine(body);
            }
            else
            {
                Debug.WriteLine("Geen content kunnen vinden.");
            }
            return body;
        }

        //haal results uit httpResponseBody
        public string regexResults(string response)
        {
            //haal body uit string
            string body = "";
            string patternBody = "id=[\"']results[\"'](.*?)class=[\"']branded-page-box\\s*";

            Match match = Regex.Match(response, patternBody, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                body = match.Value;
                Debug.WriteLine(body);
            }
            else
            {
                Debug.WriteLine("Geen results kunnen vinden.");
            }
            return body;
        }

        //haal urls uit content
        public void regexUrls(string response)
        {
            //haal urls uit body
            string url = "";
            string patternUrls = "href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))"; //beste regex

            //dan urls
            MatchCollection collection = Regex.Matches(response, patternUrls);
            foreach (Match m in collection)
            {
                //check of geldige url is
                if (m.Success)
                {
                    url = m.Value;
                    bool isUri = Uri.IsWellFormedUriString(url, UriKind.Absolute);
                    //Debug.WriteLine("Gevonden url: " + url);
                    //Debug.WriteLine("bool: " + isUri);
                    if (isUri)
                    {
                        lijstUrls.Add(url);
                    }
                }
            }
        }

        //haal keywords uit content
        public void regexKeywords(string response)
        {
            //haal keywords uit body
            string keywords = "";
            string pattern = "title=\"(.*?)</a>";
            MatchCollection collection = Regex.Matches(response, pattern);
            foreach (Match m in collection)
            {
                //spuug uit van je gevonden hebt
                keywords = m.Value;
                lijstResponsesKeywords.Add(keywords);
                Debug.WriteLine("Gevonden keywords: " + keywords);
            }
        }












        public async Task<string> getResponseBody(string url)
        {
            string body = "";

            try
            {
                //getResponseBody url
                httpClientRequest = new MaakHttpClientAan();

                //welke url crawlen
                Debug.WriteLine("url in getResponseBody() = " + url);

                string antwoord = await httpClientRequest.doeHttpRequestYoutubeVoorScrawlerEnGeefResults(url); //await = wacht totdat antwoord is
                Debug.WriteLine(antwoord);

                //haal body uit string
                body = regexBody(antwoord);
                
                lijstResponses.Add(body);
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine("getResponseBody() geeft NullReferenceException: " + e.Message);
            }

            return body;

        }

        public void getUrls(string httpResponseBody)
        {
            try
            {
                regexUrls(httpResponseBody);
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine("getUrls() geeft NullReferenceException: " + e.Message);
            }
        }

        public void getKeywords(string httpResponseBody)
        {
            try
            {
                regexKeywords(httpResponseBody);
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine("getKeywords() geeft NullReferenceException: " + e.Message);
            }
        }

        public string pakUitQueue(string lijst)
        {
            string res = "";
            if (lijst == "responses")
            {
                if (lijstResponses.Count != 0)
                {
                    string x = lijstResponses[0];
                    lijstResponses.Remove(lijstResponses[0]);
                    res = x;
                }
            }
            if (lijst == "urls")
            {
                if (lijstUrls.Count != 0)
                {
                    string x = lijstUrls[0];
                    lijstUrls.Remove(lijstUrls[0]);
                    res = x;
                }
                else
                {
                    return "https://www.google.com";
                }
            }
            if (lijst == "keywords")
            {
                if (lijstResponsesKeywords.Count != 0)
                {
                    string x = lijstResponsesKeywords[0];
                    lijstResponsesKeywords.Remove(lijstResponsesKeywords[0]);
                    res = x;
                }
            }

            /*
            try
            {
                for (int i = 0; lijstUrls.Count > i; i++)
                {
                    Debug.WriteLine("Lijst[" + i + "] = " + lijstUrls[i]);
                }
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine("getKeywords() geeft NullReferenceException: " + e.Message);
            }
            */

            return res;


        }
    }
}
