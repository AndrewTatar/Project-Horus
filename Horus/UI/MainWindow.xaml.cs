using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Horus.Classes;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.ProjectOxford.Face.Contract;
using System.Windows.Media.Imaging;
using System.Threading;

namespace Horus
{
    /// <summary>
    /// Main Window which holds graphics and core processing code
    /// </summary>
    public partial class MainWindow : Window
    {
        //Webcam Variables
        private VideoCaptureDevice videoCaptureDevice;      //Create an Instance for VideoCaptureDevice

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
            //this.MouseMove += MainWindow_MouseMove;
            this.MouseDown += MainWindow_MouseDown;
            this.KeyUp += MainWindow_KeyUp;

            App.ShowCameraFeed += App_ShowCameraFeed;
        }

        private void App_ShowCameraFeed(object sender, EventArgs e)
        {
            //Show Camera Feed Window
            if (cameraWindow.Visibility != Visibility.Visible)
            {
                cameraWindow.Visibility = Visibility.Visible;

                //Give a 2 second delay before taking a snapshot
                lastCheck = DateTime.Now.AddSeconds(-8);
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
            if (App.facialCheckDisabled)
            {
                App.CloseApplication = true;
                App.RequestClose();
            }
            else
            {
                //HACK: Override Application Close Flag
                if (e.Key == Key.F3)
                {
                    App.CloseApplication = true;
                }

                App.ShowCamera();
            }
        }
        
        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (App.facialCheckDisabled)
            {
                App.CloseApplication = true;
                App.RequestClose();
            }
            else
            {
                App.ShowCamera();
            }
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (App.facialCheckDisabled)
            {
                App.CloseApplication = true;
                App.RequestClose();
            }
            else
            {
                App.ShowCamera();
            }
        }
        
        //Open the video source
        private void OpenVideoSource(int source = 0)
        {
            try
            {
                //Get a list of video input devices
                FilterInfoCollection info = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                //HACK: FOR RECORDING PURPOSE ONLY
                if (info.Count > 1)
                    source = 1;

                //Start Capture of Video Device
                videoCaptureDevice = new VideoCaptureDevice(info[source].MonikerString);
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
            //Get New Frame
            Bitmap bitmap = eventArgs.Frame;
            Bitmap lastFrame = (Bitmap) bitmap.Clone();

            //Update Preview Window
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();
            
            Dispatcher.BeginInvoke(new Action(() => {
                cameraPreview.Source = bi;
            }));

            //Camera window needs to be visible
            if (this.cameraWindow.Visibility != Visibility.Collapsed)
            {
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
                                
                                //Make Sure I have Google Drive configured
                                if (App.AllowGoogleDrive)
                                {
                                    await GoogleAppAuthorisation.AuthorizeAndUpload(byteArray, photoName);
                                    App.WriteMessage("Photo Uploaded to Drive");
                                }
                                
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
                                this.cameraWindow.Visibility = Visibility.Collapsed;

                                MessageWindow msg = new MessageWindow { Owner = this, state = 0 };
                                msg.ShowDialog();
                            }));
                        }
                        else if (username != "")
                        {
                            //Delete Image - No Need to keep saved image of verified
                            File.Delete(imagePath);

                            //Check if we could verify, but not confident enough
                            if (username != "N/C")
                            {
                                //Unlock Screensaver
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    this.cameraWindow.Visibility = Visibility.Collapsed;

                                    MessageWindow msg = new MessageWindow { Owner = this, state = 1, username = username };
                                    msg.ShowDialog();

                                    App.CloseApplication = true;
                                    this.Close();
                                }));
                            }
                            else
                            {
                                //Verified, just not confidence
                                //Display Verification Failed Message
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    this.cameraWindow.Visibility = Visibility.Collapsed;

                                    MessageWindow msg = new MessageWindow { Owner = this, state = 2 };
                                    msg.ShowDialog();

                                    //Keep Window Open for re-verification
                                    this.cameraWindow.Visibility = Visibility.Visible;
                                }));
                            }
                            
                        }
                        else
                        {
                            //Delete Image - No Need to keep saved image of nothing
                            File.Delete(imagePath);

                            //No Faces Detected
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                cameraLabel.Content = "Verifying User";

                                this.cameraWindow.Visibility = Visibility.Collapsed;
                            }));
                        }

                        working = false;                       
                    });
        }
    }
}
