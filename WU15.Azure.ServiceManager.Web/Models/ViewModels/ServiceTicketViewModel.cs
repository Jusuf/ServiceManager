using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WU15.Azure.ServiceManager.Web.Models.ViewModels
{
    public class ServiceTicketViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Du måste ange beskrivning.")]
        [DisplayName("Felbeskrivning")]
        [StringLength(200, ErrorMessage = "Felet måste beskrivas.", MinimumLength = 1)]
        public string Description { get; set; }

        [DisplayName("Klar")]
        public bool Done { get; set; }

        [DisplayName("Återkallad")]
        public bool Withdrown { get; set; }

        [DisplayName("Skapad")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Klardatum")]
        public string DoneDate { get; set; }

        [DisplayName("Kund e-post")]
        public string CustomerEmail { get; set; }

        [DisplayName("Ansvarig")]
        public ApplicationUser ResponsibleUser { get; set; }

        [DisplayName("Välj ansvarig")]
        public List<ApplicationUser> Employees { get; set; }       
    }
}