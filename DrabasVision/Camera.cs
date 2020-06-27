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
    public enum OverlayType
    {
        BlackAndWhite,
        Grayscale,
        Color
    }

    class Camera
    {
        private VideoCaptureDevice CurrentDevice { get; set; }
        private string CurrentDeviceName { get; set; }
        private Analyzer analyzer;

        public OverlayType selectedOverlay;

        public bool IsStarted { get; private set; }
        private int Threshold { get; set; }

        System.Windows.Controls.Image outputControl;

        public Camera(int cameraIndex, int resolutionIndex, int threshold, OverlayType selectedOverlay, System.Windows.Controls.Image outputControl)
        {
            try
            {
                this.outputControl = outputControl;
                this.Threshold = threshold;
                this.selectedOverlay = selectedOverlay;

                FilterInfoCollection devices = GetDeviceCollection();
                FilterInfo device = devices[cameraIndex];

                CurrentDeviceName = device.Name;
                CurrentDevice = new VideoCaptureDevice(device.MonikerString);
                CurrentDevice.VideoResolution = CurrentDevice.VideoCapabilities[resolutionIndex];

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
                    IsStarted = true;
                }
                catch
                {
                    MessageBox.Show($"Error opening a device called {CurrentDeviceName}", "Error", MessageBoxButton.OK);
                }
            }
        }

        private void CameraNewFrameEventHandler(object sender, NewFrameEventArgs e)
        {
            var originalWinformsBitmap = (Bitmap)e.Frame.Clone();
            var grayScaleWinformsBitmap = GetGrayScaleFrameImage(originalWinformsBitmap);
            var blackAndWhitedWinformsBitmap = GetBlackAndWhiteBitmap(grayScaleWinformsBitmap, out grayScaleWinformsBitmap);

            WriteableBitmap analyzedBitmap;
            switch (selectedOverlay)
            {
                case OverlayType.BlackAndWhite:
                    analyzedBitmap = analyzer.Analyse(BitmapHelper.ConvertWinformBitmapToWPFWriteableBitmap(blackAndWhitedWinformsBitmap), BitmapHelper.ConvertWinformBitmapToWPFWriteableBitmap(blackAndWhitedWinformsBitmap));
                    break;
                case OverlayType.Grayscale:
                    analyzedBitmap = analyzer.Analyse(BitmapHelper.ConvertWinformBitmapToWPFWriteableBitmap(blackAndWhitedWinformsBitmap), BitmapHelper.ConvertWinformBitmapToWPFWriteableBitmap(grayScaleWinformsBitmap));
                    break;
                case OverlayType.Color:
                    analyzedBitmap = analyzer.Analyse(BitmapHelper.ConvertWinformBitmapToWPFWriteableBitmap(blackAndWhitedWinformsBitmap), BitmapHelper.ConvertWinformBitmapToWPFWriteableBitmap(originalWinformsBitmap));
                    break;
                default:
                    analyzedBitmap = analyzer.Analyse(BitmapHelper.ConvertWinformBitmapToWPFWriteableBitmap(blackAndWhitedWinformsBitmap), BitmapHelper.ConvertWinformBitmapToWPFWriteableBitmap(grayScaleWinformsBitmap));
                    break;
            }

            

            SendFrameToUI(analyzedBitmap);
        }

        private Bitmap GetGrayScaleFrameImage(Bitmap bitmap)
        {
            bitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);
            return bitmap;
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

        private void SendFrameToUI(WriteableBitmap image)
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
                CurrentDevice.NewFrame -= new NewFrameEventHandler(CameraNewFrameEventHandler);
                IsStarted = false;
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
