using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RwOrders.Models
{
    public class Product
    {
        public Product()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        public int ID { get; set; }

        [Index(IsUnique = true)]
        [StringLength(20)]
        [Display(Name = "Stock Code")]
        public string StockCode { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        public bool Inactive { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<ConsignmentItem> ConsignmentItems { get; set; }
    }
}