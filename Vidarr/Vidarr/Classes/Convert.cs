using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;

namespace Vidarr
{
    public partial class Convert
    {
        AsyncLock bouncer = new AsyncLock();

        void TranscodeProgress(IAsyncActionWithProgress<double> asyncInfo, double percent)
        {
            // Display or handle progress info.
        }

        void TranscodeComplete(IAsyncActionWithProgress<double> asyncInfo, AsyncStatus status)
        {
            asyncInfo.GetResults();
            if (asyncInfo.Status == AsyncStatus.Completed)
            {
                // Display or handle complete info.
            }
            else if (asyncInfo.Status == AsyncStatus.Canceled)
            {
                // Display or handle cancel info.
            }
            else
            {
                // Display or handle error info.
            }
        }

        public async void PickMedia()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");

            StorageFile source = await openPicker.PickSingleFileAsync();

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();

            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.VideosLibrary;

            savePicker.DefaultFileExtension = ".mp3";
            savePicker.SuggestedFileName = "New Video";

            savePicker.FileTypeChoices.Add("MPEG3", new string[] { ".mp3" });

            StorageFile destination = await savePicker.PickSaveFileAsync();

            MediaEncodingProfile profile =
                MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);

            MediaTranscoder transcoder = new MediaTranscoder();

            PrepareTranscodeResult prepareOp = await
                transcoder.PrepareFileTranscodeAsync(source, destination, profile);

            if (prepareOp.CanTranscode)
            {
                var transcodeOp = prepareOp.TranscodeAsync();

                transcodeOp.Progress +=
                    new AsyncActionProgressHandler<double>(TranscodeProgress);
                transcodeOp.Completed +=
                    new AsyncActionWithProgressCompletedHandler<double>(TranscodeComplete);
            }
            else
            {
                switch (prepareOp.FailureReason)
                {
                    case TranscodeFailureReason.CodecNotFound:
                        System.Diagnostics.Debug.WriteLine("Codec not found.");
                        break;
                    case TranscodeFailureReason.InvalidProfile:
                        System.Diagnostics.Debug.WriteLine("Invalid profile.");
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Unknown failure.");
                        break;
                }
            }
        }
        
        public async void ConvertChosenMedia(List<StorageFile> selectedFiles, string output)
        {
            List<StorageFile> files = selectedFiles;
            foreach (StorageFile file in files)
            {
                StorageFile source = file;
                MediaEncodingProfile profile = new MediaEncodingProfile();

                //wat wil de gebruiker er mee
                switch (output)
                {
                    case "MP3":
                        profile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
                        break;
                    case "MP4":
                        profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
                        break;
                    case "WAV":
                        profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
                        break;
                    default:
                        Debug.WriteLine("Gekozen formaat om naar te converten staat niet in de switchlijst");
                        break;

                }

                //bepaald waar het nieuwe bestand komt
                var folder = KnownFolders.VideosLibrary;
                string datetime = "_"+DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second;
                var destination = await folder.CreateFileAsync(source.Name.Remove(source.Name.Length - 4)+ datetime+"." +output);
                
                MediaTranscoder transcoder = new MediaTranscoder();

                PrepareTranscodeResult prepareOp = null;
                while (source.Equals(null)|| destination.Equals(null)|| profile.Equals(null)) {
                    //blijf wachten
                }
                using (await bouncer.LockAsync())
                {
                    //afvangen dat prepareOp nullreferenceexception gooit
                    try
                    {
                        prepareOp = await transcoder.PrepareFileTranscodeAsync(source, destination, profile);
                        //als het kan
                        if (prepareOp.CanTranscode)
                        {
                            var transcodeOp = prepareOp.TranscodeAsync();

                            transcodeOp.Progress +=
                                new AsyncActionProgressHandler<double>(TranscodeProgress);
                            transcodeOp.Completed +=
                                new AsyncActionWithProgressCompletedHandler<double>(TranscodeComplete);
                        }
                        else
                        {
                            switch (prepareOp.FailureReason)
                            {
                                case TranscodeFailureReason.CodecNotFound:
                                    Debug.WriteLine("Codec not found.");
                                    break;
                                case TranscodeFailureReason.InvalidProfile:
                                    Debug.WriteLine("Invalid profile.");
                                    break;
                                default:
                                    Debug.WriteLine("Unknown failure.");
                                    break;
                            }
                        }
                    }
                    catch (NullReferenceException ex) { Debug.WriteLine(ex.Message); }
                }
            }//foreach
            
        }
    }
}
