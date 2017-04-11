using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
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
using YoutubeExtractor;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Vidarr
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pgDownload : Page
    {
        ObservableCollection<DownloadViewModel> downloadList = new ObservableCollection<DownloadViewModel>();
        List<string> teDownloaden = new List<string>();
        Object locker = new object();
        
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

        private void lstDownload_LayoutUpdated(object sender, object e)
        {
            lstDownload.Width = gridRoot.ActualWidth;
        }

        private async void querySubmittedZoek(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Debug.WriteLine("querysubmittedzoek; " + args.QueryText);
            string input = args.QueryText;
            List<ResultZoekterm> resultatenLijst = new List<ResultZoekterm>();

            //zoek in database
            bool welInDb = false;
            Task<bool> zoekInDb = Task<bool>.Factory.StartNew(() => 
            {
                List<string> output = new List<string>();
                bool gelukt = false;

                MySqlConnection conn;
                string myConnectionString;

                myConnectionString = "Server=127.0.0.1;Database=vidarr;Uid=root;Pwd='';SslMode=None;charset=utf8";

                try
                {
                    EncodingProvider ppp;
                    ppp = CodePagesEncodingProvider.Instance;
                    Encoding.RegisterProvider(ppp);

                    conn = new MySqlConnection(myConnectionString);
                    MySqlCommand cmd;

                    conn.Open();
                    string query = "SELECT * FROM video WHERE title LIKE '%" + input + "%' ORDER BY id DESC LIMIT 0,4";

                    string urlx;
                    string titlex;
                    string descriptionx;
                    string genrex;
                    string thumbx;

                    cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        urlx = (string)reader["url"];
                        titlex = (string)reader["title"];
                        descriptionx = (string)reader["description"];
                        genrex = (string)reader["genre"];
                        thumbx = (string)reader["thumbnail"];
                        urlx += ";"+titlex+";"+thumbx;

                        Debug.WriteLine(urlx + "\n" + titlex + "\n" + descriptionx + "\n" + genrex + "\n" + thumbx + "\n");
                        resultatenLijst.Add(new ResultZoekterm { url = urlx, titel = titlex, description = descriptionx, genre = genrex, thumb = thumbx });
                        gelukt = true;
                    }

                    reader.Close();

                    
                }
                catch (MySqlException ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                return gelukt;
            });
            zoekInDb.Wait();
            zoektermResults.ItemsSource = resultatenLijst;
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

                await ZoekZoekterm.crawlZoekterm(input);

                Debug.WriteLine("Crawler is aan het zoeken, probeer het zo weer");
                var dialog = new MessageDialog("Crawler is aan het zoeken, probeer het zo weer");
                await dialog.ShowAsync();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //url uit aangevinkt resultaat
            List<string> aangevinkt = new List<string>();
            lock (this.locker)
            {
                aangevinkt = teDownloaden;
            }
            foreach (string url in aangevinkt)
            {
                string[] ontleden = url.Split(';');

                IEnumerable<VideoInfo> videosInfors = DownloadUrlResolver.GetDownloadUrls(url);

                VideoInfo video = videosInfors.First(infor => infor.VideoType == VideoType.Mp4);

                downloadList.Add(new DownloadViewModel(ontleden[0], ontleden[1], ontleden[2], video.VideoExtension));
                lstDownload.ItemsSource = downloadList;
                //textBox.Text = id[1];
            }

                  
        }

        private void slaAangevinkteUrlOp(object sender, RoutedEventArgs e)
        {
            CheckBox geselecteerdeBox = sender as CheckBox;
            string url = "";
            try
            {
                url = geselecteerdeBox.Content.ToString();
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            Debug.WriteLine(url);
            if (!url.Equals(""))
            {
                teDownloaden.Add(url);
            }
        }
    }
}
