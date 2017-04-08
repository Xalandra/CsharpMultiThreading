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

            Task beginpuntCrawl = new Task(async() => 
            {
                await crawlBeginpunt();

                gaMaarCrawlen();
            });
            beginpuntCrawl.Start();

        }

        //zoek zonder input van user beginpunt
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
                    httpResponseBody = CrawlerRegex.regexBody(httpResponseBody);
                    httpResponseBody = CrawlerRegex.regexContent(httpResponseBody);
                    //Debug.WriteLine(httpResponseBody);

                    lock (this.locker)
                    {
                        lijstResponses.Add(httpResponseBody);
                    }

                    //Debug.WriteLine("Aantal in lijstResponses na beginpuntcrawl: " + lijstResponses.Count);
                });
                probeer.Start();
                await probeer;

                Debug.WriteLine("voorbij await probeer");
                while (!probeer.IsCompleted) { }
                gaMaarCrawlen();
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
                
                //voor testen max 50 rondes
                if (aantalGecrawled < 10)
                {
                    //pakt responsebody uit LijstResponses, urls komen in LijstUrls
                    Task bodys = await Task.Factory.StartNew(async() =>
                    {
                        //Debug.WriteLine("Task gets urls uit body uit lijstResponses");
                        try
                        {
                            string uitlijstResponses = pakUitQueue("responses");
                            if (uitlijstResponses != "")
                            {
                                getUrls(uitlijstResponses);
                            }
                            else { Debug.WriteLine("Body uit queue == lege string."); }

                            //getUrls(pakUitQueue("responses"));
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
                                string uitlijstUrls = pakUitQueue("urls");
                                Debug.WriteLine("Url uit queue: " + uitlijstUrls);
                                if (!uitlijstUrls.Equals(""))
                                {
                                    await getResponseBody(uitlijstUrls);
                                }
                                else { Debug.WriteLine("Url uit queue == lege string."); }
                                   
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
                body = CrawlerRegex.regexBody(antwoord);
                body = CrawlerRegex.regexContent(body);


                lock (this.locker)
                {
                    Debug.WriteLine("Wat doe jij in lijstResponses? " + body);
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
                String[] gevondenUrls = CrawlerRegex.regexUrls(httpResponseBody);
                //toevoegen aan lijstUrls
                lock (this.locker)
                {
                    foreach (string url in gevondenUrls)
                    {
                        Debug.WriteLine("Wat doe jij in lijstUrls? "+url);
                        lijstUrls.Add(url);
                        //Debug.WriteLine("Gevonden url uit regexclasse: " + url); //deze is af en toe leeg??
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
                CrawlerRegex.regexKeywords(httpResponseBody);
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
                        Debug.WriteLine("pakUitQueue(lijst) geeft terug: " + x);
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
