using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shocker.Models;
using Shocker.Models.ViewModels;
using System.Security.Claims;

namespace Shocker.Controllers
{
	public class ProductsController : Controller
	{
		private readonly db_a98a02_thm101team1001Context _context;
		private readonly IWebHostEnvironment _environment;
		public ProductsController(db_a98a02_thm101team1001Context context, IWebHostEnvironment environment, IConfiguration configuration)
		{
			_context = context;
			_environment = environment;
		}
		[Authorize]
		public IActionResult Index()
		{
			return View();
		}
		[HttpPost]
		public JsonResult GetInfo()
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new {Login = false, Message = "請先登入"});
			var p = _context.Products.AsNoTracking().Where(p => p.SellerAccount == account.Value);			
			var od = _context.OrderDetails.AsNoTracking().Where(od => od.Product.SellerAccount == account.Value);			
			var r = _context.Ratings.AsNoTracking().Where(r => r.Product.SellerAccount == account.Value);
			var info = _context.Users.AsNoTracking().Where(u => u.Id == account.Value)
					   .Select(u => new {
						   u.Id,
						   u.AboutSeller,
						   p0 = p.Count(p => p.Status == "p0"),
						   p1 = p.Count(p => p.Status == "p1"),
						   od1 = od.Count(od => od.Status == "od1"),
						   od2 = od.Count(od => od.Status == "od2"),
						   od3 = od.Count(od => od.Status == "od3"),
						   od4 = od.Count(od => od.Status == "od4"),
						   r0 = r.Count(r => r.Status == "r0"),
						   r1 = r.Count(r => r.Status == "r1")
					   });					   			
			return Json(new { Login = true, Record = info });
		}
		[HttpPost]
		public async Task<JsonResult> UpdateInfo([FromBody]AboutModel text)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			if (ModelState.IsValid)
			{				
				Users? user = await _context.Users.FindAsync(account.Value);
				if (user == null) return Json(new { Login = false, Message = "請先註冊" });
				user.AboutSeller = text.AboutSeller;
				_context.Update(user);
				await _context.SaveChangesAsync();
				return Json(new { Login = true, Message = "修改成功" });
			}
			else return Json(new { Login = true, Message = "修改失敗" });
		}		
		[HttpPost]
		public JsonResult GetProducts()
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			var product = _context.Products.Where(p => p.SellerAccount == account.Value)
				.Select(p => new
				{
					p.ProductId, p.LaunchDate, p.ProductName, p.ProductCategoryId, p.Description,
					p.UnitsInStock, p.Sales, p.UnitPrice, p.Status, p.Currency
				});
			if (product.IsNullOrEmpty()) return Json(new { Login = true, Found = false, Message = "沒有商品記錄" });
			else return Json(new { Login = true, Found = true, Record = product });
		}
		[HttpPost]
		public JsonResult GetProduct(int id)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			var product = _context.Products.Where(p => p.ProductId == id)
			.Select(p => new
			{
				p.ProductId,
				p.SellerAccount,
				p.LaunchDate,
				p.ProductName,
				ProductCategory = p.ProductCategory.CategoryName,
				p.Description,
				p.UnitsInStock,
				p.Sales,
				p.UnitPrice,
				p.Status,
				p.Currency
			});			
			return Json(new { Login = true, Record = product });
		}
		[HttpPost]
		public JsonResult Filter([FromBody] ProductsViewModel product)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			if (ModelState.IsValid)
			{
				var query = _context.Products.Where(p =>
				p.SellerAccount == account.Value && (
				p.ProductId == product.ProductId ||
				p.ProductName.Contains(product.ProductName ?? "") ||
				p.Description.Contains(product.Description ?? "") ||
				p.StatusNavigation.StatusName.Contains(product.Status ?? "") ||
				p.ProductCategory.CategoryName.Contains(product.Description ?? "")
				)).Select(p => new
				{
					p.ProductId, p.LaunchDate, p.ProductName, p.ProductCategoryId, p.Description,
					p.UnitsInStock, p.Sales, p.UnitPrice, p.Status, p.Currency
				});
				return Json(new { Login = true, Found = true, Record = query });
			}
			else return Json(new { Login = true, Found = false, Message = "查詢失敗" });
		}
		[HttpPost]
		public async Task<JsonResult> Create([FromBody] ProductsViewModel product)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			if (ModelState.IsValid)
			{
				Products p = new()
				{
					SellerAccount = product.SellerAccount,
					LaunchDate = DateTime.Now,
					ProductName = product.ProductName,
					ProductCategoryId = product.ProductCategoryId,
					Description = product.Description,
					UnitsInStock = product.UnitsInStock,
					Sales = 0,
					UnitPrice = product.UnitPrice,
					Status = product.Status != "p2" ? (product.Status == "p1" ? "p1" : "p0") : "p0",
					Currency = product.Currency
				};
				_context.Products.Add(p);
				await _context.SaveChangesAsync();
				return Json(new { Login = true, Message = "新增成功" });
			}
			else return Json(new { Login = true, Message = "新增失敗" });
		}
		[HttpPost]
		public async Task<JsonResult> Update([FromBody] ProductsViewModel product)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			if (ModelState.IsValid)
			{
				Products? p = await _context.Products.FindAsync(product.ProductId);
				if (p == null || p.SellerAccount != account.Value)
				{
					return Json(new { Login = true, Message = "商品不存在" });
				}
				if (p.Status == "p2") return Json(new { Login = true, Message = "商品已刪除" });
				if (p.Status == "p0" && product.Status == "p1") p.LaunchDate = DateTime.Now;
				p.ProductName = product.ProductName;
				p.ProductCategoryId = product.ProductCategoryId;
				p.Description = product.Description;
				p.UnitsInStock = product.UnitsInStock;
				p.UnitPrice = product.UnitPrice;
				p.Status = product.Status != "p2" ? (product.Status == "p1" ? "p1" : "p0") : "p0";
				p.Currency = product.Currency;
				_context.Update(p);
				await _context.SaveChangesAsync();
				return Json(new { Login = true, Message = "修改記錄成功" });
			}
			else return Json(new { Login = true, Message = "修改記錄失敗" });
		}
		[HttpPost]
		public async Task<JsonResult> Delete(int id)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			Products? product = await _context.Products.FindAsync(id);
			if (product == null) return Json(new { Login = true, Message = "商品不存在" });
			var picture = _context.Pictures.Where(p => p.ProductId == id).Select(p => p);
			if (!picture.IsNullOrEmpty())
			{
				foreach (var pic in picture)
				{
					var root = $@"{_environment.WebRootPath}\productpictures";
					var path = $@"{root}\{pic.PictureId}-{pic.Path}";
					System.IO.File.Delete(path);
					_context.Pictures.Remove(pic);
					await _context.SaveChangesAsync();
				}
			}
			try
			{
				product.UnitsInStock = 0;
				product.Status = "p2";
				_context.Update(product);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return Json(new { Login = true, Message = "刪除失敗" });
			}
			return Json(new { Login = true, Message = "刪除成功" });
		}
		[HttpPost]
		public JsonResult GetPictures(int id)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			var picture = _context.Pictures.Where(p => p.ProductId == id)
				.Select(p => new
				{
					p.PictureId, p.ProductId, p.Path, p.Description
				});
			return Json(new { Login = true, Record = picture });
		}
		[HttpPost]
		public async Task<JsonResult> UpdatePicture([FromBody] Pictures picture)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			if (ModelState.IsValid)
			{
				try
				{
					Pictures? p = await _context.Pictures.FindAsync(picture.PictureId);
					if (p == null || p.ProductId != picture.ProductId)
					{
						return Json(new { Login = true, Message = "記錄不存在" });
					}
					p.Description = picture.Description;
					_context.Update(p);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!PictureExists(picture.PictureId))
					{
						return Json(new { Login = true, Message = "記錄不存在" });
					}
					else throw;
				}
				return Json(new { Login = true, Message = "修改記錄成功" });
			}
			else return Json(new { Login = true, Message = "修改記錄失敗" });
		}

		private bool PictureExists(string pictureId)
		{
			return (_context.Pictures?.Any(p => p.PictureId == pictureId)).GetValueOrDefault();
		}
		[HttpPost]
		public async Task<JsonResult> DeletePicture(string id)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			Pictures? picture = await _context.Pictures.FindAsync(id);
			if (picture == null) return Json(new { Login = true, Message = "找不到欲刪除記錄" });
			try
			{
				var root = $@"{_environment.WebRootPath}\productpictures";
				var path = $@"{root}\{picture.PictureId}-{picture.Path}";
				System.IO.File.Delete(path);
				_context.Pictures.Remove(picture);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return Json(new { Login = true, Message = "刪除失敗" });
			}
			return Json(new { Login = true, Message = "刪除成功" });
		}
		[HttpPost]
		public async Task<JsonResult> UploadPicture(PicturesViewModel pic)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			if (ModelState.IsValid)
			{
				var root = $@"{_environment.WebRootPath}\productpictures";
				var time = DateTime.Now.Ticks;
				var unid = Guid.NewGuid().ToString();
				var path = $@"{root}\{unid}-{time}-{pic.Picture.FileName}";
				using (var st = System.IO.File.Create(path))
				{
					pic.Picture.CopyTo(st);
				}
				Pictures p = new()
				{
					PictureId = $@"{unid}",
					ProductId = pic.ProductId,
					Path = $@"{time}-{pic.Picture.FileName}",
					Description = pic.Description
				};
				_context.Pictures.Add(p);
				await _context.SaveChangesAsync();
				return Json(new { Login = true, Message = "新增成功" });
			}
			else return Json(new { Login = true, Message = "新增失敗" });
		}
		[HttpPost]
		public JsonResult GetOrders()
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			var orders = _context.Orders.Where(o => o.OrderDetails.Any(od => od.Product.SellerAccount == account.Value)).Select(o => new
			{
				o.OrderId, o.BuyerAccount, o.Address, o.OrderDate, o.ArrivalDate,
				o.BuyerPhone, o.PayMethod, o.Status, o.BuyerName
			});
			if (orders.IsNullOrEmpty()) return Json(new { Login = true, Found = false, Message = "沒有訂單記錄" });
			var orderDetails = _context.OrderDetails.Where(od => od.Product.SellerAccount == account.Value)
				.GroupBy(od => od.OrderId, (order, details) => new
				{
					OrderId = order,
					ProductCount = details.Count(),
					Product = details.Select(d => new
					{
						d.ProductId, d.CouponId, d.Quantity, d.Status, d.ReturnReason,
						d.ProductName, d.UnitPrice, d.Discount, d.Currency
					})
				});
			return Json(new { Login = true, Found = true, Orders = orders, OrderDetails = orderDetails });
		}
		[HttpPost]
		public async Task<JsonResult> UpdateOrderStatus([FromBody]OrderStatusModel order)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			if (ModelState.IsValid)
			{
				for (int i = 0; i < order.ProductId.Count; i++)
				{
					OrderDetails? orderDetails = await _context.OrderDetails.FindAsync(order.OrderId, order.ProductId[i]);			
					if (orderDetails == null) return Json(new { Login = true, Message = $"訂單中沒有商品編號{order.ProductId[i]}" });
					orderDetails.Status = (orderDetails.Status == "od1") ? "od2" : orderDetails.Status;
					_context.Update(orderDetails);
					await _context.SaveChangesAsync();
				}
				Orders? o = await _context.Orders.FindAsync(order.OrderId);
				if (o == null) return Json(new { Login = true, Message = $"查無訂單編號{order.OrderId}" });
				if (o.Status == "o1")
				{
					o.Status = "o2";
					_context.Update(o);
					await _context.SaveChangesAsync();
				}
				return Json(new { Login = true, Message = "更新狀態成功" });
			}
			else return Json(new { Login = true, Message = "更新狀態失敗" });
		}
		[HttpPost]
		public JsonResult GetRatings()
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			var ratings = _context.Ratings.Where(r => r.Product.SellerAccount == account.Value)
				.GroupBy(r => r.OrderId, (order, details) => new
				{
					OrderId = order,
					Product = details.ToList()
				});
			if (ratings.IsNullOrEmpty()) return Json(new { Login = true, Found = false, Message = "沒有評價記錄" });
			var orders = _context.Orders.Where(o => o.OrderDetails.Any(od => od.Product.SellerAccount == account.Value))
				.Where(o => o.Ratings.Any(r => r.OrderId == o.OrderId))
				.Select(o => new
				{
					o.OrderId, o.BuyerAccount, o.OrderDate, o.ArrivalDate, o.Status
				});
			var orderDetails = _context.OrderDetails.Where(od => od.Product.SellerAccount == account.Value)
				.Where(od => orders.Any(o => o.OrderId == od.OrderId))
				.GroupBy(od => od.OrderId, (order, details) => new
				{
					OrderId = order,
					Product = details.ToList()
				});
			return Json(new { Login = true, Found = true, Ratings = ratings, Orders = orders, OrderDetails = orderDetails });
		}
		[HttpPost]
		public async Task<JsonResult> ReplyBuyer([FromBody]ReplyModel reply)
		{
			var account = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (account == null) return Json(new { Login = false, Message = "請先登入" });
			if (ModelState.IsValid)
			{
				Ratings? rating = await _context.Ratings.FindAsync(reply.ProductId, reply.OrderId);
				if (rating == null || reply.SellerAccount != account.Value)
				{
					return Json(new { Login = true, Message = "評價記錄不存在" });
				}
				if (rating.Reply == "") return Json(new { Login = true, Message = "無回應內容" });
				if (rating.Status == "r1") return Json(new { Login = true, Message = "已回應買家評價" });
				rating.Reply = reply.Reply;
				rating.Status = "r1";
				_context.Update(rating);
				await _context.SaveChangesAsync();
				return Json(new { Login = true, Message = "評價回應成功" });
			}
			else return Json(new { Login = true, Message = "評價回應失敗" });
		}
	}	
}
