using Microsoft.AspNetCore.Mvc;
using Shocker.Areas.Admin.Models.ViewModels;
using Shocker.Models;

namespace Shocker.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly db_a98a02_thm101team1001Context _context;
        public CouponController(db_a98a02_thm101team1001Context context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> DisaplayCoupon()
        {
            var DC = _context.Coupons
                .Select(x => new
                {
                    CouponId = x.CouponId,
                    ExpirationDate = x.ExpirationDate,
                    HolderAccount = x.HolderAccount,
                    ProductCategoryId = x.ProductCategoryId,
                    Discount = x.Discount,
                    Status = x.Status,
                    PublisherAccount = x.PublisherAccount,
                });
            return Json(DC);
        }
        [HttpPost]
        public IActionResult CreateCoupon([FromBody] CreateCouponsViewModel cvm)
        {
            if (cvm!=null && ModelState.IsValid)
            {
                try
                {
                    for (var i = 1; i <= cvm.Amount; i++)
                    {
                        Coupons coupons = new Coupons()
                        {
                            //CouponId = 0,
                            ExpirationDate = cvm.ExpirationDate,
                            HolderAccount = cvm.HolderAccount,
                            ProductCategoryId = cvm.ProductCategoryId,
                            Discount = cvm.Discount,
                            Status = "c0",
                            PublisherAccount = cvm.PublisherAccount,
                        };
                        _context.Coupons.Add(coupons);
                        _context.SaveChanges();

                    };
                    return Json(new { message = "建立成功" });
                }

                catch (Exception ex)
                {
                    return Json(new { Error = ex.Message });
                }
            }
            return Json(new {Message="格式錯誤"});
        }
            
            [HttpPost]
    public JsonResult FilterCoupon([FromBody] CouponsViewModels cvm)
    {

        return Json(_context.Coupons.Where(x =>
                  x.HolderAccount.Contains(cvm.HolderAccount) ||
                  x.Discount == cvm.Discount ||
                  x.StatusNavigation.StatusName.Contains(cvm.StatusName) ||
                  x.ProductCategory.CategoryName.Contains(cvm.CategoryName)
                  ).Select(c => new
                  {
                      PublisherAccount = c.PublisherAccount,
                      HolderAccount = c.HolderAccount,
                      ProductCategoryId = c.ProductCategoryId,
                      Discount = c.Discount,
                      StatusName = c.StatusNavigation.StatusName,
                      CategoryName = c.ProductCategory.CategoryName,
                      ExpirationDate = c.ExpirationDate,
                      Status = c.Status,
                  }
            ));

    }
}
}
