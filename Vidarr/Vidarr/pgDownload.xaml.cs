using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Vidarr
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pgDownload : Page
    {
        public pgDownload()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(pgConverter));
        }

        private void zoek(object sender, KeyRoutedEventArgs e)
        {
            //check of enter is gebruik
            if (e.Key == Windows.System.VirtualKey.Enter)
            { //do something here 
                Debug.WriteLine("Op enter gedrukt, gebruik vergrootglasknop");
                
            }
        }

        private async void querySubmittedZoek(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Debug.WriteLine("querysubmittedzoek; " + args.QueryText);
            string input = args.QueryText;

            //zoek in database
            bool welInDb = false;
            Task<bool> zoekInDb = Task<bool>.Factory.StartNew(() => 
            {
                List<string> output = new List<string>();

                MySqlConnection conn;
                string myConnectionString;

                myConnectionString = "Server=127.0.0.1;Database=vidarr;Uid=root;Pwd='';SslMode=None;charset=utf8";

                try
                {
                    System.Text.EncodingProvider ppp;
                    ppp = System.Text.CodePagesEncodingProvider.Instance;
                    Encoding.RegisterProvider(ppp);

                    conn = new MySqlConnection(myConnectionString);
                    MySqlCommand cmd;

                    conn.Open();
                    string query = "SELECT * FROM video WHERE title LIKE '%" + input + "%' ORDER BY id DESC LIMIT 0,4";

                    string url;
                    string title;
                    string description;
                    string genre;
                    string thumb;

                    cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        url = (string)reader["url"];
                        title = (string)reader["title"];
                        description = (string)reader["description"];
                        genre = (string)reader["genre"];
                        thumb = (string)reader["thumbnail"];

                        Debug.WriteLine(url + "\n" + title + "\n" + description + "\n" + genre + "\n" + thumb + "\n");
                    }

                    reader.Close();


                }
                catch (MySqlException ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                
                
                /*MySqlConnection conn;
                string myConnectionString;
                myConnectionString = "Server=127.0.0.1;Database=vidarr;Uid=root;Pwd='';SslMode=None;charset=utf8";
                try
                {
                    EncodingProvider ppp;
                    ppp = CodePagesEncodingProvider.Instance;
                    Encoding.RegisterProvider(ppp);
                    conn = new MySqlConnection(myConnectionString);
                    string query = "SELECT Url FROM video WHERE video.title LIKE'%" + input + "%'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    //MySqlDataReader reader;
                    string title;
                    conn.Open();
                    cmd = new MySqlCommand(query, conn);
                    title = cmd.ExecuteScalar().ToString();
                    Debug.WriteLine("Gevonden: "+title);
                }
                catch (MySqlException ex)
                {
                    Debug.WriteLine(ex.Message);
                }*/


                return true;
            });
            welInDb = await zoekInDb;
            if (welInDb)
            {
                Debug.WriteLine("Staat in database");
            }
            else
            {
                //staat niet in db
                //ga zoeken op zoekterm
                Debug.WriteLine("Moet gaan zoeken op zoekterm");
            }
        }
    }
}
