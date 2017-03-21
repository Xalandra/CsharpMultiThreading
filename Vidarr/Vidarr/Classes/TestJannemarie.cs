using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Web.Http;

public class TestJannemarie
{
    public TestJannemarie()
    {
    }

    public async Task<String> doeWatJannemarieWil()
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

        string zoekterm = "koekjes";
        Uri requestUri = new Uri("https://www.youtube.com/results?search_query=" + zoekterm);

        //Send the GET request asynchronously and retrieve the response as a string.
        HttpResponseMessage httpResponse = new HttpResponseMessage();
        string httpResponseBody = "";

        try
        {
            //Send the GET request
            //krijgen text/html terug
            httpResponse = await httpClient.GetAsync(requestUri);
            httpResponse.EnsureSuccessStatusCode();
            httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
        }



        //bestandpicker in downloadsmap
        FileSavePicker savePicker = new FileSavePicker();
        savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
        savePicker.FileTypeChoices.Add("Simple Line Files", new List<string>() { ".txt" });
        savePicker.SuggestedFileName = "XtestCSharp.txt";
        StorageFile file = await savePicker.PickSaveFileAsync();
        //schrijf httpResponseBody naar txt bestand
        await FileIO.WriteTextAsync(file, httpResponseBody);

        //return string van httpResponseBody
        //return httpResponseBody;
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
}
