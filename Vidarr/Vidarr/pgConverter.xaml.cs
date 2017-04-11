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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Convert conv = new Convert();
            conv.PickMedia();
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
            StorageFile file = await openPicker.PickSingleFileAsync();
            //MULTIPLE SELECTIE::: StorageFile file = await openPicker.PickMultipleFilesAsync();

            if (file != null)
            {
                textBlock.Text = "Picked audio: " + file.Name;
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
            StorageFile file = await openPicker.PickSingleFileAsync();
            //MULTIPLE SELECTIE::: StorageFile file = await openPicker.PickMultipleFilesAsync();

            if (file != null)
            {
                textBlock1.Text = "Picked audio: " + file.Name;
            }
            else
            {
                //
            }
        }
    }
}
