using System.ComponentModel.DataAnnotations;

namespace RwOrders.Models
{
    public class ConsignmentItem
    {
        public int ID { get; set; }

        public int ConsignmentID { get; set; }

        public int ProductID { get; set; }

        [StringLength(20)]
        [Display(Name = "Stock Code")]
        public string StockCode { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        public virtual Consignment Consignment { get; set; }
        public virtual Product Product { get; set; }
    }
}