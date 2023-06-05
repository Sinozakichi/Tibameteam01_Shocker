namespace Shocker.Areas.Admin.Models.ViewModels
{
    public class ActivCouponViewModel
    {
       
        public DateTime ExpirationDate { get; set; }
        public string HolderAccount { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Discount { get; set; }
        public string Status { get; set; }
        public string PublisherAccount { get; set; }
        public string Id { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}
