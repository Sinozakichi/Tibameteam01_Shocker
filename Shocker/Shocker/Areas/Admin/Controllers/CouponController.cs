using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Shocker.Areas.Admin.Models.ViewModels;
using Shocker.Models;
using System.Security.Claims;
using System.Security.Principal;

namespace Shocker.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly db_a98a02_thm101team1001Context _context;
        private readonly string loginAccount = "User2";
        public CouponController(db_a98a02_thm101team1001Context context)
        {
            _context = context;
        }
        
        [Authorize(Roles = "admin")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> DisaplayCoupon()
        {
            var Account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            if (Account == null) return Json(new { Login = false, Message = "請先登入" });

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
        [Authorize(Roles = "admin")]
        public IActionResult CreateCoupon([FromBody] CreateCouponsViewModel cvm)
        {
            var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            if (account == null) return Json(new { Login = false, Message = "請先登入" });

            var Admin = _context.Coupons.AsNoTracking().FirstOrDefault(x => x.PublisherAccount == account.Value);

            if (ModelState.IsValid)
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
                            PublisherAccount = Admin.PublisherAccount,
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
            var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            if (account == null) return Json(new { Login = false, Message = "請先登入" });

            return Json(_context.Coupons.Where(x =>
                  x.HolderAccount.Contains(cvm.HolderAccount) ||
                  x.Discount == cvm.Discount ||
                  x.StatusNavigation.StatusName.Contains(cvm.StatusName) ||
                  x.ProductCategory.CategoryName.Contains(cvm.CategoryName)||
                  x.PublisherAccount.Contains(cvm.PublisherAccount)
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
        [HttpPost]
        public IActionResult DisplayUser()
        {
            var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            if (account == null) return Json(new { Login = false, Message = "請先登入" });
            var cu =  _context.Users
                .Select(x => new
                {
                    holderAccount = x.Id
                });
            return Json(cu);
        }
        
        public JsonResult CreateHBDCoupon()
        {
            var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            if (account == null) return Json(new { Login = false, Message = "請先登入" });

            var Admin = _context.Coupons.AsNoTracking().FirstOrDefault(x => x.PublisherAccount == account.Value);

            var now  = DateTime.Now;
            var allUser = _context.Users.Where(x=>x.BirthDate!=null).ToList();
            var MatchBD =allUser.Where(x => x.BirthDate.Value.Month == now.Month && x.BirthDate.Value.Day == now.Day);
            if (MatchBD != null)
            {
                foreach (var a in MatchBD)
                {
                    if (_context.Coupons.Any(x => x.HolderAccount == a.Id)) //Any 判斷true/false
                    {
                        return Json(new { Message = "今日份已重複" });
                    }
                    else 
                    {
                        Coupons c1 = new Coupons();
                        c1.Discount = 0.5M;
                        c1.Status = "c0";
                        c1.ExpirationDate = DateTime.Now.AddDays(30);
                        c1.PublisherAccount = Admin.PublisherAccount;
                        c1.HolderAccount = a.Id;
                        c1.ProductCategoryId = 9;
                        _context.Coupons.Add(c1);
                        _context.SaveChanges();
                        return Json(new { Message = "成功送出生日優惠券" });
                    }
                };
            };

            return Json(new {Message="今天沒人生日唷" });
         }

    }
}
