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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Vidarr
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class pgConverter : Page
    {
        public pgConverter()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(pgDownload));
        }

        private async void Button_Click_1Async(object sender, RoutedEventArgs e)
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
            StorageFile file = await openPicker.PickSingleFileAsync();
            //MULTIPLE SELECTIE::: StorageFile file = await openPicker.PickMultipleFilesAsync();

            if (file != null)
            {
                FileSelectOne.Text = "Picked audio: " + file.Name;
            }
            else
            {
                //
            }
        }

        private async void Button_Click_2Async(object sender, RoutedEventArgs e)
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
            StorageFile file = await openPicker.PickSingleFileAsync();
            //MULTIPLE SELECTIE::: StorageFile file = await openPicker.PickMultipleFilesAsync();

            if (file != null)
            {
                FileSelectTwo.Text = "Picked audio: " + file.Name;
            }
            else
            {
                //
            }
        }

        private async void Button_Click_3Async(object sender, RoutedEventArgs e)
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
            StorageFile file = await openPicker.PickSingleFileAsync();
            //MULTIPLE SELECTIE::: StorageFile file = await openPicker.PickMultipleFilesAsync();

            if (file != null)
            {
                FileSelectThree.Text = "Picked audio: " + file.Name;
            }
            else
            {
                //
            }
        }

        private async void Button_Click_4Async(object sender, RoutedEventArgs e)
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
            StorageFile file = await openPicker.PickSingleFileAsync();
            //MULTIPLE SELECTIE::: StorageFile file = await openPicker.PickMultipleFilesAsync();

            if (file != null)
            {
                FileSelectFour.Text = "Picked audio: " + file.Name;
            }
            else
            {
                //
            }
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            Convert conv = new Convert();
        }
    }
}

