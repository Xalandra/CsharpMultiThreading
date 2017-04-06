using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Vidarr.Classes;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Web.Http;

public class TestJannemarie
{
    MaakHttpClientAan httpClientRequest;
    List<string> lijstUrls;
    List<string> lijstResponses;
    List<string> lijstResponsesKeywords;

    public TestJannemarie()
    {
        lijstUrls = new List<string>();
        lijstResponses = new List<string>();
        lijstResponsesKeywords = new List<string>();
        gaMaarCrawlen();
    }

    public async Task<String> doeWatJannemarieWil()
    {
        httpClientRequest = new MaakHttpClientAan();
        string httpResponseBody = await httpClientRequest.doeHttpRequestYoutubeMetZoektermEnGeefResults("koekjes"); //await = wacht totdat antwoord is

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

    public async void zetOmNaarXML() {
        //get XmlStuff
        //read html uit "XtestCSharp.txt"
        FileOpenPicker openPicker = new FileOpenPicker();
        openPicker.SuggestedStartLocation = PickerLocationId.Downloads;
        openPicker.FileTypeFilter.Add(".txt");
        StorageFile openFile = await openPicker.PickSingleFileAsync();
        string leesFile = await FileIO.ReadTextAsync(openFile);

        //haal bepaalde dingen met regex uit bestand
        var builder = new StringBuilder();
        foreach (var c in leesFile) {
            if (c.Equals(">"))
            {
                builder.Append(c + "\n");
            }
            else {
                builder.Append(c);
            }
        }
        //string newStuff = builder.ToString();
        string newStuff = leesFile.Replace(">", ">" + Environment.NewLine);

        //schrijf nieuw bestand
        //bestandpicker in downloadsmap
        FileSavePicker savePicker = new FileSavePicker();
        savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
        savePicker.FileTypeChoices.Add("Simple Line Files", new List<string>() { ".txt" });
        savePicker.SuggestedFileName = "XMLtestCSharp.txt";
        StorageFile file = await savePicker.PickSaveFileAsync();
        //schrijf httpResponseBody naar txt bestand
        await FileIO.WriteTextAsync(file, newStuff);
    }

    public async void gaMaarCrawlen()
    {
#pragma warning disable CS0219 // The variable 'beginPunt' is assigned but its value is never used
        bool beginPunt = true;
#pragma warning restore CS0219 // The variable 'beginPunt' is assigned but its value is never used
        bool finished = false;

        int aantalGecrawled = 0;

        //beginpunt
        try
        {
            //crawl beginpunt
            Debug.WriteLine("Task startpunt maakt body in lijstResponses");
            await Task.Factory.StartNew(() =>
            {
                getResponseBody("https://www.youtube.com/");

                    
            });
        }
        catch (NullReferenceException e)
        {
            Debug.WriteLine("beginpunt.getResponseBody() geeft NullReferenceException: " + e.Message);
        }


        //zolang true crawl maar door
        while (!finished)
        {
            //als beginpunt responses heeft gevonden
            if (lijstResponses.Count > 1)
            {
                //voor testen max 50 rondes
                //if (aantalGecrawled < 500)
                //{
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
                    await Task.Factory.StartNew(() =>
                    {
                        Debug.WriteLine("Task gets body uit url uit lijstUrls");
                        try
                        {
                            getResponseBody(pakUitQueue("urls"));
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

                //}
                //else
                //{
                    //stop met crawlen omdat aantal meer is dan 50
                    //finished = true;

                    //Debug.WriteLine("aantalGecrawled max bereikt");
                //}
            }
            else
            {
                Debug.WriteLine("beginpunt heeft nog geen responses gevonden..");
            }
        }
    }

    public async void getResponseBody(string url)
    {
        try
        {
            //getResponseBody url
            httpClientRequest = new MaakHttpClientAan();

            //welke url crawlen
            Debug.WriteLine("url in getResponseBody() = " + url);

            string antwoord = await httpClientRequest.doeHttpRequestYoutubeVoorScrawlerEnGeefResults(url); //await = wacht totdat antwoord is
            Debug.WriteLine(antwoord);

            //haal body uit string
            string body = "";
            string patternBody = @"<body\s(.*?)</body>";
   
            Match match = Regex.Match(antwoord, patternBody, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                body = match.Value;
                Debug.WriteLine(body);
            }
            else
            {
                Debug.WriteLine("Geen body kunnen vinden.");
            }
            lijstResponses.Add(body);
        }
        catch (NullReferenceException e)
        {
            Debug.WriteLine("getResponseBody() geeft NullReferenceException: " + e.Message);
        }

        
    }

    public void getUrls(string httpResponseBody) {
        try
        {
            //haal urls uit body
            string url = "";
            //string patternContent = "<div\\s*id\\s*=\\s*[\"']content[\"'](.*?)footer";
            string patternUrls = "href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))"; //beste regex

            //dan urls
            MatchCollection collection = Regex.Matches(httpResponseBody, patternUrls);
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
        catch (NullReferenceException e)
        {
            Debug.WriteLine("getUrls() geeft NullReferenceException: " + e.Message);
        }
    }

    public void getKeywords(string httpResponseBody)
    {
        try
        {
            //haal keywords uit body
            string keywords = "";
            string pattern = "title=\"(.*?)</a>";
            MatchCollection collection = Regex.Matches(httpResponseBody, pattern);
            foreach (Match m in collection)
            {
                //spuug uit van je gevonden hebt
                keywords = m.Value;
                Debug.WriteLine("Gevonden keywords: " + keywords);
            }
        } catch (NullReferenceException e)
        {
            Debug.WriteLine("getKeywords() geeft NullReferenceException: " + e.Message);
        }

    }

    public string pakUitQueue(string lijst) {
        string res = "";
        if (lijst == "responses") {
            if (lijstResponses.Count != 0)
            {
                string x = lijstResponses[0];
                lijstResponses.Remove(lijstResponses[0]);
                res = x;
            }
        }
        if (lijst == "urls") {
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

        try
        {
            for (int i = 0; lijstUrls.Count > i; i++)
            {
                Debug.WriteLine("Lijst[" + i + "] = " + lijstUrls[i]);
            }
        } catch (NullReferenceException e)
        {
            Debug.WriteLine("getKeywords() geeft NullReferenceException: " + e.Message);
        }

        return res;

        
    }

}

//begin punt
//task om responses uit startpunt te crawlen, Responsebody komt in LijstResponses
/* deze moet gedaan zijn voordat er verder gewerkt kan worden dus hier een task van maken en joinen
await Task.Factory.StartNew(() => {
    getResponseBody("https://www.youtube.com/");
});*/
/*Task startPunt = new Task(() =>
{
    Debug.WriteLine("Task startpunt maakt body in lijstResponses");
    getResponseBody("https://www.youtube.com/");
    //pakt responsebody uit LijstResponses, urls komen in LijstUrls
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
startPunt.Start();

while (!startPunt.IsCompleted)
{
    Debug.WriteLine("startPunt is nog niet klaar..");
}*/
