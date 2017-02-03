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
using WU15.Azure.ServiceManager.Web.Services;

namespace WU15.Azure.ServiceManager.Web.Controllers
{
    [Authorize]
    public class ServiceTicketController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ServiceTickets
        public ActionResult Index()
        {
            MessageManager.GetServiceTicketWithdrowmMessages();

            var userId = User.Identity.GetUserId();

            var serviceTickets = db.ServiceTickets.Include("ResponsibleUser").Where(st => st.ResponsibleUser.Id.ToString() == userId && st.TicketIsWithdrawn == false && st.Done == false).ToList();

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
        public ActionResult Edit(ServiceTicket serviceTicket)
        {
            
            //var ticketBeforeUpdate = db.ServiceTickets.Find(serviceTicket.Id);

            if (ModelState.IsValid)
            {
                //db.Entry(serviceTicket).State = EntityState.Modified;
                ServiceTicket sticket = db.ServiceTickets.Where(st => st.Id == serviceTicket.Id).First();
                sticket.DoneDate = serviceTicket.DoneDate ?? DateTime.Now;
                sticket.Done = serviceTicket.Done;

                db.SaveChanges();

                if (serviceTicket.Done)
                {
                    MessageManager.SendServiceTicketDoneMessage(serviceTicket.CustomerTicketId);
                }

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
       
        public ActionResult DeleteConfirmed(Guid id)
        {
            ServiceTicket serviceTicket = db.ServiceTickets.Find(id);
            db.ServiceTickets.Remove(serviceTicket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult NewTickets()
        {
            MessageManager.GetServiceticketObjectMessages();

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

                MessageManager.SendServiceTicketAddressingMessage(serviceTicket.CustomerTicketId);

                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: ServiceTicketsHistory
        public ActionResult ServiceTicketHistory()
        {
            var userId = User.Identity.GetUserId();

            var serviceTickets = db.ServiceTickets.Where(st => st.ResponsibleUser.Id == userId
                                                            && st.Done == true || st.TicketIsWithdrawn == true).ToList();

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
                        CustomerEmail = serviceTicket.CustomerEmail,
                        Withdrown = serviceTicket.TicketIsWithdrawn
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
