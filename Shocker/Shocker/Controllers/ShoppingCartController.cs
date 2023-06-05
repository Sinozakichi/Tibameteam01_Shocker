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
        public IActionResult Index()
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
        public async Task<JsonResult> NewAddress([FromBody]AddressesViewModel address)
        {
            Addresses? a = await _context.Addresses.FindAsync(address.Address, address.UserAccount);
            if(a != null) return Json(new { Result = "Error", Message = "已儲存" });
            if(address.UserAccount != loginAccount) return Json(new { Result = "Error", Message = "新增失敗" });
			Addresses NewAddress = new()
			{
				Address = address.Address,
				UserAccount = loginAccount
			};
			_context.Addresses.Add(NewAddress);
			_context.SaveChanges();
			return Json(new { Result = "OK", Message = "新增成功"});
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> Create([FromBody]OrdersViewModel order)
        {
            if (ModelState.IsValid)
            {
                if(order.BuyerAccount != loginAccount) return Json(new { Result = "Error", Message = "訂單不成立" });
				foreach (OrderDetailsViewModel item in order.OrderDetails)
				{
                    var p = await _context.Products.FindAsync(item.ProductId);
                    if(p == null) return Json(new { Result = "Error", Message = "訂單不成立" });
                    if(p.Status != "p1" || p.UnitsInStock < item.Quantity) return Json(new { Result = "Error", Message = "商品未上架或庫存不足" });
				}
				foreach (OrderDetailsViewModel item in order.OrderDetails)
				{
					var s = await _context.Shopping.FindAsync(order.BuyerAccount, item.ProductId);
					if (s == null) return Json(new { Result = "Error", Message = "購物車錯誤" });
					else _context.Shopping.Remove(s);
				}
				int newOrderId = (int)DateTime.Now.Ticks;
				Orders newOrder = new()
                {
                    OrderId = newOrderId,
					BuyerAccount = order.BuyerAccount,
                    Address = order.Address,
                    OrderDate = DateTime.Now,
                    ArrivalDate = null,
					BuyerPhone = order.BuyerPhone,					
					PayMethod = order.PayMethod,
					Status = "o1",
                    BuyerName = order.BuyerName
				};
				_context.Orders.Add(newOrder);
				await _context.SaveChangesAsync();
                var productList = order.OrderDetails.Select(od => new OrderDetails
                {
					OrderId = newOrderId,
					ProductId = od.ProductId,
                    CouponId = od.CouponId,
                    Quantity = od.Quantity,
                    Status = "od1",
					ProductName = od.ProductName,
                    UnitPrice = od.UnitPrice,
                    Discount = od.Discount,
                    Currency = od.Currency
				});
                foreach (OrderDetails item in productList)
                {					
                    if (item.Discount != null)
                    {
						var c = await _context.Coupons.FindAsync(item.CouponId);
                        if (c != null)
                        {
                            if (DateTime.Compare(c.ExpirationDate, DateTime.Now) >= 0)
                            {
								c.Status = "c1";
								_context.Update(c);
								await _context.SaveChangesAsync();
							}
                            else item.Discount = null;
                        }
                        else item.Discount = null;
					}
					_context.OrderDetails.Add(item);
					await _context.SaveChangesAsync();
					var p = await _context.Products.FindAsync(item.ProductId);
                    if (p != null)
                    {
                        p.UnitsInStock -= item.Quantity;
                        p.Sales += item.Quantity;
                        _context.Update(p); 
                        await _context.SaveChangesAsync();
					}
				}
                return Json(new { Result = "OK", OrderId = newOrderId, Message = "訂單成立" });
            }
            else return Json(new {Result = "Error", Message = "訂單不成立"});
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
