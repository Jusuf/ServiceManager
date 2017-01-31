using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WU15.Azure.ServiceManager.Web.Models
{
    public class ServiceTicket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Description { get; set; }

        public bool Done { get; set; }

        public bool TicketIsWithdrawn { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? DoneDate { get; set; }

        public Guid CustomerTicketId { get; set; }

        public Guid CustomerId { get; set; }

        public string CustomerEmail { get; set; }

        public ApplicationUser ResponsibleUser { get; set; }
    }
}