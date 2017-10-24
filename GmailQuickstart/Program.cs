using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using SimpleAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GmailQuickstart
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/gmail-dotnet-quickstart.json
        static string[] Scopes = { GmailService.Scope.GmailReadonly }; //View your emails messages and settings
        static string ApplicationName = "TMF GMail";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/gmail-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                //Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Gmail API service.
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

            // List labels.
            //IList<Label> labels = request.Execute().Labels;
            //Console.WriteLine("Labels:");
            //if (labels != null && labels.Count > 0)
            //{
            //    foreach (var labelItem in labels)
            //    {
            //        Console.WriteLine("{0}", labelItem.Name);
            //    }
            //}
            //else
            //{
            //    Console.WriteLine("No labels found.");
            //}

            var messages = ListMessages(service, "me", "CAS Gateway has:attachment newer_than:2d");
            Console.WriteLine("Messages:");

            if (messages != null && messages.Count > 0)
            {
                foreach (var message in messages)
                {
                    Message msg=GetMessage(service, "me", message.Id);
                   
                    foreach (var head in msg.Payload.Headers)
                    {
                        //if (head.Name == "Date")
                        //{
                        //    Console.Write(head.Value);
                        //    Console.WriteLine();

                        //}
                        if (head.Name == "Subject")
                        {
                            var root = @"F:\TMFRoot\";
                            if (!Directory.Exists(root + head.Value.GetSubject()))
                            {
                                //Directory.CreateDirectory(root);
                                Directory.CreateDirectory(root + head.Value.GetSubject());
                            }
                            //Console.Write(head.Value.GetSubject());
                            //Console.WriteLine();
                           
                            try
                            {
                                GetAttachments(service, "me", message.Id, root + head.Value.GetSubject());
                            }
                            catch (Exception e)
                            {
                                log.Info(e);
                                Console.WriteLine(e);
                                throw;
                            }
                            var oks = "Downloaded "+ message.Id;
                            //Console.WriteLine(oks);
                            log.Info(oks);
                            Debug.WriteLine(oks);
                            string rootFolderPath = root + head.Value.GetSubject();
                            string filesToDelete = @"*RAW*.csv";   // Only delete RAW
                            string[] fileList = Directory.GetFiles(rootFolderPath, filesToDelete);
                            foreach (string file in fileList)
                            {
                                Debug.WriteLine(file + " was deleted");
                                File.Delete(file);
                            }
                        }
                    }
                }
                Console.WriteLine("Complete");
                //foreach (var message in messages)
                //{
                    //Console.WriteLine("{0}", message.Id);
                    //var dirPath = AppDomain.CurrentDomain.BaseDirectory;
                    //GetAttachments(service, "me", message.Id, dirPath);
                //}

            }
            else
            {
                Console.WriteLine("No message found.");
            }
            Console.Read();
        }

        static List<Message> ListMessages(GmailService service, String userId, String query)
        {
            List<Message> result = new List<Message>();
            UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List(userId);
            request.Q = query;

            do
            {
                try
                {
                    ListMessagesResponse response = request.Execute();
                    result.AddRange(response.Messages);
                    request.PageToken = response.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return result;
        }

        static void GetAttachments(GmailService service, String userId, String messageId, String outputDir)
        {
            try
            {
                Message message = service.Users.Messages.Get(userId, messageId).Execute();
                IList<MessagePart> parts = message.Payload.Parts;
                foreach (MessagePart part in parts)
                {
                    if (!String.IsNullOrEmpty(part.Filename))
                    {
                        String attId = part.Body.AttachmentId;
                        MessagePartBody attachPart = service.Users.Messages.Attachments.Get(userId, messageId, attId).Execute();

                        // Converting from RFC 4648 base64 to base64url encoding
                        // see http://en.wikipedia.org/wiki/Base64#Implementations_and_history
                        String attachData = attachPart.Data.Replace('-', '+');
                        attachData = attachData.Replace('_', '/');

                        byte[] data = Convert.FromBase64String(attachData);
                        File.WriteAllBytes(Path.Combine(outputDir, part.Filename), data);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }

        static Message GetMessage(GmailService service, String userId, String messageId)
        {
            try
            {
                return service.Users.Messages.Get(userId, messageId).Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            return null;
        }
    }
}
