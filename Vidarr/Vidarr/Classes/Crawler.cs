using MySql.Data.MySqlClient;
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
using Windows.UI.Popups;

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

        //start crawlen is een task
        public async void startCrawlen() {
            //leeg db
            maakDatabaseLeeg();

            //haal eerste body op
            beginGelukt = await crawlBeginpunt();

            //ga verder met 1e body
            gaMaarCrawlen();
        }

        //eerst database leegmaken
        public void maakDatabaseLeeg()
        {
            MySqlConnection conn;
            string myConnectionString;

            myConnectionString = "Server=127.0.0.1;Database=vidarr;Uid=root;Pwd='';SslMode=None;charset=utf8";

            try
            {
                conn = new MySqlConnection(myConnectionString);
                MySqlCommand cmd = new MySqlCommand();

                cmd.CommandText = "truncate video";
                conn.Open();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                //MessageBox.Show(ex.Message);
                var dialog = new MessageDialog(ex.Message);
                //dialog.ShowAsync();

            }
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
                httpResponseBody = CrawlerRegex.regexContent(httpResponseBody);

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
                //if (aantalGecrawled < 10)
                //{
                    //pak alleen uit lijstResponses als lijstUrls minder dan 16 heeft;
                    if (lijstUrls.Count < 11)
                    {
                        getUrls(pakUitQueue("responses")); //vult lijstUrls bij
                        aantalGecrawled++;
                    }

                    //haal 15 bodies op
                    for (int i = 0; i < 10; i++)
                    {
                        //pak eerste url en haal body eruit
                        string body = await getResponseBody(pakUitQueue("urls")); //vult lijstResponses(Keywords) aan
                        //als body niet lege string is, lege string kan komen door foute url in httprequest
                        if (!body.Equals(""))
                        {
                            //zet body in de twee bodylijsten
                            lock (this.locker)
                            {
                                lijstResponses.Add(body);
                                lijstResponsesKeywords.Add(body);
                            }
                            aantalGecrawled++;
                        }
                    }

                    //pak uit lijstResponsesKeywords zolang er een body in zit
                    while (lijstResponsesKeywords.Count > 0)
                    {
                        //haal keywords uit body uit lijstResponsesKeywords
                        getKeywords(pakUitQueue("keywords"));
                        //aantalGecrawled++;
                    }
                //}
                //else
                //{
                //    //stop met crawlen omdat aantal meer is dan 50
                //    finished = true;
                //
                //    Debug.WriteLine("aantalGecrawled max bereikt");
                //}
            }
        }

        
        public async Task<string> getResponseBody(string url)
        //public async void getResponseBody(string url)
        {
            string antwoord = "";
            Debug.WriteLine("getResponseBody starts");

            //getResponseBody url
            httpClientRequest = new MaakHttpClientAan();

            //welke url crawlen
            Debug.WriteLine("url in getResponseBody() = " + url);

            try
            {
                antwoord = await httpClientRequest.doeHttpRequestYoutubeVoorScrawlerEnGeefResults(url); //await = wacht totdat antwoord is
                //Debug.WriteLine(antwoord);
            }
            catch (Exception ex)
            {
                //als httprequest bijv door foute url een error terug geeft
                Debug.WriteLine("httpError: " + ex.StackTrace);
            }

            return antwoord;
        }

        public void getUrls(string httpResponseBody)
        {
            try
            {
                List<string> gevondenUrls = CrawlerRegex.regexUrls(httpResponseBody);
                //List<string> gevondenUrls = regexUrls(httpResponseBody);
                //toevoegen aan lijstUrls
                lock (this.locker)
                {
                    foreach (string url in gevondenUrls)
                    {
                        lijstUrls.Add(url);
                        //Debug.WriteLine("getUrls: " + url);
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
                //regexKeywords(httpResponseBody);
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
        
    }
}
