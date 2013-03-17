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

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Set colors based on depth
                    byte[] pixels = GenerateColoredBytes(depthFrame);

                    // Bytes/row = 4 * with (for bgr32)
                    int stride = depthFrame.Width * 4;
                    // Display image
                    img_depthimage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
                }
            }
            // Auto dispose
        }

        private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
        {
            // Get raw depth data
            short[] raw = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(raw);

            // Create byte array with size h*w*4 (for bgr_)
            Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            // Loop through all distances
            for (int depthIndex = 0, colorIndex = 0; depthIndex < raw.Length && colorIndex < pixels.Length; depthIndex++, colorIndex += 4)
            {
                // Get player
                int player = raw[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                // Get depth
                int depth = raw[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                // Set colors based on depth
                if (depth <= 900)
                {
                    pixels[colorIndex + BlueIndex] = 255;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 0;
                }
                else if (depth > 900 && depth < 2000)
                {
                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 255;
                    pixels[colorIndex + RedIndex] = 0;
                }
                else if (depth > 2000)
                {
                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 255;
                }
            }
            return pixels;
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
