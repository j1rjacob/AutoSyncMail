using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using System;

namespace TMFGmail.GmailExtension
{
    public static class Messages
    {
        public static Message GetMessage(this GmailService service, String userId, String messageId)
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
