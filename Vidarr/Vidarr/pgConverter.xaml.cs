using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Vidarr.Classes;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Vidarr
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pgConverter : Page
    {
        public StorageFile file1;
        public StorageFile file2;
        public StorageFile file3;

        public pgConverter()
        {
            this.InitializeComponent();
            file1 = null;
            file2 = null;
            file3 = null;
            getVideos();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Convert conv = new Convert();
            string selectionBox = comboBox.SelectionBoxItem.ToString();
            List<StorageFile> files = new List<StorageFile>();
            if (file1 != null)
            {
                files.Add(file1);
            }
            if (file2 != null)
            {
                files.Add(file2);
            }
            if (file3 != null)
            {
                files.Add(file3);
            }
            conv.ConvertChosenMedia(files, selectionBox);
            //conv.PickMedia();
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            //Maak een nieuwe FileOpenPicker aan
            FileOpenPicker openPicker = new FileOpenPicker();

            //Kies welke weergave het moet hebben
            openPicker.ViewMode = PickerViewMode.Thumbnail;

            //STandaard openings plek
            openPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;

            //Bestands extensies die toegelaten worden
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".wma");
            openPicker.FileTypeFilter.Add(".mov");
            openPicker.FileTypeFilter.Add(".flv");
            openPicker.FileTypeFilter.Add(".avi");

            //Hier geven we de type selectie weer, single of multiple
            //StorageFile file = await openPicker.PickSingleFileAsync();
            file1 = await openPicker.PickSingleFileAsync();
            //MULTIPLE SELECTIE::: StorageFile file = await openPicker.PickMultipleFilesAsync();

            if (file1 != null)
            {
                textBlock.Text = "Picked file: " + file1.Name;
            }
            else
            {
                //
            }
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            //Maak een nieuwe FileOpenPicker aan
            FileOpenPicker openPicker = new FileOpenPicker();

            //Kies welke weergave het moet hebben
            openPicker.ViewMode = PickerViewMode.Thumbnail;

            //STandaard openings plek
            openPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;

            //Bestands extensies die toegelaten worden
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".wma");
            openPicker.FileTypeFilter.Add(".mov");
            openPicker.FileTypeFilter.Add(".flv");
            openPicker.FileTypeFilter.Add(".avi");

            //Hier geven we de type selectie weer, single of multiple
            file2 = await openPicker.PickSingleFileAsync();
            //MULTIPLE SELECTIE::: StorageFile file = await openPicker.PickMultipleFilesAsync();

            if (file2 != null)
            {
                textBlock1.Text = "Picked file: " + file2.Name;
            }
            else
            {
                //
            }
        }

        private async void button3_Click_1(object sender, RoutedEventArgs e)
        {
            //Maak een nieuwe FileOpenPicker aan
            FileOpenPicker openPicker = new FileOpenPicker();

            //Kies welke weergave het moet hebben
            openPicker.ViewMode = PickerViewMode.Thumbnail;

            //STandaard openings plek
            openPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;

            //Bestands extensies die toegelaten worden
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".wma");
            openPicker.FileTypeFilter.Add(".mov");
            openPicker.FileTypeFilter.Add(".flv");
            openPicker.FileTypeFilter.Add(".avi");

            //Hier geven we de type selectie weer, single of multiple
            file3 = await openPicker.PickSingleFileAsync();
            //MULTIPLE SELECTIE::: StorageFile file = await openPicker.PickMultipleFilesAsync();

            if (file3 != null)
            {
                textBlock2.Text = "Picked file: " + file3.Name;
            }
            else
            {
                //
            }
        }

        public async void getVideos()
        {
            while (true)
            {
                List<Converted> bestandenLijstDownloadsVidarrMap = new List<Converted>();
                StorageFolder picturesFolder = KnownFolders.VideosLibrary;
                IReadOnlyList<IStorageItem> itemsList = await picturesFolder.GetItemsAsync();
                foreach (var item in itemsList)
                {
                    if (item is StorageFolder)
                    {
                        //outputText.Append(item.Name + " folder\n");

                    }
                    else
                    {
                        //outputText.Append(item.Name + "\n");
                        string name = item.Name;
                        var iSize = await item.GetBasicPropertiesAsync();
                        string size = iSize.Size.ToString();
                        string ext = name.Substring(name.Length -4);
                        bestandenLijstDownloadsVidarrMap.Add(new Converted{ titel = name, grootte = size, extensie = ext });
                    }
                }

                //listOfStudents.Add("John");
                lijstGeconverteerd.ItemsSource = bestandenLijstDownloadsVidarrMap;
                await Task.Delay(10000);
            }
        }

        private void backDownloader_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(pgDownload));
        }


    }
}
