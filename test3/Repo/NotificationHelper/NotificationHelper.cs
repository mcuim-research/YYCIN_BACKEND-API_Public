using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;

namespace test3.Repo.NotificationHelper
{
    public class NotificationHelper
    {
        public async void sendNotification(string token, string title, string body)
        {
            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Token = token
            };
            var messaging = FirebaseMessaging.DefaultInstance;
            var response = await messaging.SendAsync(message);

            if(!string.IsNullOrEmpty(response)){
                Console.WriteLine("Successfully sent message: " + response);
            }
            else{
                Console.WriteLine("Failed to send message");
            }
        }
    }
}