using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Shocker.Models;
using Shocker.Models.ViewModels;

namespace Shocker.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly db_a98a02_thm101team1001Context _context;
        private readonly string loginAccount = "User2";
        public ShoppingCartController(db_a98a02_thm101team1001Context context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            int productId = 10;
            var products = _context.Products.Where(x => x.ProductId==productId).OrderByDescending(m => m.ProductId);
            return View(products);
        }

        public IActionResult Product()
        {
              return View();
        }
        [HttpPost]
        public JsonResult GetProduct()
        {
            int productId = 2;
            var products = from x in _context.OrderDetails.Where(x => x.ProductId == productId)
                           select new
                           {
                               ProductId = x.ProductId,
                               ProductName = x.ProductName,
                               UnitPrice = x.UnitPrice,
                               Quantity = x.Quantity

                           };

            //var products = _context.Pictures.Select(p =>
            //                new
            //                {
            //                    PictureId= p.PictureId,
            //                    PicturePath=p.Path,
            //                    ProductId = p.ProductId,
            //                    ProductName = p.Product.ProductName,
            //                    UnitPrice = p.Product.UnitPrice,

            //                }
            //                );

            return Json(products);
        }
        public IActionResult ShoppingCart()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetShopping()
        {
            var cart = _context.Shopping.Where(c => c.BuyerAccount == loginAccount)
                .Select(c => new
                {
                    sellerAccount = c.Product.SellerAccount,
                    productId = c.ProductId,
                    productName = c.Product.ProductName,
                    categoryId = c.Product.ProductCategoryId,
                    categoryName = c.Product.ProductCategory.CategoryName,
                    unitPrice = c.Product.UnitPrice,
                    quantity = c.Quantity,
                    unitsInStock = c.Product.UnitsInStock,
                    currency = c.Product.Currency,
                    picture = c.Product.Pictures.ToList().Select(p => new
                    {
                        pictureId = p.PictureId,
                        picturePath = p.Path
                    })
                }).ToList().GroupBy(c => c.sellerAccount, (seller, product) => new
                {
                    sellerAccount = seller,
                    productCount = product.Count(),
                    products = product.Select(p => new
                    {
                        p.productId, p.productName, p.categoryId, p.categoryName, p.unitPrice, p.quantity, p.unitsInStock, p.picture, p.currency
                    })
                });
            var coupon = _context.Coupons.Where(c => c.HolderAccount == loginAccount && c.Status == "c0")
                .Where(c => DateTime.Compare(c.ExpirationDate, DateTime.Now) >= 0)
                .Select(c => new { c.CouponId, c.ProductCategoryId, c.ProductCategory.CategoryName, c.Discount })
                .ToList().GroupBy(c => c.ProductCategoryId, (category, coupon) => new
                {
                    categoryId = category,
                    coupons = coupon.Select(c => new
                    {
                        c.CouponId, c.CategoryName, c.Discount
                    })
                });
			if (cart == null) return Json(new { Result = "null"});
            return Json(new { cart, coupon });
        }
        [HttpPost]
        public async Task<JsonResult> AddCart([FromBody]ShoppingViewModel product)
        {
            if (ModelState.IsValid)
			{
				for (int i = 0; i < product.ProductId.Count; i++)
				{
					var p = await _context.Shopping.FindAsync(product.BuyerAccount, product.ProductId[i]);
                    if (p == null) continue;
					if (p.Quantity == product.Quantity[i]) continue;
					p.Quantity = product.Quantity[i];
					_context.Update(p);
					await _context.SaveChangesAsync();
				}
                return Json(new {Result = "OK", Message = "更新成功" });
            }
            else
            {
                return Json(new {Result = "Error", Message = "更新失敗" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            Shopping? shopping = await _context.Shopping.FindAsync(loginAccount, id);
			if (shopping == null)
			{
				return Json(new { Result = "Error", Message = "找不到欲刪除商品" });
			}
            try
            {
				_context.Shopping.Remove(shopping);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return Json(new { Result = "Error", Message = "刪除失敗" });
			}
			return Json(new { Result = "OK", Message = "刪除成功" });
		}
        [HttpPost]
        public JsonResult GetUserInfo(string id)
        {
            var user = _context.Users.Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.NickName,
                    u.Email,
                    u.Phone,
					Addresses = u.Addresses.Select(a => a.Address)
				});
            return Json(user);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Create(OrdersViewModel ordersViewModel)
        {

            if (ModelState.IsValid)
            {
                string UserId = loginAccount;//User.Identity.Name;
                                             //string guid = Guid.NewGuid().ToString();
                Orders order = new Orders();
                order.BuyerAccount = UserId;
                order.Address = ordersViewModel.Address;
                order.BuyerPhone = ordersViewModel.BuyerPhone;
                order.OrderDate = DateTime.Now;
                order.PayMethod = ordersViewModel.PayMethod;
                order.Status = "o1";
                _context.Orders.Add(order);
                //order.BuyerAccount = "User2";
                //order.Address = "台中";
                //order.BuyerPhone = "0933335566";
                //order.OrderDate = DateTime.Now;
                //order.PayMethod = "信用卡";
                //order.Status = "未出貨";
                //order.ArrivalDate = ;
                //_context.Orders.Add(order);

                var carList = _context.Orders.Where(m => m.BuyerAccount == UserId)/*.ToList()*/;


                foreach (var item in carList)
                {
                    item.Status = "o1";
                }
                _context.SaveChanges();
                return RedirectToAction("OrderList");
            }
            return View(ordersViewModel);
        }


        //建立訂單主檔列表
        public IActionResult OrderList()
        {
            string UserId = loginAccount;//User.Identity.Name;
            var orders = _context.Orders.Where(m => m.BuyerAccount == UserId).OrderByDescending(m => m.OrderDate).ToList();
            //目前會員的訂單主檔OrderList
            return View(orders);
        }
        public IActionResult OrderDetails(int OrderId)
        {
            var orderDetails = _context.OrderDetails.Where(m => m.OrderId == OrderId).ToList();
            return View(orderDetails);
        }
      
    }
}
