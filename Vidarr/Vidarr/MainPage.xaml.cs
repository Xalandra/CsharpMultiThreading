using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vidarr.Classes;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

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

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private async void MenuButton2_Click(object sender, RoutedEventArgs e)
        {
            //haal info van website en stop in txt bestand
            watVanWebsite.Text = await crawler.crawlZoekterm(inputZoekterm.Text);
        }

        private void MenuButton3_Click(object sender, RoutedEventArgs e)
        {
            //probeer van txt bestand xml te maken
            //tj.zetOmNaarXML();
        }

        
    }
}
