using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;
using AForge.Video.DirectShow;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using AForge.Video;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace Horus_Config
{
    /// <summary>
    /// Interaction logic for Users.xaml
    /// </summary>
    public partial class Users : Window
    {
        public string faceGroupID;
        public Person person;
        public int state = 0;

        string newImage = "";

        //Webcam Variables
        VideoCaptureDevice webCam;
        public FilterInfoCollection webCamCollection;
        static BitmapImage lastFrame;

        public Users()
        {
            InitializeComponent();
        }

        private void LoadProfileImage()
        {
            if (person.PersistedFaceIds.Count() == 0)
                eFaceInfo.Content = "No image on file!";
            else
            {
                //Load Face from file
                foreach (var f in person.PersistedFaceIds)
                {
                    if (File.Exists(@"Faces\" + f.ToString() + ".jpg"))
                    {
                        //Load In Face Image
                        try
                        {
                            BitmapImage imgsource = new BitmapImage();
                            imgsource.BeginInit();
                            imgsource.CacheOption = BitmapCacheOption.OnLoad;
                            imgsource.UriSource = new Uri(@"Faces\" + f.ToString() + ".jpg", UriKind.Relative);
                            imgsource.EndInit();

                            iProfile.Source = imgsource;
                        }
                        catch (Exception)
                        {

                        }

                        if (iProfile.Source != null)
                            break;
                    }
                }

                if (iProfile.Source == null)
                {
                    eFaceInfo.Content = "Image not found, but data secured.";
                }
            }
        }

        private async void bSave_Click(object sender, RoutedEventArgs e)
        {
            if (eName.Text.Trim() == "")
            {
                eName.Background = new SolidColorBrush(Colors.Red);
                return;
            }
            else
            {
                eName.Background = new SolidColorBrush(Colors.White);
            }

            try
            {
                if (state == 0)
                {
                    if (newImage == "")
                    {
                        MessageBox.Show("An image is required to add this user!");
                        return;
                    }

                    //Add new Person
                    var newPerson = await MainWindow.faceServiceClient.CreatePersonAsync(faceGroupID, eName.Text);
                    if (newPerson != null)
                    {
                        //Person Created
                        //TODO: Support Multi-snap uploading
                        //Add Face to Person
                        using (Stream imgFile = File.OpenRead(newImage))
                        {
                            var addFace = await MainWindow.faceServiceClient.AddPersonFaceAsync(faceGroupID, newPerson.PersonId, imgFile);
                            if (addFace != null)
                            {
                                //Process completed
                                //Rename image file
                                File.Move(newImage, @"Faces\" + addFace.PersistedFaceId.ToString() + ".jpg");

                                MainWindow.FaceChangesMade = true;
                            }
                        }
                    }
                }
                else
                {
                    //Updating User
                    //Look for any changes
                    if (person.Name != eName.Text)
                    {
                        //Update Person Details
                        await MainWindow.faceServiceClient.UpdatePersonAsync(faceGroupID, person.PersonId, eName.Text);
                    }

                    if (newImage != "")
                    {
                        //TODO: Support Multi-snap uploading

                        //Clear Exising Faces
                        foreach(var f in person.PersistedFaceIds)
                        {
                            await MainWindow.faceServiceClient.DeletePersonFaceAsync(faceGroupID, person.PersonId, f);
                        }

                        //Add Person Faces
                        using (Stream imgFile = File.OpenRead(newImage))
                        {
                            var addFace = await MainWindow.faceServiceClient.AddPersonFaceAsync(faceGroupID, person.PersonId, imgFile);
                            if (addFace != null)
                            {
                                //Process completed
                                //Rename image file
                                File.Move(newImage, @"Faces\" + addFace.PersistedFaceId.ToString() + ".jpg");

                                MainWindow.FaceChangesMade = true;
                            }
                        }
                    }
                }

                this.Close();
            }
            catch (Exception)
            {

            }
        }

        private async void bRemoveAccess_Click(object sender, RoutedEventArgs e)
        {
            if (person != null)
            {
                var response = MessageBox.Show("Are you sure you want to delete this User?", "Delete User", MessageBoxButton.YesNo);
                if (response == MessageBoxResult.Yes)
                {
                    await MainWindow.faceServiceClient.DeletePersonAsync(faceGroupID, person.PersonId);

                    this.Close();
                }
            }            
        }

        private void bNewImage_Click(object sender, RoutedEventArgs e)
        {
            eFaceInfo.Visibility = Visibility.Hidden;

            OpenWebcam();

            bNewImage.Visibility = Visibility.Collapsed;
            bTakePhoto.Visibility = Visibility.Visible;
            bCancelPhoto.Visibility = Visibility.Visible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (state == 0)
            {
                //New User
                eFaceInfo.Content = "Profile image required!";

                bRemoveAccess.Visibility = Visibility.Collapsed;
            }
            else if (state == 1 && person != null)
            {
                //Load in Person details
                eName.Text = person.Name;

                LoadProfileImage();
            }
        }

        private void OpenWebcam()
        {
            try
            {
                if (webCam != null)
                {
                    //Clear Webcam
                    webCam.Stop();
                    webCam.NewFrame -= WebCam_NewFrame;
                    webCam = null;
                }

                webCamCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                webCam = new VideoCaptureDevice(webCamCollection[0].MonikerString);
                webCam.NewFrame += WebCam_NewFrame;

                webCam.Start();
            }
            catch (Exception)
            {

            }
        }

        private void WebCam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();

                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();

                lastFrame = bi.Clone();
                lastFrame.Freeze();
                                
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    iProfile.Source = bi;
                }));
            }
            catch (Exception)
            {

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (webCam != null)
            {
                webCam.Stop();
            }
        }

        private void bTakePhoto_Click(object sender, RoutedEventArgs e)
        {
            //Stop Webcam
            if (webCam != null)
            {
                try
                {
                    webCam.Stop();

                    Guid photoID = Guid.NewGuid();
                    string filePath = @"Faces\" + photoID.ToString() + ".jpg";

                    using (var fstream = new FileStream(filePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(lastFrame));
                        encoder.Save(fstream);
                    }
                        
                    newImage = filePath;

                    //BitmapImage imgsource = new BitmapImage();
                    //imgsource.BeginInit();
                    //imgsource.CacheOption = BitmapCacheOption.OnLoad;
                    //imgsource.UriSource = new Uri(filePath, UriKind.Relative);
                    //imgsource.EndInit();

                    //iProfile.Source = imgsource;
                }
                catch (Exception ex)
                {

                }
            }

            bNewImage.Visibility = Visibility.Visible;
            bTakePhoto.Visibility = Visibility.Collapsed;
            bCancelPhoto.Visibility = Visibility.Collapsed;
        }

        private void bCancelPhoto_Click(object sender, RoutedEventArgs e)
        {
            if (webCam != null)
            {
                webCam.Stop();

                if (newImage != "")
                {
                    //Load previous Image
                    BitmapImage imgsource = new BitmapImage();
                    imgsource.BeginInit();
                    imgsource.CacheOption = BitmapCacheOption.OnLoad;
                    imgsource.UriSource = new Uri(newImage, UriKind.Relative);
                    imgsource.EndInit();

                    iProfile.Source = imgsource;
                }
                else if (state == 1 && person != null)
                {
                    //Load Profile Image
                    LoadProfileImage();
                }
            }

            bNewImage.Visibility = Visibility.Visible;
            bTakePhoto.Visibility = Visibility.Collapsed;
            bCancelPhoto.Visibility = Visibility.Collapsed;
        }
    }
}
