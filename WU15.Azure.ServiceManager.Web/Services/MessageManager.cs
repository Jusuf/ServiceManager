﻿using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WU15.Azure.ServiceManager.Web.Models;

namespace WU15.Azure.ServiceManager.Web.Services
{
    public class MessageManager
    {
        public static void GetServiceticketObjectMessages()
        {
            CloudStorageAccount storageAccount
     = CloudStorageAccount.Parse(
         CloudConfigurationManager.GetSetting("StorageConnectionString"));

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference("myobjectqueue");

            List<ReceivedTicket> receivedTickets = new List<ReceivedTicket>();

            var messages = queue.GetMessages(32, TimeSpan.FromMinutes(2), null, null);

            foreach (var message in messages)
            {
                try
                {
                    //If processing was not possible, delete the message check for unprocesible messages
                    if (message.DequeueCount < 5)
                    {
                        var messageItem = JsonConvert.DeserializeObject<ReceivedTicket>(message.AsString);

                        receivedTickets.Add(messageItem);
                    }
                    else
                    {
                        System.Console.WriteLine("De-queueeing failed");
                    }

                    // Delete the message so that it becomes invisible for other workers
                    queue.DeleteMessage(message);
                }
                catch (Exception e)
                {

                    System.Console.WriteLine(string.Format("An excepted error occured: {0}", e.Message));
                }
            }

            if (receivedTickets.Count > 0)
            {
                ApplicationDbContext context = new ApplicationDbContext();

                foreach (var receivedTicket in receivedTickets)
                {
                    ServiceTicket newTicket = new ServiceTicket()
                    {
                        CreatedDate = receivedTicket.CreatedDate,
                        CustomerEmail = receivedTicket.UserEmail,
                        CustomerTicketId = receivedTicket.Id,
                        CustomerId = receivedTicket.UserId,
                        Description = receivedTicket.Description
                    };
                    context.ServiceTickets.Add(newTicket);

                }

                context.SaveChanges();
            }
        }

        public static void SaveTicketAsWithdrown(string customerTicketId)
        {
            ApplicationDbContext context = new ApplicationDbContext();

            ServiceTicket serviceTicket = new ServiceTicket();

            serviceTicket = context.ServiceTickets.Where(st => st.CustomerTicketId.ToString() == customerTicketId).FirstOrDefault();

            if (serviceTicket != null)
            {
                serviceTicket.TicketIsWithdrawn = true;

                context.Entry(serviceTicket).State = EntityState.Modified;

                context.SaveChanges();
            }

        }

        public static void GetServiceTicketWithdrowmMessages()
        {
            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var topic = "ticketsw";
            var subscription = "withdrownTickets";

            var nameSpaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!nameSpaceManager.TopicExists(topic))
            {
                nameSpaceManager.CreateTopic(topic);
            }

            // Create subscription.
            if (!nameSpaceManager.SubscriptionExists(topic, subscription))
            {
                nameSpaceManager.CreateSubscription(topic, subscription);
            }

            var withdrownTicketsClient = SubscriptionClient.CreateFromConnectionString(connectionString, topic, subscription);

            List<Guid> messageList = new List<Guid>();

            withdrownTicketsClient.OnMessage(message =>
            {
                var messageContent = String.Format(message.GetBody<string>());

                SaveTicketAsWithdrown(messageContent);
                message.Complete();
            });
        }

        public static void SendServiceTicketDoneMessage(Guid? serviceTicketId)
        {
            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var nameSpaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            var topic = "ticketsd";
            var subscription = "doneTickets";

            // Create a topic.
            if (!nameSpaceManager.TopicExists(topic))
            {
                nameSpaceManager.CreateTopic(topic);
            }

            // Create subscription.
            if (!nameSpaceManager.SubscriptionExists(topic, subscription))
            {
                var description = nameSpaceManager.CreateSubscription(topic, subscription);
                nameSpaceManager.CreateSubscription(description);
            }

            var doneTicketsClient = TopicClient.CreateFromConnectionString(connectionString, topic);

            var messageText = "";

            if (serviceTicketId != null)
            {
                messageText = serviceTicketId.ToString();
            }

            if (messageText != String.Empty)
            {
                var message = new BrokeredMessage(messageText);
                doneTicketsClient.Send(message);
            }
        }

        public static void SendServiceTicketAddressingMessage(Guid? serviceTicketId)
        {
            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var nameSpaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            var topic = "ticketsa";
            var subscription = "addressedTickets";

            // Create a topic.
            if (!nameSpaceManager.TopicExists(topic))
            {
                nameSpaceManager.CreateTopic(topic);
            }

            // Create subscription.
            if (!nameSpaceManager.SubscriptionExists(topic, subscription))
            {
                var description = nameSpaceManager.CreateSubscription(topic, subscription);
                nameSpaceManager.CreateSubscription(description);
            }

            TopicClient addressedTicketsClient = TopicClient.CreateFromConnectionString(connectionString, topic);

            var messageText = "";

            if (serviceTicketId != null)
            {
                messageText = serviceTicketId.ToString();
            }

            if (messageText != String.Empty)
            {
                var message = new BrokeredMessage(messageText);
                addressedTicketsClient.Send(message);
            }
        }
    }
}