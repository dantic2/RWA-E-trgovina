using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Order Number")]
        public string OrderNumber => $"#{Id}";

        [Display(Name = "Customer")]
        public string CustomerName { get; set; } = null!;

        [Display(Name = "Customer Email")]
        public string? CustomerEmail { get; set; }

        [Display(Name = "Shipping Address")]
        public string? ShippingAddress { get; set; }

        [Display(Name = "Order Date")]
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = null!;

        [Display(Name = "Items")]
        public List<OrderItemViewModel> OrderItems { get; set; } = new();

        public int TotalItems => OrderItems.Sum(i => i.Quantity);
    }
}