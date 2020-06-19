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
        WriteableBitmap wb { get; set; }
        System.Windows.Controls.Image outputControl;

        public Camera(int cameraIndex, System.Windows.Controls.Image outputControl)
        {
            try
            {
                this.outputControl = outputControl;

                FilterInfoCollection devices = GetDeviceCollection();
                FilterInfo device = devices[cameraIndex];

                CurrentDeviceName = device.Name;
                CurrentDevice = new VideoCaptureDevice(device.MonikerString);
            }
            catch
            {
                CurrentDeviceName = String.Empty;
                CurrentDevice = null;
                MessageBox.Show("Error initializing camera instance.", "Error", MessageBoxButton.OK);
            }
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
            var blackAndWhitedWinformsBitmap = GetGrayScaleFrameImage(e);

            BitmapImage grayScaledWriteableBitmap = BitmapHelper.ConvertWinformBitmapToWPFBitmap(blackAndWhitedWinformsBitmap);
            SendFrameToUI(grayScaledWriteableBitmap);
        }

        private Bitmap GetGrayScaleFrameImage(NewFrameEventArgs e)
        {
            Bitmap winformsBitmap = (Bitmap)e.Frame.Clone();
            Threshold filter = new Threshold(100);
            winformsBitmap = Grayscale.CommonAlgorithms.BT709.Apply(winformsBitmap);
            filter.ApplyInPlace(winformsBitmap);
            return winformsBitmap;
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
                CurrentDevice.Stop();
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
    }
}
