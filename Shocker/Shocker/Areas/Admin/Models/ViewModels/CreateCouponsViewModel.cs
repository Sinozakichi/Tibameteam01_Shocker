using System.ComponentModel.DataAnnotations;

namespace Shocker.Areas.Admin.Models.ViewModels
{
    public class CreateCouponsViewModel 
    {
        //[DataType(DataType.Date)]
        //[Required(ErrorMessage = "請選擇到期日")]
        public DateTime ExpirationDate { get; set; }

        [Required(ErrorMessage = "請輸入使用者帳號")]
        [MaxLength(50)]
        public string HolderAccount { get; set; }
        [Required(ErrorMessage ="請選擇產品類別")]
        public int ProductCategoryId { get; set; }
        
        [Required(ErrorMessage="請輸入數字0~1")]
        public decimal Discount { get; set; }
        //public string PublisherAccount { get; set; }

        [Required(ErrorMessage = "請輸入張數範圍1~20張")]
        [Range(1,20)]
        public int Amount { get; set; }
    }
}
