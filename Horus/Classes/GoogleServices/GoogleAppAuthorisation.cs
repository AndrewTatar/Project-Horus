using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using File = Google.Apis.Drive.v2.Data.File;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Horus.Classes
{
    public static class GoogleAppAuthorisation
    {
        //WARNING: DO NOT USE THE STUDENT ACCOUNT.

        private static File snapshotsFolder;
        private static UserCredential credential;
        
        public static DriveService _service { get; private set; }

        /// <summary>
        /// This is the worker method that executes when the user clicks the GO button.
        /// It illustrates the workflow that would need to take place in an actual application.
        /// </summary>
        public static async Task AuthorizeAndUpload(Byte[] byteArray, string title)
        {
            try
            {
                MemoryStream stream = new MemoryStream(byteArray);

                // First, create a reference to the Drive service.
                // The CreateAuthenticator method is passed to the service which will use that when it is time to authenticate
                // the calls going to the service.
                if (credential != null)
                {
                    AuthoriseUser();

                    await setFolder(_service, _service.ApplicationName);

                    // Create metaData for a new file
                    File body = new File();
                    body.Title = title;
                    body.Description = App.currentIP;
                    body.MimeType = "image/png";

                    // Set the parent folder.
                    body.Parents = new List<ParentReference>() { new ParentReference() { Id = snapshotsFolder.Id } };

                    try
                    {
                        FilesResource.InsertMediaUpload request = _service.Files.Insert(body, stream, body.MimeType);
                        request.Upload();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public static void BuildCredentails(string storageDirectory)
        {
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets { ClientId = "616661645851-meq5cd830t5kbe98lko4vdcn5mukb9mp.apps.googleusercontent.com", ClientSecret = "1cPabJvr4I8KnSP5oLqhseR0" }, new[] { DriveService.Scope.DriveFile }, "user", CancellationToken.None, new FileDataStore(storageDirectory, true)).Result;
        }

        public static void AuthoriseUser()
        {
            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "HORUS Security",
            });
        }
        
        private static async Task setFolder(DriveService driveService, string folderName)
        {
            var folderListReq = driveService.Files.List();
            folderListReq.Fields = "items/title,items/id";

            // Set query
            folderListReq.Q = "mimeType = 'application/vnd.google-apps.folder' and title ='" + folderName + "' and trashed = false";

            try
            {
                FileList folderList = await folderListReq.ExecuteAsync();

                if (folderList.Items.Count >= 1)
                {
                    // If multiple folders with application name title always choose first one (one may exist in the junk folder?)
                    snapshotsFolder = folderList.Items.First();
                }
                else
                {
                    snapshotsFolder = createDirectory(driveService, folderName, "RMIT", "root");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static File createDirectory(DriveService driveService, string directoryName, string _descriptor, string _parent)
        {
            File NewDirectory = null;

            // Create metaData for a new Directory
            File body = new File();
            body.Title = directoryName;
            body.Description = _descriptor;
            body.MimeType = "application/vnd.google-apps.folder";
            body.Parents = new List<ParentReference>() { new ParentReference() { Id = _parent } };
            
            try
            {
                FilesResource.InsertRequest request = driveService.Files.Insert(body);
                NewDirectory = request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return null;
            }

            return NewDirectory;
        }
    }
}
