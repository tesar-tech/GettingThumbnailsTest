using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GettingThumbnailsTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/beru.wmv"));
            var clip = await MediaClip.CreateFromFileAsync(file);
            var composition = new MediaComposition();
            composition.Clips.Add(clip);

            var framePeriod = 1 / 25d;//25 is fps of video
            var frameTimes = Enumerable.Range(0, 250).Select(x => TimeSpan.FromSeconds(framePeriod * x)).ToList();

            //generate multiple image streams using GetThumbnailsAsync
            var imageStreamsMultiple = (await composition.GetThumbnailsAsync(frameTimes, 0, 0, VideoFramePrecision.NearestFrame));

            int i = 0;
            foreach (var frameTime in frameTimes)//check all frameTimes. Looking for differences
            {
                // get single image stream for frameTime
                var imageStreamsSingle = await composition.GetThumbnailAsync(frameTime, 0, 0, VideoFramePrecision.NearestFrame);

                var bytesFromMultiple = await GetBytesFromImageStreamAsync(imageStreamsMultiple[i++]);
                var bytesFromSingle = await GetBytesFromImageStreamAsync(imageStreamsSingle);

                //compare bytes from both methods
                var absDiff = bytesFromMultiple.Zip(bytesFromSingle, (m, s) => Math.Abs(m - s)).Sum();
                if (absDiff != 0)
                {
                    //it should not jump here !!!
                    Debug.WriteLine($"There is difference {absDiff}");
                }
                Debug.WriteLine(i);
            }
        }

        async System.Threading.Tasks.Task<byte[]> GetBytesFromImageStreamAsync(ImageStream stream)
        {
            var deco = await BitmapDecoder.CreateAsync(stream);
            var pixelss = await deco.GetPixelDataAsync();
            var bytes = pixelss.DetachPixelData();
            return bytes;
        }
    }
}
