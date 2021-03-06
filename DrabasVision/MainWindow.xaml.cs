﻿using System;
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

using AForge;
using AForge.Imaging.ColorReduction;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;

namespace DrabasVision
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
      

        private bool CameraStarted { get; set; }
        private Camera CurrentCamera { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            LoadDeviceNames();



         /*   FilterInfoCollection devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            VideoCaptureDevice webcam;
            foreach(FilterInfo device in devices)
            {
                //MessageBox.Show(device.Name);
                webcam = new VideoCaptureDevice(device.MonikerString);
                webcam.NewFrame += new NewFrameEventHandler(webcamNewFrameEventHandler);
                webcam.Start();
                
            }*/
        }

        private void LoadDeviceNames()
        {
            foreach (var deviceName in Camera.GetAllDeviceNames())
            {
                ddlDevices.Items.Add(deviceName);
            }
            ddlDevices.SelectedIndex = 0;
            LoadDeviceResolutions(ddlDevices.SelectedIndex);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            CurrentCamera = new Camera(ddlDevices.SelectedIndex, ddlDeviceResolutions.SelectedIndex, (int)sldThreshold.Value, (OverlayType)ddlBackground.SelectedIndex, imgCameraPreview);
            ddlDevices.IsEnabled = false;
            ddlDeviceResolutions.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            sldThreshold.IsEnabled = true;
            CurrentCamera.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            CurrentCamera.Stop();
            ddlDevices.IsEnabled = true;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            sldThreshold.IsEnabled = false;
            ddlDeviceResolutions.IsEnabled = true;
        }

        private void LoadDeviceResolutions(int deviceIndex)
        {
            ddlDeviceResolutions.Items.Clear();
            foreach (var resolution in Camera.GetDeviceResolutions(deviceIndex))
            {
                ddlDeviceResolutions.Items.Add(resolution);
            }
            ddlDeviceResolutions.SelectedIndex = 0;
        }

        private void ddlDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadDeviceResolutions(ddlDevices.SelectedIndex);
        }

        private void sldThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentCamera.SetThreshold((int)sldThreshold.Value);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(CurrentCamera != null)
            {
                if(CurrentCamera.IsStarted)
                {
                    CurrentCamera.Stop();
                }
            }
        }

        private void ddlBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentCamera != null)
            {
                CurrentCamera.selectedOverlay = (OverlayType)ddlBackground.SelectedIndex;
            }
        }
    }
}
