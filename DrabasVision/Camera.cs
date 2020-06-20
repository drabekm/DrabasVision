using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// Aforge.net      | maybe I should've used OpenCV? I dunno, documentation on this thing is pretty shitty imo
using AForge;
using AForge.Imaging.ColorReduction;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;

namespace DrabasVision
{
    

    class Camera
    {
        private VideoCaptureDevice CurrentDevice { get; set; }
        private string CurrentDeviceName { get; set; }
        private Analyzer analyzer;

        private int Threshold { get; set; }

        System.Windows.Controls.Image outputControl;

        public Camera(int cameraIndex, int resolutionIndex, int threshold, System.Windows.Controls.Image outputControl)
        {
            try
            {
                this.outputControl = outputControl;
                this.Threshold = threshold;

                FilterInfoCollection devices = GetDeviceCollection();
                FilterInfo device = devices[cameraIndex];

                CurrentDeviceName = device.Name;
                CurrentDevice = new VideoCaptureDevice(device.MonikerString);
                CurrentDevice.VideoResolution = CurrentDevice.VideoCapabilities[resolutionIndex];
              /*  foreach(var capability in CurrentDevice.VideoCapabilities)
                {
                    MessageBox.Show(capability.FrameSize.Width + " " + capability.FrameSize.Height);
                }*/

                analyzer = new Analyzer();
            }
            catch
            {
                CurrentDeviceName = String.Empty;
                CurrentDevice = null;
                MessageBox.Show("Error initializing camera instance.", "Error", MessageBoxButton.OK);
            }
        }

        public void SetThreshold(int value)
        {
            this.Threshold = value;
        }

        public void Start()
        {
            if(CurrentDevice != null)
            {
                try
                {                  
                    CurrentDevice.NewFrame += new NewFrameEventHandler(CameraNewFrameEventHandler);
                    CurrentDevice.Start();
                }
                catch
                {
                    MessageBox.Show($"Error opening a device called {CurrentDeviceName}", "Error", MessageBoxButton.OK);
                }
            }
        }

        private void CameraNewFrameEventHandler(object sender, NewFrameEventArgs e)
        {
            var grayScaleWinformsBitmap = GetGrayScaleFrameImage(e);
            var blackAndWhitedWinformsBitmap = GetBlackAndWhiteBitmap(grayScaleWinformsBitmap, out grayScaleWinformsBitmap);

            var test = analyzer.Analyse(blackAndWhitedWinformsBitmap, grayScaleWinformsBitmap);
            BitmapImage analysedWPFBitmap = BitmapHelper.ConvertWinformBitmapToWPFBitmap(test);

            SendFrameToUI(analysedWPFBitmap);
        }

        private Bitmap GetGrayScaleFrameImage(NewFrameEventArgs e)
        {
            Bitmap winformsBitmap = (Bitmap)e.Frame.Clone();
            
            winformsBitmap = Grayscale.CommonAlgorithms.BT709.Apply(winformsBitmap);
            return winformsBitmap;
        }

        /// <summary>
        /// Transforms a grayscale image into a black and white one
        /// </summary>
        /// <param name="bitmap">Bitmap needs to be in grayscale!</param>
        /// <returns></returns>
        private Bitmap GetBlackAndWhiteBitmap(Bitmap bitmap, out Bitmap original)
        {
            Threshold filter = new Threshold(Threshold);
            original = new Bitmap(bitmap);
            filter.ApplyInPlace(bitmap);
            return bitmap;
        }
        
        private void SendFrameToUI(BitmapImage image)
        {
            image.Freeze();
            outputControl.Dispatcher.BeginInvoke(new Action(() => outputControl.Source = image));
        }

        public void Stop()
        {
            if (CurrentDevice != null)
            {
                CurrentDevice.SignalToStop();
                CurrentDevice.WaitForStop();
                //CurrentDevice.Stop();
                CurrentDevice.NewFrame -= new NewFrameEventHandler(CameraNewFrameEventHandler);
            }
        }

        public static List<string> GetAllDeviceNames()
        {            
            List<string> deviceNames = new List<string>();

            FilterInfoCollection devices = GetDeviceCollection();
            
            foreach (FilterInfo device in devices)
            {
                deviceNames.Add(device.Name);
            }

            return deviceNames;
        }

        static private FilterInfoCollection GetDeviceCollection()
        {
            return new FilterInfoCollection(FilterCategory.VideoInputDevice);
        }

        public static List<string> GetDeviceResolutions(int deviceIndex)
        {
            List<string> deviceResolutions = new List<string>();
            var devices = GetDeviceCollection();
            var device = new VideoCaptureDevice(devices[deviceIndex].MonikerString);
            
            foreach (var deviceVideoCapability in device.VideoCapabilities)
            {
                deviceResolutions.Add($"{deviceVideoCapability.FrameSize.Width} x {deviceVideoCapability.FrameSize.Height}");
            }

            return deviceResolutions;
        }
    }
}
