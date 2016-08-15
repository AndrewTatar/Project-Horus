using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Horus_Config
{
    /// <summary>
    /// Custom User Creation and Editing Dialog
    /// </summary>
    public partial class UserWizard : Window
    {
        //Public Variables
        public string faceGroupID;
        public Person person;

        //Wizard Variables/Fields
        private int state = 0;
        private List<string> imageFiles = new List<string>();
        private string lastFile;
        private bool isPreview = false;

        //Webcam Variables
        VideoCaptureDevice webCam;
        public FilterInfoCollection webCamCollection;
        static BitmapImage lastFrame;

        public UserWizard()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (person != null)
            {
                Title = "Edit User";
                eName.Text = person.Name;
            }

            ChangeState(false);
        }

        private void ChangeState (bool forwards = true)
        {
            //Update State
            if (forwards)
            {
                state += 1;
            }                
            else
            {
                if (state > 0)
                {
                    state -= 1;
                }
            }

            //Reset All States
            PageTitle.Content = "";
            gName.Visibility = Visibility.Collapsed;
            gImages.Visibility = Visibility.Collapsed;
            bNext.Visibility = Visibility.Collapsed;
            bBack.Visibility = Visibility.Collapsed;
            bSave.Visibility = Visibility.Collapsed;
            bDelete.Visibility = Visibility.Collapsed;

            switch (state)
            {
                case 0:
                    PageTitle.Content = "Please enter the name of the user";
                    gName.Visibility = Visibility.Visible;
                    bNext.Visibility = Visibility.Visible;

                    if (person != null)
                    {
                        bDelete.Visibility = Visibility.Visible;
                        bSave.Visibility = Visibility.Visible;
                    }                        

                    break;

                case 1:
                    //Image Capture
                    PageTitle.Content = "Capture a few snapshots of the user";

                    if (OpenWebcam())
                    {
                        bBack.Visibility = Visibility.Visible;
                        gImages.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show("Error: Unable to connect to webcam. Please check that it is connected and available");
                        ChangeState(false);
                    }
                    break;
            }
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            //Discard all changes
            this.DialogResult = false;
            this.Close();
        }

        private void bBack_Click(object sender, RoutedEventArgs e)
        {
            //Handle Current State
            switch(state)
            {
                case 0:
                    //No Back
                    break;

                case 1:
                    //Clear Saved Images
                    imageFiles.Clear();
                    lastFile = "";

                    //Reset Labels
                    Img1.Background = new SolidColorBrush(Colors.Orange);
                    Img2.Background = new SolidColorBrush(Colors.Red);
                    Img3.Background = new SolidColorBrush(Colors.Red);
                    Img4.Background = new SolidColorBrush(Colors.Red);
                    Img5.Background = new SolidColorBrush(Colors.Red);

                    //Close Webcam Feed
                    CloseWebcam();
                    break;
            }
            ChangeState(false);
        }

        private void bNext_Click(object sender, RoutedEventArgs e)
        {
            //Validate Current State
            switch (state)
            {
                case 0:
                    //Name Entry
                    if (eName.Text != "")
                    {
                        //Valid Field
                        eName.Background = new SolidColorBrush(Colors.White);
                        
                        //Update State
                        ChangeState();
                    }
                    else
                        eName.Background = new SolidColorBrush(Colors.Red);
                    break;

                case 1:
                    //Image Captures
                    if (imageFiles.Count == 5)
                    {
                        //I have the required 5 images
                        ChangeState();
                    }                        
                    break;
            }
        }

        private void bCapture_Click(object sender, RoutedEventArgs e)
        {
            if (webCam != null)
            {
                try
                {
                    //Pause Preview
                    isPreview = true;

                    if (!Directory.Exists(@"Faces"))
                        Directory.CreateDirectory("Faces");

                    Guid photoID = Guid.NewGuid();
                    string filePath = @"Faces\" + photoID.ToString() + ".jpg";

                    using (var fstream = new FileStream(filePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(lastFrame));
                        encoder.Save(fstream);
                    }

                    lastFile = filePath;

                    bCapture.Visibility = Visibility.Collapsed;
                    bKeep.Visibility = Visibility.Visible;
                    bDiscard.Visibility = Visibility.Visible;
                }
                catch (Exception)
                {

                }
            }            
        }

        private void bDiscard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Delete last image file
                if (File.Exists(lastFile))
                    File.Delete(lastFile);
            }
            catch (Exception)
            {

            }

            bCapture.Visibility = Visibility.Visible;
            bKeep.Visibility = Visibility.Collapsed;
            bDiscard.Visibility = Visibility.Collapsed;

            isPreview = false;
        }

        private void bKeep_Click(object sender, RoutedEventArgs e)
        {
            if (lastFile != "")
            {
                imageFiles.Add(lastFile);

                //Label Details
                switch (imageFiles.Count)
                {
                    case 1:
                        Img1.Background = new SolidColorBrush(Colors.Green);
                        Img2.Background = new SolidColorBrush(Colors.Orange);
                        break;

                    case 2:
                        Img2.Background = new SolidColorBrush(Colors.Green);
                        Img3.Background = new SolidColorBrush(Colors.Orange);
                        break;

                    case 3:
                        Img3.Background = new SolidColorBrush(Colors.Green);
                        Img4.Background = new SolidColorBrush(Colors.Orange);
                        break;

                    case 4:
                        Img4.Background = new SolidColorBrush(Colors.Green);
                        Img5.Background = new SolidColorBrush(Colors.Orange);
                        break;

                    case 5:
                        Img5.Background = new SolidColorBrush(Colors.Green);
                        break;
                }

                if (imageFiles.Count < 5)
                {
                    bCapture.Visibility = Visibility.Visible;
                    bKeep.Visibility = Visibility.Collapsed;
                    bDiscard.Visibility = Visibility.Collapsed;

                    isPreview = false;
                }
                else
                {
                    bCapture.Visibility = Visibility.Collapsed;
                    bKeep.Visibility = Visibility.Collapsed;
                    bDiscard.Visibility = Visibility.Collapsed;

                    CloseWebcam();

                    bSave.Visibility = Visibility.Visible;
                }                
            }
        }

        private async void bSave_Click(object sender, RoutedEventArgs e)
        {
            Loading l = new Loading { Owner = this };
            l.lMessage.Content = "Saving...";
            l.Show();

            try
            {
                if (person == null)
                {
                    //New User
                    //Add new Person
                    var newPerson = await MainWindow.faceServiceClient.CreatePersonAsync(faceGroupID, eName.Text);
                    if (newPerson != null)
                    {
                        //Person Created
                        //Add Faces to Person
                        foreach (var f in imageFiles)
                        {
                            try
                            {
                                using (Stream imgFile = File.OpenRead(f))
                                {
                                    var addFace = await MainWindow.faceServiceClient.AddPersonFaceAsync(faceGroupID, person.PersonId, imgFile);
                                    if (addFace != null)
                                    {
                                        //Process completed
                                        //Rename image file
                                        File.Move(f, @"Faces\" + addFace.PersistedFaceId.ToString() + ".jpg");
                                    }
                                }

                            }
                            catch (Exception)
                            {
                                //Skip Image and move on
                            }
                        }

                        MainWindow.FaceChangesMade = true;
                    }
                }
                else
                {
                    //Update User
                    if (person.Name != eName.Text)
                    {
                        //Update Person Details
                        await MainWindow.faceServiceClient.UpdatePersonAsync(faceGroupID, person.PersonId, eName.Text);
                    }

                    if (imageFiles.Count > 0)
                    {
                        //Clear Exising Faces
                        foreach(var face in person.PersistedFaceIds)
                        {
                            await MainWindow.faceServiceClient.DeletePersonFaceAsync(faceGroupID, person.PersonId, face);
                        }

                        //Update Images
                        foreach (var f in imageFiles)
                        {
                            try
                            {
                                using (Stream imgFile = File.OpenRead(f))
                                {
                                    var addFace = await MainWindow.faceServiceClient.AddPersonFaceAsync(faceGroupID, person.PersonId, imgFile);
                                    if (addFace != null)
                                    {
                                        //Process completed
                                        //Rename image file
                                        File.Move(f, @"Faces\" + addFace.PersistedFaceId.ToString() + ".jpg");
                                    }
                                }

                            }
                            catch (Exception)
                            {

                            }
                        }

                        MainWindow.FaceChangesMade = true;
                    }
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception)
            {

            }

            l.loaded = true;
        }

        private bool OpenWebcam(int source = 0)
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

                //HACK: FOR RECORDING PURPOSE ONLY
                if (webCamCollection.Count > 1)
                    source = 1;

                webCam = new VideoCaptureDevice(webCamCollection[source].MonikerString);
                webCam.NewFrame += WebCam_NewFrame;
                webCam.Start();
                                
                return true;
            }
            catch (Exception)
            {

            }

            return false;
        }

        private void CloseWebcam()
        {
            if (webCam != null)
            {
                webCam.Stop();
                webCam.NewFrame -= WebCam_NewFrame;
            }

            webCam = null;
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

                Dispatcher.BeginInvoke(new Action(() => {
                    if (!isPreview)
                        eCapture.Source = bi;
                }));
            }
            catch (Exception)
            {

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseWebcam();
        }

        private async void bDelete_Click(object sender, RoutedEventArgs e)
        {
            if (person != null)
            {
                var response = MessageBox.Show("Are you sure you want to delete this User?", "Delete User", MessageBoxButton.YesNo);
                if (response == MessageBoxResult.Yes)
                {
                    await MainWindow.faceServiceClient.DeletePersonAsync(faceGroupID, person.PersonId);

                    MainWindow.FaceChangesMade = true;
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }
    }
}
