using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RwOrders.Models
{
    public class EventDay
    {
        public int ID { get; set; }

        [StringLength(128)]
        [Display(Name = "Created By")]
        public string CreatedByID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Event Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Event Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EventDate { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}