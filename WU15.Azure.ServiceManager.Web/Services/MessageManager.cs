using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
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

            var topic = "tickets";
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

            var client = SubscriptionClient.CreateFromConnectionString(connectionString, topic, subscription);

            List<Guid> messageList = new List<Guid>();

            client.OnMessage(message =>
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

            var topic = "tickets";
            var subscription = "doneTickets";

            // Create a topic.
            if (!nameSpaceManager.TopicExists(topic))
            {
                nameSpaceManager.CreateTopic(topic);
            }

            // Create subscription.
            if (!nameSpaceManager.SubscriptionExists(topic, subscription))
            {
                nameSpaceManager.CreateSubscription(topic, subscription);
            }

            TopicClient client = TopicClient.CreateFromConnectionString(connectionString, topic);

            var messageText = "";

            if (serviceTicketId != null)
            {
                messageText = serviceTicketId.ToString();
            }

            if (messageText != String.Empty)
            {
                var message = new BrokeredMessage(messageText);
                client.Send(message);
            }
        }

        public static void SendServiceTicketAddressingMessage(Guid? serviceTicketId)
        {
            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var nameSpaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            var topic = "tickets";
            var subscription = "addressedTickets";

            // Create a topic.
            if (!nameSpaceManager.TopicExists(topic))
            {
                nameSpaceManager.CreateTopic(topic);
            }

            // Create subscription.
            if (!nameSpaceManager.SubscriptionExists(topic, subscription))
            {
                nameSpaceManager.CreateSubscription(topic, subscription);
            }

            TopicClient client = TopicClient.CreateFromConnectionString(connectionString, topic);

            var messageText = "";

            if (serviceTicketId != null)
            {
                messageText = serviceTicketId.ToString();
            }

            if (messageText != String.Empty)
            {
                var message = new BrokeredMessage(messageText);
                client.Send(message);
            }
        }
    }
}