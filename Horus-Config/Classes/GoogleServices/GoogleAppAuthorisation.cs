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

namespace Horus_Config
{
    public static class GoogleAppAuthorisation
    {
        //WARNING: DO NOT USE THE STUDENT ACCOUNT.
        private static File snapshotsFolder;
        private static UserCredential credential;
        public static DriveService _service { get; private set; }
        
        public static async void BuildCredentails(string storageDirectory)
        {
            ClientSecrets cs = new ClientSecrets { ClientId = "616661645851-meq5cd830t5kbe98lko4vdcn5mukb9mp.apps.googleusercontent.com", ClientSecret = "1cPabJvr4I8KnSP5oLqhseR0" };
            string[] scope = new string[] { DriveService.Scope.DriveFile };

            var creds = await GoogleWebAuthorizationBroker.AuthorizeAsync(cs, scope, "user", CancellationToken.None, new FileDataStore(storageDirectory, true));

            credential = creds;
        }

        public static async void AuthoriseUser()
        {
            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "HORUS Security",
            });

            //Create new Folder if required
            await setFolder(_service, _service.ApplicationName);
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
