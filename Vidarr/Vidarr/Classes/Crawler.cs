﻿using System;
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
        bool beginGelukt = false;

        int aantalGecrawled;

        public Crawler()
        {
            lijstUrls = new List<string>();
            lijstResponses = new List<string>();
            lijstResponsesKeywords = new List<string>();
            locker = new Object();

            Task beginpuntCrawl = new Task(() => 
            {
                startCrawlen();
            });
            beginpuntCrawl.Start();
        }

        //start crawlen
        public async void startCrawlen() {
            beginGelukt = await crawlBeginpunt();

            gaMaarCrawlen();
        }

        //zoek zonder input van user beginpunt
        public async Task<bool> crawlBeginpunt()
        {
            bool gelukt = false;
            //beginpunt
            try
            {
                //crawl beginpunt
                Debug.WriteLine("crawlBeginpunt gets results");
                httpClientRequest = new MaakHttpClientAan();
                string httpResponseBody = "";
                string url = "https://www.youtube.com/";
                httpResponseBody = await httpClientRequest.doeHttpRequestYoutubeVoorScrawlerEnGeefResults(url);

                //haal de body uit de response
                //httpResponseBody = CrawlerRegex.regexBody(httpResponseBody);
                //httpResponseBody = CrawlerRegex.regexContent(httpResponseBody);
                httpResponseBody = regexContent(httpResponseBody);
                //Debug.WriteLine(httpResponseBody);

                lock (this.locker)
                {
                    lijstResponses.Add(httpResponseBody);
                    gelukt = true;
                }
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine("crawlBeginpunt() geeft NullReferenceException: " + e.Message);
            }

            return gelukt;
        }


        public async void gaMaarCrawlen()
        {
            bool finished = false;

            aantalGecrawled = 0;

            //zolang true crawl maar door
            while (!finished)
            {
                
                //voor testen max 50 rondes
                if (aantalGecrawled < 10)
                {
                    //pakt responsebody uit LijstResponses, urls komen in LijstUrls
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


                    //get meer bodys
                    for (int i = 0; i < 10; i++)
                    {
                        bool zegtNiksMaarWachtOpStringAntwoord = false;

                        //pakt url van LijstUrls, Responsebody komt in LijstResponses
                        //Debug.WriteLine("Task gets body uit url uit lijstUrls");
                        try
                        {
                            zegtNiksMaarWachtOpStringAntwoord = await getResponseBody(pakUitQueue("urls"));
                        }
                        catch (NullReferenceException e)
                        {
                            Debug.WriteLine("getResponseBody() in while geeft NullReferenceException: " + e.Message);
                        }
                        //wacht even met verder gaan
                        await Task.Delay(1000);
                        aantalGecrawled++;

                        Debug.WriteLine("antwoord van getResponseBody: " + zegtNiksMaarWachtOpStringAntwoord);
                    }


                    //pakt maar verwijdert niet responsebody uit lijstresponses, keywords komen in database
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

                }
                else
                {
                    //stop met crawlen omdat aantal meer is dan 50
                    finished = true;

                    Debug.WriteLine("aantalGecrawled max bereikt");
                }
                

                /*try
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
                }*/
                

            }
        }











        












        public async Task<bool> getResponseBody(string url)
        //public async void getResponseBody(string url)
        {
            bool bodyGelukt = false;
            
            try
            {
                Debug.WriteLine("getResponseBody starts");

                //getResponseBody url
                httpClientRequest = new MaakHttpClientAan();

                string body = "";

                //welke url crawlen
                Debug.WriteLine("url in getResponseBody() = " + url);

                //als de methode public async void getResponseBody() dan geeft de volgende reden een error dat antwoord niet null mag zijn
                string antwoord = await httpClientRequest.doeHttpRequestYoutubeVoorScrawlerEnGeefResults(url); //await = wacht totdat antwoord is
                //Debug.WriteLine(antwoord);

                //haal body uit string
                //body = CrawlerRegex.regexBody(antwoord);
                //body = CrawlerRegex.regexContent(body);
                body = regexContent(antwoord);

                lock (this.locker)
                {
                    lijstResponses.Add(body);
                    lijstResponsesKeywords.Add(body);
                    bodyGelukt = true;
                }

            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine("getResponseBody() geeft NullReferenceException: " + e.Message);
            }

            return bodyGelukt;

        }

        public void getUrls(string httpResponseBody)
        {
            try
            {
                //String[] gevondenUrls = CrawlerRegex.regexUrls(httpResponseBody);
                List<string> gevondenUrls = regexUrls(httpResponseBody);
                //toevoegen aan lijstUrls
                lock (this.locker)
                {
                    foreach (string url in gevondenUrls)
                    {
                        lijstUrls.Add(url);
                        Debug.WriteLine("getUrls: " + url);
                    }
                }
                
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
                //CrawlerRegex.regexKeywords(httpResponseBody);
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
                        //Debug.WriteLine("pakUitQueue(lijst) geeft terug: " + x);
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

        //haal body uit httpResponseBody
        static public string regexBody(string response)
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
                Debug.WriteLine("Geen content kunnen vinden.");
            }
            return body;
        }

        //haal urls uit content
        public List<string> regexUrls(string response)
        {
            int maxUrls = 50;
            int gevondenUrls = 0;
            Object thisLocker = new object();

            //haal urls uit body
            List<string> urls = new List<string>();
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
                                //Debug.WriteLine("Vlak voordat url toegevoegd wordt: " + url);
                                lock (thisLocker)
                                {
                                    urls.Add(url);
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
            return urls;
        }

        //haal keywords uit content
        static public void regexKeywords(string response)
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
            catch (NullReferenceException e)
            {
                Debug.WriteLine("getKeywords() geeft NullReferenceException: " + e.Message);
            }

        }
    }
}
