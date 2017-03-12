using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RwOrders.Models
{
    public enum PaymentMethod
    {
        Cash,
        Cheque,
        Card,
        VouchersOnly,
        Account
    }

    public class Order
    {
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        public int ID { get; set; }

        public int EventDayID { get; set; }

        [StringLength(128)]
        [Display(Name = "Taken By")]
        public string TakenByID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(50)]
        public string Locality { get; set; }

        [StringLength(50)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [StringLength(50)]
        [Display(Name = "Account No")]
        public string AccountNo { get; set; }

        public string Notes { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod? PaymentMethod { get; set; }

        [Display(Name = "Vouchers Amount")]
        [DataType(DataType.Currency)]
        public decimal Vouchers { get; set; }

        public virtual EventDay EventDay { get; set; }
        public virtual ApplicationUser TakenBy { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}