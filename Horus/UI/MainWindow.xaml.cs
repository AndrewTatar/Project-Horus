//Aforge Reference
using AForge.Video; //this needs to be imported via the Accord package in nuget otherwise namespace conflict with aforge's.
using AForge.Video.DirectShow; //these namespaces are imported via the Accord package in nuget otherwise there will be a namespace conflict with aforge's. 
//using AForge.Imaging.Filters;
//Accord Reference
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Vision.Motion;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
using Rectangle = System.Drawing.Rectangle;
using System.Windows.Threading;
using Horus.Classes;
using System.Drawing.Imaging;
using System.IO;

namespace Horus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Webcam Variables
        private VideoCaptureDevice videoCaptureDevice;      //Create an Instance for VideoCaptureDevice
        private HaarObjectDetector haarObjectDetector;      //Create an Instance for Haar Object
        private HaarCascade cascade;
        private SimpleBackgroundModelingDetector motionDetector = new SimpleBackgroundModelingDetector();

        //Image Processing Variablse
        private Bitmap bitmap, lastFrame, grayImage;
        private bool face_captured = true;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private bool cameraOnline = false;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            //this.MouseDown += MainWindow_MouseDown;
            //this.KeyUp += MainWindow_KeyUp;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseVideoSource();
            App.RequestClose();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide Mouse Cursor
            Mouse.OverrideCursor = Cursors.None;

            //Full Screen
            WindowState = WindowState.Maximized;

            //Configure Webcam
            cascade = new FaceHaarCascade();
            haarObjectDetector = new HaarObjectDetector(cascade,
                25, ObjectDetectorSearchMode.Single, 1.2f,
                ObjectDetectorScalingMode.SmallerToGreater);

            //Setup Dispatcher Timer
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 1, 0);

            //Connect to Camera
            OpenVideoSource();

            //Graphics/Animations for Screensaver
            //TODO: Graphics/Animation Loading

            Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00ff00"));
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            App.RequestClose();
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.RequestClose();
        }
        
        //Open the video source
        private void OpenVideoSource()
        {
            try
            {
                //gets the first webcam available
                videoCaptureDevice = new VideoCaptureDevice(new FilterInfoCollection(FilterCategory.VideoInputDevice)[0].MonikerString);

                //Start Capture of Video Device
                videoCaptureDevice.NewFrame += new NewFrameEventHandler(nextFrame);
                videoCaptureDevice.Start();

                dispatcherTimer.Start();
                cameraOnline = true;

                App.WriteMessage("Webcam Connected");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        //Close the video source
        private void CloseVideoSource()
        {
            if (cameraOnline)
            {
                try
                {
                    videoCaptureDevice.Stop();
                    cameraOnline = false;

                    App.WriteMessage("Webcam Disconnected");
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
            }            
        }

        /// <summary>getFrame from the videosource</summary>
        private void nextFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // get new frame
            bitmap = eventArgs.Frame;
            lastFrame = (Bitmap) bitmap.Clone();

            // process the frame
            ResizeBicubic resize = new ResizeBicubic(lastFrame.Width/2, lastFrame.Height/2);
            Bitmap bresize = resize.Apply(lastFrame);

            //Convert the Image into grayscale Image
            grayImage = Grayscale.CommonAlgorithms.BT709.Apply(bresize);
            Rectangle[] rect = haarObjectDetector.ProcessFrame(grayImage);
            if (rect != null)
            {
                //check flag is false
                if (face_captured == false)
                {
                    App.WriteMessage("Face Captured");

                    //take a pic if false, set  flag true (reset to false after 2 minutes (could be 5 or 0 minutes))
                    captureImage(lastFrame);
                    face_captured = true;
                }
            }
        }

        /// <summary></summary>
        private async void captureImage(Bitmap image)
        {
            await
                Task.Run(
                    async () =>
                    //This code is intended to run on a new thread, control is returned to the caller on the UI thread.
                    {
                        Guid photoID = Guid.NewGuid();

                        string photoName = photoID.ToString() + ".png"; //file name

                        using (var fileStream = new FileStream(System.IO.Path.Combine(App.imageSavePath, photoName), FileMode.Create))
                        {
                            image.Save(fileStream, ImageFormat.Png);
                        }

                        App.WriteMessage("Photo Saved To File");

                        byte[] byteArray = File.ReadAllBytes(System.IO.Path.Combine(App.imageSavePath, photoName));

                        await GoogleAppAuthorisation.AuthorizeAndUpload(byteArray, photoName);
                        App.WriteMessage("Photo Uploaded to Drive");

                        if (App.smsClient != null)
                        {
                            App.smsClient.sendSMS();
                            App.WriteMessage("SMS Notification Sent");
                        }                        
                    });
        }

        /// <summary></summary>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //Clear Face Captured Flag
            if (face_captured == true)
            {
                face_captured = false;
            }
        }
    }
}
