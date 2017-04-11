﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Windows.UI.Popups;

namespace Vidarr.Classes
{
    class CrawlerRegex
    {

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
        static public string regexContent(string response)
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
            //else{ Debug.WriteLine("Geen content kunnen vinden.");}
            return body;
        }

        //haal results uit httpResponseBody
        static public string regexResults(string response)
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
        static public List<string> regexUrls(string response)
        {
            int maxUrls = 50;
            int gevondenUrls = 0;
            Object thisLocker = new object();

            //haal urls uit body
            List<string> urls = new List<string>();
            string vorigGevondenUrl = "";
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
                        if (url.Contains("www.youtube.com") && !url.Contains("channel") && !url.Contains("user") && !url.Contains("playlist") && !url.Contains("feed") && !url.Contains("bit.ly") && !url.Contains("accounts.google") && !url.Contains("android-app"))
                        {
                            //Debug.WriteLine("Gevonden url: " + url);
                            bool isUri = Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
                            //Debug.WriteLine("bool: " + isUri);
                            //correcte uri + nog geen 10 urls toegevoegd + vorig toegevoegde url was niet dezelfde
                            if (isUri && (gevondenUrls < maxUrls) && !vorigGevondenUrl.Equals(url))
                            {
                                //Debug.WriteLine("Vlak voordat url toegevoegd wordt: " + url);
                                lock (thisLocker)
                                {
                                    urls.Add(url);
                                    //Debug.WriteLine(url);

                                    //dubbele urls te voorkomen
                                    vorigGevondenUrl = url;
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
            //string pattern = "title=\"(.*?)</a>";
            string pattern = "(<meta itemprop=\"|<link itemprop=\"thumbnailUrl\")(.*?)\">";
            string title = "(<meta itemprop=\"name\" content=\")(.*?)\">";
            string description = "(<meta itemprop=\"description\" content=\")(.*?)\">";
            string genre = "(<meta itemprop=\"genre\" content=\")(.*?)\">";
            string thumbnail = "(<link itemprop=\"thumbnailUrl\" href=\")(.*?)\">";

            MatchCollection collection;
            MatchCollection collectionTitle;
            MatchCollection collectionDescription;
            MatchCollection collectionGenre;
            MatchCollection collectionThumbnail;
            var videos = new Videos();

            try
            {
                collection = Regex.Matches(response, pattern);
                foreach (Match m in collection)
                {
                    //spuug uit van je gevonden hebt
                    keywords = m.Value;
                    string gevondenTitle = "";
                    string gevondenDescription = "";
                    string gevondenGenre = "";
                    string gevondenThumbnail = "";

                    //keywords in database!!!!!!!!!
                    Debug.WriteLine("Gevonden keywords: " + keywords);

                    collectionTitle = Regex.Matches(m.Value, title);
                    foreach (Match m2 in collectionTitle)
                    {
                        gevondenTitle = m2.Groups[2].Value;
                        //videos.Title = gevondenTitle;
                        Debug.WriteLine("Gevonden title: " + gevondenTitle);
                    }
                    collectionDescription = Regex.Matches(m.Value, description);
                    foreach (Match m2 in collectionDescription)
                    {
                        gevondenDescription = m2.Groups[2].Value;
                        //videos.Description = gevondenDescription;
                        Debug.WriteLine("Gevonden description: " + gevondenDescription);
                    }
                    collectionGenre = Regex.Matches(m.Value, genre);
                    foreach (Match m2 in collectionGenre)
                    {
                        gevondenGenre = m2.Groups[2].Value;
                        //videos.Genre = gevondenGenre;
                        Debug.WriteLine("Gevonden genre: " + gevondenGenre);
                    }
                    collectionThumbnail = Regex.Matches(m.Value, thumbnail);
                    foreach (Match m2 in collectionThumbnail)
                    {
                        gevondenThumbnail = m2.Groups[2].Value;
                        //videos.Thumbnail = gevondenThumbnail;
                        Debug.WriteLine("Gevonden thumbnailUrl: " + gevondenThumbnail);
                    }

                    //cmd.CommandText = "INSERT INTO video(Url,Title,Description,Genre,Thumbnail) VALUES('https://www.youtube.com/watch?v=fPJ2RAmDQ3Y','DMX - We In Here (Dirty Version)','DMX official music video for 'We In Here'.','Rap','https://i.ytimg.com/vi/1GGw2nqIMfE/hqdefault.jpg')";


                    MySqlConnection conn;
                    string myConnectionString;

                    myConnectionString = "Server=127.0.0.1;Database=vidarr;Uid=root;Pwd='';SslMode=None;charset=utf8";

                    try
                    {
                        conn = new MySqlConnection(myConnectionString);
                        MySqlCommand cmd = new MySqlCommand();
                        MySqlDataReader reader;

                        //cmd.CommandText = "INSERT INTO video(Url,Title,Description,Genre,Thumbnail) VALUES('https://www.youtube.com/watch?v=fPJ2RAmDQ3Y" + "','" + gevondenTitle + "','" + gevondenDescription + "','" + gevondenGenre + "','" + gevondenThumbnail + "'";
                        conn.Open();
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = conn;
                        cmd.ExecuteNonQuery();
                        //reader = cmd.ExecuteReader();
                    }
                    catch (MySqlException ex)
                    {
                        //MessageBox.Show(ex.Message);

                        var dialog = new MessageDialog(ex.Message);
                        //dialog.ShowAsync();
                    }



                    //using (var db = new BloggingContext())
                    //{
                    //var video = new Videos { Title = gevondenTitle, Description = gevondenDescription, Genre = gevondenGenre, Thumbnail = gevondenThumbnail };

                    //db.Videos.Add(videos);
                    //Debug.WriteLine("DIT IS EEN TEST " + gevondenTitle + " EN DIT OOK " + videos.Title);
                    //db.Videos.Add(vidDescription);
                    //db.Videos.Add(vidGenre);
                    //db.Videos.Add(vidThumbnail);

                    //db.SaveChanges();
                    //}
                }
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine("getKeywords() geeft NullReferenceException: " + e.Message);
            }

        }

        /*
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
                Debug.WriteLine(body);
            }
            else
            {
                //Debug.WriteLine("Geen body kunnen vinden.");
            }
            return body;
        }

        //haal content uit httpResponseBody
        static public string regexContent(string response)
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
                //Debug.WriteLine("Geen content kunnen vinden.");
            }
            return body;
        }

        //haal results uit httpResponseBody
        static public string regexResults(string response)
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
        static public string[] regexUrls(string response)
        {
            int maxUrls = 20;
            int gevondenUrls = 0;
            Object thisLocker = new object();

            //haal urls uit body
            string[] urls = new string[maxUrls];
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
                                Debug.WriteLine("Vlak voordat url toegevoegd wordt: " + url);
                                lock (thisLocker)
                                {
                                    urls[gevondenUrls] = url;
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

        }*/
    }

    
}
