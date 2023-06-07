using Microsoft.Build.Framework;

namespace Shocker.Areas.Admin.Models.ViewModels
{
    public class CouponsViewModels
    {
        //public Coupons()
        //{
        //    OrderDetails = new HashSet<OrderDetails>();
        //}

        public string CouponId { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }
        [Required]
        public string HolderAccount { get; set; }
        [Required]
        public int ProductCategoryId { get; set; }
        public string CategoryName { get; set; }
        [Required]
        public decimal Discount { get; set; }
        public string Status { get; set; }
        public string StatusName { get; set; }
        public string PublisherAccount { get; set; }
    }
}
