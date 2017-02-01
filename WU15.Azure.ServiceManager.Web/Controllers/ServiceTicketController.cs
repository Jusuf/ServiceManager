using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WU15.Azure.ServiceManager.Web.Models;

namespace WU15.Azure.ServiceManager.Web.Controllers
{
    [Authorize]
    public class ServiceTicketController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ServiceTickets
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var serviceTickets = db.ServiceTickets.Where(st => st.ResponsibleUser.Id == userId).ToList();

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
