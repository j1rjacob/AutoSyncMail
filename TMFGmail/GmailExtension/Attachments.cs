using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace TMFGmail.GmailExtension
{
    public static class Attachments
    {
        public static bool GetAttachments(this GmailService service, String userId, String messageId, String outputDir)
        {
            bool GAttach = false;
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

                        if (part.Filename.Contains("RDS"))
                        {
                            byte[] data = Convert.FromBase64String(attachData);
                            File.WriteAllBytes(Path.Combine(outputDir, part.Filename), data);
                            Console.WriteLine("Download Successfully " + part.Filename);
                            GAttach = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                GAttach = false;
                //Console.WriteLine("An error occurred: " + e.Message);
            }
            return GAttach;
        }

    }
}
