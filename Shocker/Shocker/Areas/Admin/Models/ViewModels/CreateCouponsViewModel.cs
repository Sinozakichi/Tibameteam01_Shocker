namespace Shocker.Areas.Admin.Models.ViewModels
{
    public class CreateCouponsViewModel
    {
        public DateTime ExpirationDate { get; set; }
        public string HolderAccount { get; set; }
        public int ProductCategoryId { get; set; }
        //[Required(ErrorMessage="請輸入折扣小於1")]
        public decimal Discount { get; set; }
        public string PublisherAccount { get; set; }
        public int Amount { get; set; }
    }
}
