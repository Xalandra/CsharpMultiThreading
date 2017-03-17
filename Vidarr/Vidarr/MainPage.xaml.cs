﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Vidarr
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //List<string> videoList = new List<string>();
        string[] videoArray = new string[] {"New song", "New song" , "New song" , "New song" , "New song" };

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var v in videoArray)
            {
                listBox.Items.Add(v);
            }

            //Create an HTTP client object
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
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

            string requestString = textBox.Text;
            if (requestString.Contains(" "))
            {
                Debug.WriteLine("Het bevat een spatiebalk");
                requestString = requestString.Replace(" ", "+");
            }

            Uri requestUri = new Uri("https://www.youtube.com/results?search_query=" + requestString);
            Debug.WriteLine(requestUri);

            //Send the GET request asynchronously and retrieve the response as a string.
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            string httpResponseBody = "";

            try
            {
                //Send the GET request
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                textBox1.Text = httpResponseBody;


                MatchCollection m1 = Regex.Matches(textBox1.Text, @"(<h3(.*?)/h3>)",
                RegexOptions.Singleline);

                foreach (Match m in m1)
                {
                    string cell = m.Groups[1].Value;
                    Match match = Regex.Match(cell, @"<h3(.+?)/h3>");
                    if (match.Success)
                    {
                        string value = match.Groups[1].Value;
                        Debug.WriteLine(value);
                    }
                }

                //MatchCollection m1 = Regex.Matches(textBox1.Text, @"(<span(.*?)/span>)",
                //    RegexOptions.Singleline);

                //foreach (Match m in m1)
                //{
                //    string cell = m.Groups[1].Value;
                //    Match match = Regex.Match(cell, @"<span(.+?)/span>");
                //    if (match.Success)
                //    {
                //        string value = match.Groups[1].Value;
                //        Debug.WriteLine(value);
                //    }
                //}
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void textBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
