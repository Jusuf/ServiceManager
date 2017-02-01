using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WU15.Azure.ServiceManager.Web.Models
{
    public class ReceivedTicket
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public bool Done { get; set; }

        public DateTime CreatedDate { get; set; }

        public string DoneDate { get; set; }

        public Guid UserId { get; set; }

        public string UserEmail { get; set; }
    }
}