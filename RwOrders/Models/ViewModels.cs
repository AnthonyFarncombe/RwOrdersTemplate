using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RwOrders.Models
{
    public class EventDayViewModel
    {
        public int ID { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        public string Name { get; set; }

        [Display(Name = "Event Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EventDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal Total { get; set; }
    }

    public class OrderViewModel
    {
        public OrderViewModel()
        {
            OrderItems = new List<OrderItemViewModel>();
        }

        public int ID { get; set; }

        [Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required]
        public string Locality { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Account No")]
        public string AccountNo { get; set; }

        [Display(Name = "Email Order?")]
        public bool EmailConfirmation { get; set; }

        public string Notes { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod? PaymentMethod { get; set; }

        [Display(Name = "Vouchers Amount")]
        [DataType(DataType.Currency)]
        public decimal Vouchers { get; set; }

        public IList<OrderItemViewModel> OrderItems { get; set; }
    }

    public class OrderItemViewModel
    {
        public int ID { get; set; }

        public int OrderID { get; set; }

        public int ProductID { get; set; }

        public string StockCode { get; set; }

        public string Description { get; set; }

        public int Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }
    }

    public class ProductOrderViewModel
    {
        public int ID { get; set; }

        [Display(Name = "Event Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EventDate { get; set; }

        [Display(Name = "Event Name")]
        public string EventName { get; set; }

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        public int Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }
    }

    public class UserVisit
    {
        public string UserID { get; set; }
        public DateTime VisitDate { get; set; }
        public string Url { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public int Count { get; set; } = 1;
    }
}