using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RwOrders.Models
{
    public class Consignment
    {
        public Consignment()
        {
            ConsignmentItems = new HashSet<ConsignmentItem>();
        }

        [Key]
        [Display(Name = "Number")]
        [DisplayFormat(DataFormatString = "{0:'C'0}")]
        public int ID { get; set; }

        //[Required]
        [StringLength(128)]
        [Display(Name = "Sales Person")]
        public string SalesPersonID { get; set; }

        [Required]
        [Display(Name = "Dispatch Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DispatchDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Campus { get; set; }

        [Required]
        [StringLength(100)]
        public string Locality { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }

        [Required]
        [StringLength(50)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Return By")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ReturnBy { get; set; }

        [Display(Name = "Number Of Sales")]
        public int? NumberOfSales { get; set; }

        [StringLength(100)]
        [Display(Name = "Sale Dates")]
        public string SaleDates { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Attention Of")]
        public string AttentionOf { get; set; }

        [StringLength(50)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [StringLength(50)]
        [Display(Name = "Street 1")]
        public string Street1 { get; set; }

        [StringLength(50)]
        [Display(Name = "Street 2")]
        public string Street2 { get; set; }

        [StringLength(50)]
        public string Town { get; set; }

        [StringLength(50)]
        public string County { get; set; }

        [StringLength(50)]
        public string Postcode { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        public virtual ApplicationUser SalesPerson { get; set; }
        public virtual ICollection<ConsignmentItem> ConsignmentItems { get; set; }
    }
}