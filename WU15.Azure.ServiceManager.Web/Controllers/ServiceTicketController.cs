using Microsoft.AspNet.Identity;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WU15.Azure.ServiceManager.Web.Models;
using WU15.Azure.ServiceManager.Web.Models.ViewModels;

namespace WU15.Azure.ServiceManager.Web.Controllers
{
    [Authorize]
    public class ServiceTicketController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public void SaveTicketAsWithdrown(string ticketId)
        {
            ApplicationDbContext context = new ApplicationDbContext();

            ServiceTicket serviceTicket = new ServiceTicket();

            serviceTicket = context.ServiceTickets.Where(st => st.CustomerTicketId.ToString() == ticketId).FirstOrDefault();

            if (serviceTicket != null)
            {
                serviceTicket.TicketIsWithdrawn = true;

                context.Entry(serviceTicket).State = EntityState.Modified;

                context.SaveChanges();
            }

        }

        // GET: ServiceTickets
        public ActionResult Index()
        {
            var conectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var topic = "tickets";
            var subscription = "addressedTickets";

            var client = SubscriptionClient.CreateFromConnectionString(conectionString, topic, subscription);

            List<Guid> messageList = new List<Guid>();

            client.OnMessage(message =>
            {
                var messageContent = String.Format(message.GetBody<string>());

                SaveTicketAsWithdrown(messageContent);
                message.Complete();
            });

            var userId = User.Identity.GetUserId();

            var serviceTickets = db.ServiceTickets.Where(st => st.ResponsibleUser.Id == userId && st.TicketIsWithdrawn == false).ToList();

            List<ServiceTicketViewModel> tickets = new List<ServiceTicketViewModel>();

            if (serviceTickets.Count > 0)
            {
                foreach (var serviceTicket in serviceTickets)
                {
                    ServiceTicketViewModel ticket = new ServiceTicketViewModel()
                    {
                        Id = serviceTicket.Id,
                        Description = serviceTicket.Description,
                        CreatedDate = serviceTicket.CreatedDate,
                        Done = serviceTicket.Done,
                        DoneDate = serviceTicket.DoneDate.ToString() ?? String.Empty,
                        CustomerEmail = serviceTicket.CustomerEmail
                    };

                    tickets.Add(ticket);
                }
            }

            return View(tickets);
        }



        // GET: ServiceTickets/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceTicket serviceTicket = db.ServiceTickets.Find(id);
            if (serviceTicket == null)
            {
                return HttpNotFound();
            }
            return View(serviceTicket);
        }

        // GET: ServiceTickets/Create
        public ActionResult Create()
        {
            ServiceTicketViewModel model = new ServiceTicketViewModel();

            return View(model);
        }

        // POST: ServiceTickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Description")] ServiceTicket serviceTicket)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());

                serviceTicket.Id = Guid.NewGuid();
                serviceTicket.CreatedDate = DateTime.Now;
                serviceTicket.ResponsibleUser = user;

                db.ServiceTickets.Add(serviceTicket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(serviceTicket);
        }

        // GET: ServiceTickets/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceTicket serviceTicket = db.ServiceTickets.Find(id);
            if (serviceTicket == null)
            {
                return HttpNotFound();
            }
            return View(serviceTicket);
        }

        // POST: ServiceTickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,Done,TicketIsWithdrawn,CreatedDate,DoneDate,CustomerTicketId,CustomerId,CustomerEmail")] ServiceTicket serviceTicket)
        {
            if (ModelState.IsValid)
            {
                db.Entry(serviceTicket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(serviceTicket);
        }

        // GET: ServiceTickets/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceTicket serviceTicket = db.ServiceTickets.Find(id);
            if (serviceTicket == null)
            {
                return HttpNotFound();
            }
            return View(serviceTicket);
        }

        // POST: ServiceTickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            ServiceTicket serviceTicket = db.ServiceTickets.Find(id);
            db.ServiceTickets.Remove(serviceTicket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult NewTickets()
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
                    db.ServiceTickets.Add(newTicket);

                }

                db.SaveChanges();
            }

            // Get tickets
            var userId = User.Identity.GetUserId();

            var serviceTickets = db.ServiceTickets.Where(st => st.ResponsibleUser.Id == null && st.TicketIsWithdrawn == false).ToList();

            List<ServiceTicketViewModel> tickets = new List<ServiceTicketViewModel>();

            if (serviceTickets.Count > 0)
            {
                foreach (var serviceTicket in serviceTickets)
                {
                    ServiceTicketViewModel ticket = new ServiceTicketViewModel()
                    {
                        Id = serviceTicket.Id,
                        Description = serviceTicket.Description,
                        CreatedDate = serviceTicket.CreatedDate,
                        Done = serviceTicket.Done,
                        DoneDate = serviceTicket.DoneDate.ToString() ?? String.Empty,
                        CustomerEmail = serviceTicket.CustomerEmail
                    };

                    tickets.Add(ticket);
                }
            }

            return View(tickets);
        }

        // GET: ServiceTickets/Edit/5
        public ActionResult EditNewTicket(Guid? id)
        {


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ServiceTicket serviceTicket = db.ServiceTickets.Find(id);

            ServiceTicketViewModel serviceTicketModel = new ServiceTicketViewModel()
            {
                Id = serviceTicket.Id,
                Description = serviceTicket.Description,
                CreatedDate = serviceTicket.CreatedDate,
                Done = serviceTicket.Done,
                DoneDate = serviceTicket.DoneDate.ToString() ?? String.Empty,
                CustomerEmail = serviceTicket.CustomerEmail,
                ResponsibleUser = serviceTicket.ResponsibleUser,
                Employees = db.Users.ToList()
            };

            if (serviceTicket == null)
            {
                return HttpNotFound();
            }
            return View(serviceTicketModel);
        }

        // POST: ServiceTickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNewTicket(ServiceTicketViewModel model)
        {
            if (ModelState.IsValid)
            {
                ServiceTicket serviceTicket = db.ServiceTickets.Find(model.Id);

                if (model.ResponsibleUser.Id != String.Empty)
                {
                    ApplicationUser selectedUser = db.Users.FirstOrDefault(x => x.Id == model.ResponsibleUser.Id);

                    serviceTicket.ResponsibleUser = selectedUser;
                }

                serviceTicket.Description = model.Description;


                db.Entry(serviceTicket).State = EntityState.Modified;

                db.SaveChanges();

                // Servicebus message

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
                 
                if (serviceTicket.CustomerTicketId != null)
                {
                    messageText = serviceTicket.CustomerTicketId.ToString();
                }

                if (messageText != String.Empty)
                {
                    var message = new BrokeredMessage(messageText);
                    client.Send(message);
                }

                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: ServiceTicketsHistory
        public ActionResult ServiceTicketHistory()
        {
            var userId = User.Identity.GetUserId();

            var serviceTickets = db.ServiceTickets.Where(st => st.ResponsibleUser.Id == userId
                                                            && st.Done == true).ToList();

            List<ServiceTicketViewModel> tickets = new List<ServiceTicketViewModel>();

            if (serviceTickets.Count > 0)
            {
                foreach (var serviceTicket in serviceTickets)
                {
                    ServiceTicketViewModel ticket = new ServiceTicketViewModel()
                    {
                        Id = serviceTicket.Id,
                        Description = serviceTicket.Description,
                        CreatedDate = serviceTicket.CreatedDate,
                        Done = serviceTicket.Done,
                        DoneDate = serviceTicket.DoneDate.ToString() ?? String.Empty,
                        CustomerEmail = serviceTicket.CustomerEmail
                    };

                    tickets.Add(ticket);
                }
            }

            return View(tickets);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
