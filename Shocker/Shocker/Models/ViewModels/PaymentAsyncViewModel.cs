namespace Shocker.Models.ViewModels
{
	public class PaymentAsyncViewModel
	{
		public int orderId { get; set; }
		public string payType { get; set; }
		public int amount { get; set; }
		public string Email { get; set; }
		//public string? ItemDesc { get; set; }
		//public string? OrderComment { get; set; }
	}
}
