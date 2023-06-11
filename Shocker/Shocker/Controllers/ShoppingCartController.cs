using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shocker.Models;
using Shocker.Models.Banking;
using Shocker.Models.ViewModels;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace Shocker.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly db_a98a02_thm101team1001Context _context;
        public ShoppingCartController(db_a98a02_thm101team1001Context context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
        }

        public IActionResult Product(int id)
        {
			if (User.Identity.IsAuthenticated)
			{
				var loginAccount = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
				ViewBag.Account = loginAccount.Value;
			}
			ViewBag.Id = id;
            return View();
        }
        [HttpPost]
        public JsonResult GetProduct(int id)
        {
            var test = _context.Products.Find(id);
			if (test == null) return Json(new { Found = false, Message = "查無商品" });
			var product = _context.Products.Where(p => p.ProductId == id)
                .Select(p => new
                {
                    p.ProductId, p.ProductName, p.SellerAccount, p.LaunchDate, p.ProductCategoryId ,p.ProductCategory.CategoryName,
                    p.Description, p.UnitsInStock, p.Sales, p.UnitPrice, p.Status, p.Currency,
                    p.SellerAccountNavigation.AboutSeller
                });
            var products = _context.Products.Where(p => p.SellerAccount == product.ToList()[0].SellerAccount && p.Status == "p1").Take(4)
                .Select(p => new
                {
                    p.ProductId, p.ProductName, p.UnitPrice, p.UnitsInStock, p.Currency,
                    p.Pictures.FirstOrDefault().Path,
                    p.Pictures.FirstOrDefault().PictureId
                });
            var pictures = _context.Pictures.Where(p => p.ProductId == id)
                .Select(p => new
                {
                    p.PictureId, p.Path, p.Description
                });
            var ratings = _context.Ratings.Where(r => r.ProductId == id)
                .Select(r => new
                {
                    r.Description, r.StarCount, r.Reply,
                    r.Order.BuyerAccount
                });
            return Json(new { Found = true, Product = product, Pictures = pictures, Ratings = ratings, Products = products });
        }
        [Authorize]
        public IActionResult Index()
        {
			var loginAccount = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			ViewBag.Account = loginAccount.Value;
			return View();
        }
        [HttpPost]
        public async Task<JsonResult> CreateCart([FromBody] ShoppingViewModel product)
        {
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			if (ModelState.IsValid)
            {
                if (product.ProductId.Count > 1) return Json(new { Login = true, Message = "加入失敗" });
                var cart = await _context.Shopping.FindAsync(account.Value, product.ProductId[0]);
                if (cart != null) return Json(new { Login = true, Message = "已加入購物車" });
                var p = await _context.Products.FindAsync(product.ProductId[0]);
                if (p == null) return Json(new { Login = true, Message = "加入失敗" });
                if (p.SellerAccount == account.Value) return Json(new { Login = true, Message = "加入失敗" });
                if (p.Status != "p1" || p.UnitsInStock < product.Quantity[0]) return Json(new { Login = true, Message = "商品未上架或庫存不足" });
                Shopping s = new()
                {
                    BuyerAccount = account.Value,
                    ProductId = product.ProductId[0],
                    Quantity = product.Quantity[0]
                };
                _context.Shopping.Add(s);
                await _context.SaveChangesAsync();
                return Json(new { Login = true, Message = "加入成功" });
            }
            else return Json(new { Login = true, Message = "加入失敗" });
        }
        [HttpPost]
        public JsonResult GetShopping()
        {
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			var cart = _context.Shopping.Where(c => c.BuyerAccount == account.Value)
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
            var coupon = _context.Coupons.Where(c => c.HolderAccount == account.Value && c.Status == "c0")
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
            if (cart.IsNullOrEmpty()) return Json(new { Login = true, Found = false, Message = "商品尚未加入購物車" });
            return Json(new { Login = true, Found = true, Cart = cart, Coupon = coupon });
        }
        [HttpPost]
        public async Task<JsonResult> UpdateCart([FromBody] ShoppingViewModel product)
        {
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			if (ModelState.IsValid)
            {
                for (int i = 0; i < product.ProductId.Count; i++)
                {
                    var p = await _context.Shopping.FindAsync(account.Value, product.ProductId[i]);
                    if (p == null) continue;
                    if (p.Quantity == product.Quantity[i]) continue;
                    p.Quantity = product.Quantity[i];
                    _context.Update(p);
                    await _context.SaveChangesAsync();
                }
                return Json(new { Login = true, Message = "更新成功" });
            }
            else
            {
                return Json(new { Login = true, Message = "更新失敗" });
            }
        }
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			Shopping? shopping = await _context.Shopping.FindAsync(account.Value, id);
            if (shopping == null) return Json(new { Login = true, Message = "找不到欲刪除商品" });
            try
            {
                _context.Shopping.Remove(shopping);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Json(new { Login = true, Message = "刪除失敗" });
            }
            return Json(new { Login = true, Message = "刪除成功" });
        }
        [HttpPost]
        public JsonResult GetUserInfo()
        {
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			var user = _context.Users.Where(u => u.Id == account.Value)
                .Select(u => new
                {
                    u.Id,
                    u.NickName,
                    u.Email,
                    u.Phone,
                    Addresses = u.Addresses.Select(a => a.Address)
                });
            return Json(new { Login = true, User = user });
        }
        [HttpPost]
        public async Task<JsonResult> NewAddress([FromBody] AddressesViewModel address)
        {
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			Addresses? a = await _context.Addresses.FindAsync(address.Address, account.Value);
            if (a != null) return Json(new { Login = true, New = false, Message = "已儲存" });
            Addresses newAddress = new()
            {
                Address = address.Address,
                UserAccount = account.Value
            };
            _context.Addresses.Add(newAddress);
            _context.SaveChanges();
            return Json(new { Login = true, New = true, Message = "新增成功" });
        }

		[HttpPost]
		public async Task<JsonResult> CreateOrder([FromBody] OrdersViewModel order)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			if (!ModelState.IsValid) return Json(new { Login = true, Message = "訂單不成立" });
			if (order.BuyerAccount != account.Value) return Json(new { Login = true, Message = "訂單不成立" });

			foreach (OrderDetailsViewModel item in order.OrderDetails)
			{
				var p = await _context.Products.FindAsync(item.ProductId);
				if (p == null) return Json(new { Login = true, Message = "訂單不成立" });
				if (p.Status != "p1" || p.UnitsInStock < item.Quantity) return Json(new { Login = true, Message = "商品未上架或庫存不足" });
			}
			foreach (OrderDetailsViewModel item in order.OrderDetails)
			{
				var s = await _context.Shopping.FindAsync(order.BuyerAccount, item.ProductId);
				if (s == null) return Json(new { Login = true, Message = "購物車錯誤" });
				else _context.Shopping.Remove(s);
			}
			int newOrderId = (int)((DateTime.Now - new DateTime(2023, 6, 1)).Ticks / 1000000);
			Orders newOrder = new()
			{
				OrderId = newOrderId,
				BuyerAccount = order.BuyerAccount,
				Address = order.Address,
				OrderDate = DateTime.Now,
				ArrivalDate = null,
				BuyerPhone = order.BuyerPhone,
				PayMethod = order.PayMethod,
				BuyerName = order.BuyerName
			};
			if (newOrder.PayMethod == "信用卡") newOrder.Status = "o0";
			else if (newOrder.PayMethod == "貨到付款") newOrder.Status = "o1";
			else return Json(new { Login = true, Message = "訂單不成立" });
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
			var Total = 0;
			foreach (OrderDetails item in productList)
			{
				if (item.Discount != null)
				{
					var c = await _context.Coupons.FindAsync(item.CouponId);
					if (c != null && c.HolderAccount == account.Value)
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
				var subTotal = item.Discount != null ? item.UnitPrice * item.Quantity * (decimal)item.Discount : item.UnitPrice * item.Quantity;
				Total += (int)Math.Round(subTotal, 0);
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
			if (newOrder.PayMethod == "信用卡")
			{
				PaymentAsyncViewModel payment = new()
				{
					OrderId = newOrderId,
					PayType = "CREDIT",
					Amt = Total,
					Email = order.Email,
					ItemDesc = order.OrderDetails.FirstOrDefault().ProductName + "等",
					OrderComment = "信用卡支付"
				};
				return Json(new { Login = true, CreditCard = true, Payment = payment, Message = "訂單成立，請前往填寫付款資料" });
			}
			else return Json(new { Login = true, CreditCard = false, OrderId = newOrderId, Message = "訂單成立" });
		}
		
		public IActionResult ProductList(int id)
        {
			if (User.Identity.IsAuthenticated)
			{
				var loginAccount = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
				ViewBag.Account = loginAccount.Value;
			}
			ViewBag.CategoryId = id;
			return View();
        }
        [HttpPost]
        public JsonResult GetProductList(int id)
        {
            if (id == 0)
			{
				var productList = _context.Products.Where(p => p.Status == "p1")
					.Select(p => new
					{
						p.ProductId, p.ProductName, p.SellerAccount, p.UnitsInStock, p.UnitPrice, p.Currency,
						p.Pictures.FirstOrDefault().Path,
						p.Pictures.FirstOrDefault().PictureId
					});
				if (productList.IsNullOrEmpty()) return Json(new { Found = false, Message = "查無商品" });
				else return Json(new { Found = true, ProductList = productList });
			}
            if (id == 1 || id == 2 || id == 3 || id == 4 || id == 5 || id == 6 || id == 7 || id == 8)
            {
                var productList = _context.Products.Where(p => p.ProductCategoryId == id && p.Status == "p1")
                    .Select(p => new
                    {
                        p.ProductId, p.ProductName, p.SellerAccount, p.UnitsInStock, p.UnitPrice, p.Currency,
                        p.Pictures.FirstOrDefault().Path,
                        p.Pictures.FirstOrDefault().PictureId
                    });
                if (productList.IsNullOrEmpty()) return Json(new { Found = false, Message = "查無商品" });
                else return Json(new { Found = true, ProductList = productList });
            }
            return Json(new { Found = false, Message = "查無商品" });
		}
		
		[HttpPost]
        public JsonResult GetSearchResult(string id)
        {
			var query = _context.Products.Where(p =>
				p.ProductName.Contains(id) ||
				p.SellerAccount.Contains(id) ||
				p.Description.Contains(id) ||
				p.ProductCategory.CategoryName.Contains(id)
				).Where(p => p.Status == "p1")
				.Select(p => new
				{
					p.ProductId, p.ProductName, p.SellerAccount, p.UnitsInStock, p.UnitPrice, p.Currency,
					p.Pictures.FirstOrDefault().Path,
					p.Pictures.FirstOrDefault().PictureId
				});
			if (query.IsNullOrEmpty()) return Json(new { Found = false, Message = "查無商品" });
			else return Json(new { Found = true, Query = query });
		}

		[HttpGet]
		public ApiResultModel GetPopluarProduct()
		{
			var p = _context.Products.Where(p => p.Status == "p1").Include(p => p.Pictures).OrderByDescending(p => p.Sales).Take(4).Select(p => new
			{
				productId = p.ProductId,
				productName = p.ProductName,
				unitPrice = p.UnitPrice,
				picture = $"{p.Pictures.FirstOrDefault().PictureId}-{p.Pictures.FirstOrDefault().Path}"
			}).ToList();

			var p1 = _context.Products.Where(p => p.ProductCategoryId == 1 && p.Status == "p1").Include(p => p.Pictures).OrderByDescending(p => p.Sales).Take(4).Select(p => new
			{
				productId = p.ProductId,
				productName = p.ProductName,
				unitPrice = p.UnitPrice,
				picture = $"{p.Pictures.FirstOrDefault().PictureId}-{p.Pictures.FirstOrDefault().Path}"
			}).ToList();

			var p2 = _context.Products.Where(p => p.ProductCategoryId == 2 && p.Status == "p1").Include(p => p.Pictures).OrderByDescending(p => p.Sales).Take(4).Select(p => new
			{
				productId = p.ProductId,
				productName = p.ProductName,
				unitPrice = p.UnitPrice,
				picture = $"{p.Pictures.FirstOrDefault().PictureId}-{p.Pictures.FirstOrDefault().Path}"
			}).ToList();

			var p3 = _context.Products.Where(p => p.ProductCategoryId == 3 && p.Status == "p1").Include(p => p.Pictures).OrderByDescending(p => p.Sales).Take(4).Select(p => new
			{
				productId = p.ProductId,
				productName = p.ProductName,
				unitPrice = p.UnitPrice,
				picture = $"{p.Pictures.FirstOrDefault().PictureId}-{p.Pictures.FirstOrDefault().Path}"
			}).ToList();

			var p4 = _context.Products.Where(p => p.ProductCategoryId == 4 && p.Status == "p1").Include(p => p.Pictures).OrderByDescending(p => p.Sales).Take(4).Select(p => new
			{
				productId = p.ProductId,
				productName = p.ProductName,
				unitPrice = p.UnitPrice,
				picture = $"{p.Pictures.FirstOrDefault().PictureId}-{p.Pictures.FirstOrDefault().Path}"
			}).ToList();
			if (p == null || p1 == null || p2 == null || p3 == null || p4 == null) return new ApiResultModel() { Status = false, ErrorMessage = "商品類別中有無上架的商品!" };
			return new ApiResultModel()
			{
				Status = true,
				Data = new
				{
					allProduct = p,
					product1 = p1,
					product2 = p2,
					product3 = p3,
					product4 = p4,
				}
			};
		}
		[HttpGet]
		public ApiResultModel GetLatestProduct()
		{
			var p = _context.Products.Where(p => p.Status == "p1").Include(p => p.Pictures).OrderByDescending(p => p.LaunchDate).Take(6).Select(p => new
			{
				productId = p.ProductId,
				productName = p.ProductName,
				unitPrice = p.UnitPrice,
				picture = $"{p.Pictures.FirstOrDefault().PictureId}-{p.Pictures.FirstOrDefault().Path}"
			}).ToList();
			if (p == null) return new ApiResultModel() { Status = false, ErrorMessage = "沒有上架的商品!" };
			return new ApiResultModel()
			{
				Status = true,
				Data = new
				{
					latestProduct = p,
				}
			};
		}
		[HttpGet]
		public ApiResultModel GetYourCoupons()
		{
			var loginAccount = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (loginAccount == null) return new ApiResultModel { Status = false, ErrorMessage = "找不到此帳號" };

			var c = _context.Coupons.Where(c => c.Status == "c0" && c.HolderAccount == loginAccount.Value).Include(c => c.ProductCategory).OrderByDescending(c => c.ExpirationDate).Take(6).Select(c => new
			{
				discount = c.Discount,
				categoryName = c.ProductCategory.CategoryName,
			}).ToList();
			if (c == null) return new ApiResultModel() { Status = false, ErrorMessage = "您沒有優惠碼!" };
			return new ApiResultModel()
			{
				Status = true,
				Data = new
				{
					yourCoupons = c,
				}
			};
		}
	}
}
