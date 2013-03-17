using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Kinect_Experiments
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor sensor;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                sensor = KinectSensor.KinectSensors[0];

                // Start sensor
                if (sensor.Status == KinectStatus.Connected)
                {
                    sensor.ColorStream.Enable();
                    sensor.DepthStream.Enable();
                    sensor.SkeletonStream.Enable();
                    sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
                    sensor.Start();
                }
            }
        }

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Create byte array with pixel data
                    byte[] pixels = new byte[colorFrame.PixelDataLength];
                    colorFrame.CopyPixelDataTo(pixels);

                    // Bytes/row = 4 * with (for bgr32)
                    int stride = colorFrame.Width * 4;
                    // Display image
                    img_colorimage.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
                }
            }
            // Auto dispose
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Stop sensor
            if (sensor != null && sensor.IsRunning)
            {
                sensor.Stop();
                sensor.ColorStream.Disable();
                sensor.DepthStream.Disable();
                sensor.SkeletonStream.Disable();
            }
        }
    }
}
