using AForge.Video; 
using AForge.Video.DirectShow;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
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
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace Horus
{
    /// <summary>
    /// Main Window which holds graphics and core processing code
    /// </summary>
    public partial class MainWindow : Window
    {
        //Webcam Variables
        private VideoCaptureDevice videoCaptureDevice;      //Create an Instance for VideoCaptureDevice
        private HaarObjectDetector haarObjectDetector;      //Create an Instance for Haar Object
        private HaarCascade cascade;

        //Image Processing Variablse
        private bool cameraOnline = false;
        private static bool working = false;
        private DateTime lastCheck;        
        private DateTime lastNotification;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            if (App.facialCheckDisabled)
            {
                this.MouseDown += MainWindow_MouseDown;
                this.KeyUp += MainWindow_KeyUp;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!App.CloseApplication)
            {
                e.Cancel = true;
                return;
            }

            CloseVideoSource();
            App.RequestClose();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide Mouse Cursor
            Mouse.OverrideCursor = Cursors.None;

            //Full Screen
            WindowState = WindowState.Maximized;

            if (!App.facialCheckDisabled)
            {
                //Configure Webcam
                cascade = new FaceHaarCascade();
                haarObjectDetector = new HaarObjectDetector(cascade,
                    25, ObjectDetectorSearchMode.Single, 1.2f,
                    ObjectDetectorScalingMode.SmallerToGreater);

                //Connect to Camera
                OpenVideoSource();
            }

            //Graphics/Animations for Screensaver
            Uri video = new Uri(@"horus.wmv", UriKind.Relative);
            mediaPlayer.Source = video;
        }
        
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            //Loop video playback
            mediaPlayer.Position = new TimeSpan(0, 0, 0, 0, 1);
            mediaPlayer.Play();
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            App.CloseApplication = true;
            App.RequestClose();
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.CloseApplication = true;
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

                cameraOnline = true;

                App.WriteMessage("Webcam Connected");
            }
            catch (Exception ex)
            {
                App.WriteMessage(ex.ToString());
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
                catch (Exception ex)
                {
                    App.WriteMessage(ex.ToString());
                }
            }            
        }

        /// <summary>getFrame from the videosource</summary>
        private void nextFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // get new frame
            Bitmap bitmap = eventArgs.Frame;
            Bitmap lastFrame = (Bitmap) bitmap.Clone();
            
            //Don't check more than every 10 seconds
            if (DateTime.Now.Subtract(lastCheck).TotalSeconds > 10)
            {
                //Only continue if there is no processing happening in the background
                if (!working)
                {
                    //We can now process a frame
                    //Set flags
                    lastCheck = DateTime.Now;
                    working = true;

                    //Resize Frame
                    ResizeBicubic resize = new ResizeBicubic(lastFrame.Width / 2, lastFrame.Height / 2);
                    Bitmap resized = resize.Apply(lastFrame);

                    //Process image
                    processImage(resized);
                }
            }
        }

        /// <summary></summary>
        private async void processImage(Bitmap image)
        {
            await
                Task.Run(
                    async () =>
                    //This code is intended to run on a new thread, control is returned to the caller on the UI thread.
                    {
                        Guid photoID = Guid.NewGuid();

                        string photoName = photoID.ToString() + ".png"; //file name
                        string imagePath = System.IO.Path.Combine(App.imageSavePath, photoName);
                        
                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            image.Save(fileStream, ImageFormat.Png);
                            fileStream.Close();
                        }

                        App.WriteMessage("Photo Saved To File");

                        //Check for Face & Get Attributes
                        bool intruder = false;
                        string username = "";

                        //API Call to Microsoft Cognitive
                        List<Face> faces = await App.UploadAndDetectFaces(imagePath);

                        if (faces != null)
                        {
                            if (faces.Count > 0)
                            {
                                //Face Detected
                                App.WriteMessage("Faces Detected From API");

                                //Compare to Owner
                                username = await App.VerifyPerson(faces.Select(f => f.FaceId).ToArray());

                                if (username == "")
                                {
                                    //Face detected, not owner
                                    intruder = true;
                                }
                            }
                            else
                            {
                                App.WriteMessage("No Faces Detected");
                            }
                        }

                        //Test to see if we have an intruder
                        if (intruder)
                        {
                            //Check we have not sent a notification in the past 2 minutes
                            if (DateTime.Now.Subtract(lastNotification).TotalMinutes >= 2 )
                            {
                                //Upload Photo to Google Drive
                                byte[] byteArray = File.ReadAllBytes(System.IO.Path.Combine(App.imageSavePath, photoName));

                                await GoogleAppAuthorisation.AuthorizeAndUpload(byteArray, photoName);
                                App.WriteMessage("Photo Uploaded to Drive");

                                //Send Notification
                                if (App.smsClient != null)
                                {
                                    App.smsClient.sendSMS();
                                    App.WriteMessage("SMS Notification Sent");
                                }

                                lastNotification = DateTime.Now;
                            }

                            //Display Verification Failed Message
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                MessageWindow msg = new MessageWindow { Owner = this, state = 0 };
                                msg.ShowDialog();
                            }));
                        }
                        else if (username != "")
                        {
                            //Delete Image - No Need to keep saved image of verified
                            File.Delete(imagePath);

                            //Unlock Screensaver
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                MessageWindow msg = new MessageWindow { Owner = this, state = 1, username = username };
                                msg.ShowDialog();

                                App.CloseApplication = true;
                                this.Close();
                            }));
                        }
                        else
                        {
                            //Delete Image - No Need to keep saved image of nothing
                            File.Delete(imagePath);
                        }

                        working = false;                       
                    });
        }
    }
}
