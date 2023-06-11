namespace Shocker.Models.ViewModels
{
	public class PaymentAsyncViewModel
	{
		public int OrderId { get; set; }
		public string PayType { get; set; }
		public int Amt { get; set; }
		public string Email { get; set; }
		public string? ItemDesc { get; set; }
		public string? OrderComment { get; set; }
	}
}
