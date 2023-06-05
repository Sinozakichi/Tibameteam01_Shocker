namespace Shocker.Models.ViewModels
{
	public class OrderDetailsViewModel
	{
		public int ProductId { get; set; }
		public int? CouponId { get; set; }
		public int Quantity { get; set; }
		public string ProductName { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal? Discount { get; set; }
		public string Currency { get; set; }
	}
}
