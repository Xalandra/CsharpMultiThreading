using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vidarr.Classes;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using System.Text;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Vidarr
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Crawler crawler;

        public MainPage()
        {
            this.InitializeComponent();

            crawler = new Crawler();
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }


        private void MenuButton1_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Download));

        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void MenuButton2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuButton3_Click(object sender, RoutedEventArgs e)
        {
            //probeer van txt bestand xml te maken
            //tj.zetOmNaarXML();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            crawler.outputLists();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using (var db = new BloggingContext())
            {
                Videos.ItemsSource = db.Videos.ToList();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new BloggingContext())
            {
                var blog = new Videos { Url = NewBlogUrl.Text };
                db.Videos.Add(blog);
                db.SaveChanges();

                Videos.ItemsSource = db.Videos.ToList();
            }
        }

        private async void zoekButton_Click(object sender, RoutedEventArgs e)
        {
            //haal info van website en stop in txt bestand
            watVanWebsite.Text = await ZoekZoekterm.crawlZoekterm(inputZoekterm.Text);
        }

        //private async void Button_Click_1Async(object sender, RoutedEventArgs e)
        //{
        //    //MySqlConnection conn = new MySqlConnection("Server=127.0.0.1;Database=vidarr;Uid=root;Pwd='';SslMode=None;charset=utf8");
        //    //MySqlCommand cmd = new MySqlCommand("SELECT * FROM video");

        //    //conn.Open();
        //    //MySqlDataReader data = cmd.ExecuteReader();
        //    //while (data.Read())
        //    //{
        //    //    // doe iets met data
        //    //    String row = data.GetString(0);
        //    //}
        //    //conn.Close();


        //    //VERSIE CODE 2
        //    //MySqlConnection sqlConnection1 = new MySqlConnection("Server=127.0.0.1;Database=vidarr;Uid=root;Pwd='';SslMode=None;charset=utf8");
        //    //MySqlCommand cmd = new MySqlCommand();
        //    //MySqlDataReader reader;

        //    //cmd.CommandText = "SELECT * FROM video";
        //    //cmd.CommandType = CommandType.Text;
        //    //cmd.Connection = sqlConnection1;

        //    //sqlConnection1.Open();

        //    //reader = cmd.ExecuteReader();
        //    //// Data is accessible through the DataReader object here.

        //    //sqlConnection1.Close();

        //    MySqlConnection conn;
        //    string myConnectionString;

        //    myConnectionString = "Server=127.0.0.1;Database=vidarr;Uid=root;Pwd='';SslMode=None;charset=utf8";

        //    try
        //    {
        //        conn = new MySqlConnection(myConnectionString);
        //        MySqlCommand cmd = new MySqlCommand();
        //        MySqlDataReader reader;


        //        cmd.CommandText = "SELECT * FROM video";
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Connection = conn;
        //        conn.Open();
        //        reader = cmd.ExecuteReader();
        //    }
        //    catch (MySqlException ex)
        //    {
        //        //MessageBox.Show(ex.Message);

        //        var dialog = new MessageDialog(ex.Message);
        //        await dialog.ShowAsync();
        //    }
        //}

        private async void Button_Click_1Async(object sender, RoutedEventArgs e)
        {
            MySqlConnection conn;
            string myConnectionString;

            myConnectionString = "Server=127.0.0.1;Database=vidarr;Uid=root;Pwd='';SslMode=None;charset=utf8";

            try
            {
                conn = new MySqlConnection(myConnectionString);
                MySqlCommand cmd = new MySqlCommand();
                MySqlDataReader reader;

                

                //cmd.CommandText = "SELECT * FROM video";
                cmd.CommandText = "INSERT INTO video(Url,Title,Description,Genre,Thumbnail) VALUES('url','titlehere','Cooldescription','Rap','imagehere')";
                conn.Open();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
                reader = cmd.ExecuteReader();


            }
            catch (MySqlException ex)
            {
                //MessageBox.Show(ex.Message);

                var dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();
            }
        }
    }
}
