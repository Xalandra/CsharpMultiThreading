using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

        private async void zoek(object sender, KeyRoutedEventArgs e)
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

            //zoek in database
            bool welInDb = false;
            Task<bool> zoekInDb = Task<bool>.Factory.StartNew(() => {



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
