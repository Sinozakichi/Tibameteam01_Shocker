namespace Shocker.Models.ViewModels
{
	public class PaymentAsyncViewModel
	{
		public string payType { get; set; }
		public int amount { get; set; }
		public string Email { get; set; }
		public string? ItemDesc { get; set; }
		public string? OrderComment { get; set; }
	}
}
