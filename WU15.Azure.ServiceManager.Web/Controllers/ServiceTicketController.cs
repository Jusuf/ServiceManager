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

            return View(db.ServiceTickets.ToList());
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
                

                serviceTicket.Id = Guid.NewGuid();
                serviceTicket.CreatedDate = DateTime.Now;
                
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
