namespace Shocker.Models.ViewModels
{
    public class OrdersViewModel
    {
        public string BuyerAccount { get; set; }
        public string Address { get; set; }
        public string BuyerPhone { get; set; }
        public string PayMethod { get; set; }
        public string BuyerName { get; set; }
		public ICollection<OrderDetailsViewModel> OrderDetails { get; set; }

	}
}
