using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.Threading;

namespace Vidarr.Classes
{
    class Crawler
    {
        MaakHttpClientAan httpClientRequest;
        List<string> lijstUrls;
        List<string> lijstResponses;
        List<string> lijstResponsesKeywords;
        Object locker;

        int aantalGecrawled;

        public Crawler()
        {
            lijstUrls = new List<string>();
            lijstResponses = new List<string>();
            lijstResponsesKeywords = new List<string>();
            locker = new Object();

            Task beginpuntCrawl = new Task(async() => {
                await crawlBeginpunt();

                //gaMaarCrawlen();
            });
            beginpuntCrawl.Start();
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

            /*lock (this.locker) {
                //bodyresponse krijgt voorrang in de lijst
                lijstResponses.Insert(0, httpResponseBody);
            }*/
            
            Task crawlZoekterm = new Task(async() => {
                //haal uit results urls
                int maxUrls = 10;
                int gevondenUrls = 0;
                List<String> tijdelijkeUrls = new List<string>();

                //haal urls uit body
                string url = "";
                string patternUrls = "href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))"; //beste regex

                //dan urls
                MatchCollection collection = Regex.Matches(httpResponseBody, patternUrls);
                foreach (Match m in collection)
                {
                    //check of er al 10 urls zijn gevonden
                    if (gevondenUrls < maxUrls)
                    {

                        //check of geldige url is
                        if (m.Success)
                        {
                            url = m.Value;

                            //haal href=" er af
                            url = url.Remove(0, 6);
                            url = url.Remove(url.Length - 1);
                            if (url.Contains("/watch?"))
                            {
                                url = "https://www.youtube.com" + url;
                            }
                            if (url.Contains("www.youtube.com") && !url.Contains("channel") && !url.Contains("user") && !url.Contains("playlist") && !url.Contains("feed") && !url.Contains("bit.ly") && !url.Contains("accounts.google"))
                            {
                                //Debug.WriteLine("Gevonden url: " + url);
                                bool isUri = Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
                                //Debug.WriteLine("bool: " + isUri);
                                //correcte uri + nog geen 10 urls toegevoegd
                                if (isUri && (gevondenUrls < maxUrls))
                                {
                                    tijdelijkeUrls.Add(url);
                                    
                                    gevondenUrls++;
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }



                //ga over 10 gevonden urls
                foreach (String tu in tijdelijkeUrls)
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
                        body = regexBody(antwoord);
                        body = regexContent(body);

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
                } //10 gehad





                //show resultaten uit database
            });
            crawlZoekterm.Start();
            
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

                    lock (this.locker)
                    {
                        lijstResponses.Add(httpResponseBody);
                    }

                    //Debug.WriteLine("Aantal in lijstResponses na beginpuntcrawl: " + lijstResponses.Count);
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
                if (lijstResponses.Count > 0)
                {
                    //voor testen max 50 rondes
                    if (aantalGecrawled < 5000)
                    {
                        //pakt responsebody uit LijstResponses, urls komen in LijstUrls
                        Task bodys = await Task.Factory.StartNew(async() =>
                        {
                            //Debug.WriteLine("Task gets urls uit body uit lijstResponses");
                            try
                            {
                                getUrls(pakUitQueue("responses"));
                            }
                            catch (NullReferenceException e)
                            {
                                Debug.WriteLine("getUrls() in while geeft NullReferenceException: " + e.Message);
                            }

                            //wacht even met verder gaan
                            await Task.Delay(1000);

                            aantalGecrawled++;
                        });
                        //get meer bodys
                        for (int i = 0; i < 10; i++)
                        {
                            //pakt url van LijstUrls, Responsebody komt in LijstResponses
                            Task urls = await Task.Factory.StartNew(async () =>
                            {
                                //Debug.WriteLine("Task gets body uit url uit lijstUrls");
                                try
                                {
                                    await getResponseBody(pakUitQueue("urls"));
                                }
                                catch (NullReferenceException e)
                                {
                                    Debug.WriteLine("getResponseBody() in while geeft NullReferenceException: " + e.Message);
                                }

                                //wacht even met verder gaan
                                await Task.Delay(1000);

                                aantalGecrawled++;
                            });
                        }
                        //pakt maar verwijdert niet responsebody uit lijstresponses, keywords komen in database
                        Task keys = await Task.Factory.StartNew(async() =>
                        {
                            //Debug.WriteLine("Task gets keywords uit body uit lijstResponses");
                            try
                            {
                                getKeywords(pakUitQueue("keywords"));
                            }
                            catch (NullReferenceException e)
                            {
                                Debug.WriteLine("getKeywords() in while geeft NullReferenceException: " + e.Message);
                            }

                            //wacht even met verder gaan
                            await Task.Delay(1000);

                            aantalGecrawled++;
                        });

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
                    //Debug.WriteLine("beginpunt heeft nog geen responses gevonden..");

                    try
                    {
                        lock (this.locker)
                        {
                            for (int i = 0; lijstUrls.Count > i; i++)
                            {
                                Debug.WriteLine("Lijst[" + i + "] = " + lijstUrls[i]);
                            }
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.WriteLine("getKeywords() geeft NullReferenceException: " + e.Message);
                    }
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
                //Debug.WriteLine(body);
            }
            else
            {
                //Debug.WriteLine("Geen body kunnen vinden.");
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
                //Debug.WriteLine(body);
            }
            else
            {
                //Debug.WriteLine("Geen content kunnen vinden.");
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
                //Debug.WriteLine(body);
            }
            else
            {
                //Debug.WriteLine("Geen results kunnen vinden.");
            }
            return body;
        }

        //haal urls uit content
        public void regexUrls(string response)
        {
            int maxUrls = 10;
            int gevondenUrls = 0;
            
            //haal urls uit body
            string url = "";
            string patternUrls = "href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))"; //beste regex

            //dan urls
            MatchCollection collection = Regex.Matches(response, patternUrls);
            foreach (Match m in collection)
            {
                //check of er al 10 urls zijn gevonden
                if (gevondenUrls < maxUrls)
                {

                    //check of geldige url is
                    if (m.Success)
                    {
                        url = m.Value;

                        //haal href=" er af
                        url = url.Remove(0, 6);
                        url = url.Remove(url.Length - 1);
                        if (url.Contains("/watch?"))
                        {
                            url = "https://www.youtube.com" + url;
                        }
                        if (url.Contains("www.youtube.com") && !url.Contains("channel") && !url.Contains("user") && !url.Contains("playlist") && !url.Contains("feed") && !url.Contains("bit.ly") && !url.Contains("accounts.google"))
                        {
                            //Debug.WriteLine("Gevonden url: " + url);
                            bool isUri = Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
                            //Debug.WriteLine("bool: " + isUri);
                            //correcte uri + nog geen 10 urls toegevoegd
                            if (isUri && (gevondenUrls < maxUrls))
                            {
                                lock (this.locker)
                                {
                                    lijstUrls.Add(url);
                                }
                                gevondenUrls++;
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        //haal keywords uit content
        public void regexKeywords(string response)
        {
            //haal keywords uit body
            string keywords = "";
            string pattern = "title=\"(.*?)</a>";
            MatchCollection collection;
            try
            {
                collection = Regex.Matches(response, pattern);

                foreach (Match m in collection)
                {
                    //spuug uit van je gevonden hebt
                    keywords = m.Value;
                    //keywords in database!!!!!!!!!
                    Debug.WriteLine("Gevonden keywords: " + keywords);
                }
            }
            catch (NullReferenceException e) {
                Debug.WriteLine("getKeywords() geeft NullReferenceException: " + e.Message);
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
                //Debug.WriteLine("url in getResponseBody() = " + url);

                string antwoord = await httpClientRequest.doeHttpRequestYoutubeVoorScrawlerEnGeefResults(url); //await = wacht totdat antwoord is
                //Debug.WriteLine(antwoord);

                //haal body uit string
                body = regexBody(antwoord);
                body = regexContent(body);

                lock (this.locker)
                {
                    lijstResponses.Add(body);
                    lijstResponsesKeywords.Add(body);
                }
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
                lock (this.locker)
                {
                    if (lijstResponses.Count != 0)
                    {
                        string x = lijstResponses[0];
                        lijstResponses.Remove(lijstResponses[0]);
                        res = x;
                    }
                }
            }
            if (lijst == "urls")
            {
                lock (this.locker)
                {
                    if (lijstUrls.Count != 0)
                    {
                        string x = lijstUrls[0];
                        lijstUrls.Remove(lijstUrls[0]);
                        res = x;
                    }
                    else
                    {
                        return "https://www.youtube.com";
                    }
                }
            }
            if (lijst == "keywords")
            {
                lock (this.locker)
                {
                    if (lijstResponsesKeywords.Count != 0)
                    {
                        string x = lijstResponsesKeywords[0];
                        lijstResponsesKeywords.Remove(lijstResponsesKeywords[0]);
                        res = x;
                    }
                }
            }

            return res;


        }

        public void outputLists()
        {
            try
            {
                lock (this.locker)
                {
                    Debug.WriteLine("LijstUrls = " + lijstUrls.Count);
                }
                lock (this.locker)
                {
                    Debug.WriteLine("LijstBodys = " + lijstResponses.Count);
                }
                lock (this.locker)
                {
                    Debug.WriteLine("LijstKeywords = " + lijstResponsesKeywords.Count);
                }
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine("getKeywords() geeft NullReferenceException: " + e.Message);
            }
        }
    }
}
