using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Shocker.Models;
using Shocker.Models.ViewModels;
using System.Net;

namespace Shocker.Controllers
{
	[Route("{controller}/{action}/{id?}")]
	public class UserController : Controller
	{
		private string loginAccount = "User8";//暫時寫死，等登入的參數
		private readonly db_a98a02_thm101team1001Context _context;
		private readonly IWebHostEnvironment _environment;

		public UserController(db_a98a02_thm101team1001Context context, IWebHostEnvironment environment)
		{
			_context = context;
			_environment = environment;
		}
		//後併入ShoppindCartController
		[HttpGet]
		public ApiResultModel GetPopluarProduct()
		{
			var p = _context.Products.Where(p =>  p.Status == "p1").Include(p => p.Pictures).OrderByDescending(p => p.Sales).Take(4).Select(p => new {
				productId = p.ProductId,
				productName = p.ProductName,
				unitPrice=p.UnitPrice,
				path = p.Pictures.FirstOrDefault().Path,
			}).ToList();

			var p1 = _context.Products.Where(p => p.ProductCategoryId == 1 && p.Status == "p1").Include(p => p.Pictures).OrderByDescending(p => p.Sales).Take(4).Select(p => new {
				productId = p.ProductId,
				productName = p.ProductName,
				unitPrice = p.UnitPrice,
				path = p.Pictures.FirstOrDefault().Path,
			}).ToList();

			var p2 = _context.Products.Where(p => p.ProductCategoryId == 2 && p.Status == "p1").Include(p => p.Pictures).OrderByDescending(p => p.Sales).Take(4).Select(p => new {
				productId = p.ProductId,
				productName = p.ProductName,
				unitPrice = p.UnitPrice,
				path = p.Pictures.FirstOrDefault().Path,
			}).ToList();

			var p3 = _context.Products.Where(p => p.ProductCategoryId == 3 && p.Status == "p1").Include(p => p.Pictures).OrderByDescending(p => p.Sales).Take(4).Select(p => new {
				productId = p.ProductId,
				productName = p.ProductName,
				unitPrice = p.UnitPrice,
				path = p.Pictures.FirstOrDefault().Path,
			}).ToList();

			var p4 = _context.Products.Where(p => p.ProductCategoryId == 4 && p.Status == "p1").Include(p => p.Pictures).OrderByDescending(p => p.Sales).Take(4).Select(p => new {
				productId = p.ProductId,
				productName = p.ProductName,
				unitPrice = p.UnitPrice,
				path = p.Pictures.FirstOrDefault().Path,
			}).ToList();
			if (p == null||p1==null || p2 == null || p3 == null || p4 == null) return new ApiResultModel() { Status = false, ErrorMessage = "商品類別中有無上架的商品!" };
			return new ApiResultModel()
			{
				Status = true,
				Data = new
				{
					allProduct=p,
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
				path = p.Pictures.FirstOrDefault().Path,
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
		//
		[HttpGet]
		public IActionResult MyAccount(string tab)//點選用戶資訊編輯的菜單選項時，帶一個tab的參數，依據參數abcde呈現不同的Partial View
		{
			ViewBag.Tab = tab;
			return View();
		}
		[HttpGet]
		public ApiResultModel GetAccount()//要接登入驗證那裡傳回的Users.Account參數，找出User表裡Account欄位符合登入帳號的該筆資料，將資料物件包成JSON傳到前端，先暫時寫死，等登入的參數
		{
			var a = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == loginAccount);
			if (a == null) return new ApiResultModel() { Status = false, ErrorMessage = "此帳號不存在!" };
			var ad = _context.Addresses.AsNoTracking().Where(u => u.UserAccount == loginAccount).Select(x => new
			{
				address = x.Address,
			}).ToList();

			return new ApiResultModel()
			{
				Status = true,
				Data = new
				{
					user = new
					{
						id = a.Id,
						password = a.Password,
						nickName = a.NickName,
						gender = a.Gender,
						birthDate = a.BirthDate,
						email = a.Email,
						phone = a.Phone,
						role = a.Role,
						registerDate = a.RegisterDate,
						picture = a.PicturePath,
					},
					address = ad
				}
			};
		}
		[HttpPost]
		public ApiResultModel UpdateAccount([FromBody] UserViewModel uvm)//更新User資訊
		{
			try
			{
				var u = _context.Users.FirstOrDefault(u => u.Id == uvm.Id);
				if (u == null) return new ApiResultModel() { Status = false, ErrorMessage = "此帳號不存在!" };
				u.Password = uvm.Password;
				u.NickName = uvm.NickName;
				u.Gender = uvm.Gender;
				u.BirthDate = uvm.BirthDate;
				u.Email = uvm.Email;

				_context.Addresses.Add(new Addresses
				{
					Address = uvm.Address,
					UserAccount = uvm.Id,
				});
				_context.SaveChanges();
				return new ApiResultModel() { Status = true };
			}
			catch (Exception)
			{
				return new ApiResultModel() { Status = false, ErrorMessage = "上傳帳號資訊失敗!" };
			}
		}
		[HttpPost]
		public ApiResultModel UploadPicture(PictureViewModel pvm)//更改User照片
		{
			try
			{
				var u = _context.Users.FirstOrDefault(u => u.Id == pvm.Id);
				if (u == null) return new ApiResultModel() { Status = false, ErrorMessage = "此帳號不存在!" };
				var root = $@"{_environment.WebRootPath}\img\userphoto";//網站根目錄的路由
				var time = DateTime.Now.Ticks;//當下的時間
				var unid = Guid.NewGuid();//創生一獨一無二的Id
				var path = $@"{root}\{unid}-{time}-{pvm.Picture.FileName}";//路徑全名:路由/檔案名
				using (var st = System.IO.File.Create(path))//創造路徑
				{
					pvm.Picture.CopyTo(st);//將前端傳來的檔案複製至該路徑上
				}
				u.PicturePath = $@"img\userphoto\{unid}-{time}-{pvm.Picture.FileName}";//擷取後半段存入資料庫的PicturePath欄位
				_context.SaveChanges();
				return new ApiResultModel() { Status = true };
			}
			catch (Exception)
			{
				return new ApiResultModel() { Status = false, ErrorMessage = "上傳帳號頭像失敗!" };
			}
		}
		[HttpGet]
		public ApiResultModel GetOrders()//抓取登入的Account的所有的Orders的以下欄位資訊，暫時寫死，等登入傳入的參數
		{
			var o = _context.Orders.AsNoTracking().Include(x => x.StatusNavigation).Where(x => x.BuyerAccount == loginAccount).Select(x => new
			{
				buyerAccount = x.BuyerAccount,
				orderId = x.OrderId,
				payMethod = x.PayMethod,
				status = x.StatusNavigation.StatusName,
			}).ToList();
			if (o == null) return new ApiResultModel() { Status = false, ErrorMessage = "此帳號不存在!" };
			var od = _context.OrderDetails.AsNoTracking().Include(x => x.Order).Where(x => x.Order.BuyerAccount == loginAccount).Select(x => new
			{
				orderId = x.OrderId,
				product = x.ProductId,
				quantity = x.Quantity,
				productName = x.ProductName,
				unitPrice = x.UnitPrice,
				disCount = x.Discount,
			}).ToList();
			return new ApiResultModel()
			{
				Status = true,
				Data = new
				{
					order = o,
					orderdetails = od,
				}
			};
		}
		[HttpPost]
		public ApiResultModel CancelOrders([FromBody] CancelOrdersViewModel covm)//更改Order狀態為已取消
		{
			try
			{
				var o = _context.Orders.Include(x => x.OrderDetails).ThenInclude(x=>x.Product).FirstOrDefault(x => x.OrderId == covm.OrderId);
				if (o == null) return new ApiResultModel() { Status = false, ErrorMessage = "此訂單不存在!" };
				o.Status = "o5";//o5=已取消
				foreach (var od in o.OrderDetails)//尋覽每一個此筆Order裡的OrderDetail，並將他們的狀態一併改為已取消
				{
					od.Status = "od5";//od5=已取消
					od.Product.Sales-=od.Quantity;
					od.Product.UnitsInStock += od.Quantity;
				}
				_context.SaveChanges();
				return new ApiResultModel() { Status = true };
			}
			catch (Exception)
			{
				return new ApiResultModel() { Status = false, ErrorMessage = "取消訂單失敗!" };
			}
		}
		public IActionResult UserOrderDetails()
		{
			return View();
		}
		[HttpGet]
		public ApiResultModel GetUserOrderDetails(int id)//抓取上頁點選之指定OrderId的全部OrderDetail的指定欄位的資料
		{
			var o = _context.Orders.AsNoTracking().FirstOrDefault(x => x.OrderId == id);
			if (o == null) return new ApiResultModel() { Status = false, ErrorMessage = "此筆訂單不存在!" };
			var od = _context.OrderDetails.AsNoTracking().Include(x => x.StatusNavigation)
				.Include(x => x.Product).ThenInclude(x => x.Pictures).Include(x => x.Product)
				.ThenInclude(x => x.ProductCategory).Include(x => x.Product).ThenInclude(x => x.Ratings)
				.Where(x => x.OrderId == id).Select(x => new
				{
					productId = x.ProductId,
					quantity = x.Quantity,
					sellerAccount = x.Product.SellerAccount,
					productName = x.ProductName,
					unitPrice = x.UnitPrice,
					discount = x.Discount,
					categoryName = x.Product.ProductCategory.CategoryName,
					pictureId=x.Product.Pictures.FirstOrDefault().PictureId,
					path = x.Product.Pictures.FirstOrDefault().Path,
					statusName = x.StatusNavigation.StatusName,
					description = x.Product.Ratings.FirstOrDefault().Description == null ? "" : x.Product.Ratings.FirstOrDefault().Description,
					starCount = x.Product.Ratings.FirstOrDefault().StarCount == null ? 0 : x.Product.Ratings.FirstOrDefault().StarCount,
				}).ToList();

			return new ApiResultModel()
			{
				Status = true,
				Data = new
				{
					order = new
					{
						buyerAccount = o.BuyerAccount,
						orderId = o.OrderId,
						address = o.Address,
						orderDate = o.OrderDate,
						arrivalDate = o.ArrivalDate,
						buyerPhone = o.BuyerPhone,
						payMethod = o.PayMethod,
					},
					details = od
				}
			};
		}
		[HttpPost]
		public ApiResultModel TakeProduct([FromBody] TakeProductViewModel tpvm)
		{
			try
			{
				var od = _context.OrderDetails.FirstOrDefault(od => od.OrderId == tpvm.OrderId && od.ProductId == tpvm.ProductId);
				if (od == null) return new ApiResultModel { Status = false, ErrorMessage = "此筆訂單明細不存在!" };
				od.Status = "od3";
				_context.SaveChanges();
				return new ApiResultModel() { Status = true };
			}
			catch (Exception)
			{
				return new ApiResultModel() { Status = false, ErrorMessage = "取貨失敗!" };
			}
		}
		[HttpPost]
		public ApiResultModel CreateRating([FromBody] RatingViewModel rvm)
		{
			try
			{
				_context.Ratings.Add(new Ratings
				{
					ProductId = rvm.ProductId,
					OrderId = rvm.OrderId,
					StarCount = rvm.StarCount,
				});

				var od = _context.OrderDetails.FirstOrDefault(od => od.OrderId == rvm.OrderId && od.ProductId == rvm.ProductId);
				if (od == null) return new ApiResultModel { Status = false, ErrorMessage = "此筆訂單明細不存在!" };
				od.Status = "od6";
				_context.SaveChanges();
				return new ApiResultModel() { Status = true };
			}
			catch (Exception)
			{
				return new ApiResultModel() { Status = false, ErrorMessage = "新增評價失敗!" };
			}
		}
		[HttpPost]
		public ApiResultModel UpdateOdReturnreason([FromBody] ReturnreasonViewModel rrvm)
		{
			try
			{
				var od = _context.OrderDetails.FirstOrDefault(od => od.OrderId == rrvm.OrderId && od.ProductId == rrvm.ProductId);
				if (od == null) return new ApiResultModel { Status = false, ErrorMessage = "此筆訂單明細不存在!" };
				od.ReturnReason = rrvm.ReturnReason;
				od.Status = "od4";

				var o = _context.Orders.FirstOrDefault(o => o.OrderId == rrvm.OrderId);
				if (o == null) return new ApiResultModel() { Status = false, ErrorMessage = "此筆訂單不存在!" };
				o.Status = "o4";
				_context.SaveChanges();
				return new ApiResultModel() { Status = true };
			}
			catch (Exception)
			{
				return new ApiResultModel() { Status = false, ErrorMessage = "退貨失敗!" };
			}
		}
		[HttpPost]
		public ApiResultModel UpdateRatingDescription([FromBody] RatingDescriptionViewModel rdvm)
		{
			try
			{
				var r = _context.Ratings.FirstOrDefault(r => r.OrderId == rdvm.OrderId && r.ProductId == rdvm.ProductId);
				if (r == null) return new ApiResultModel() { Status = false, ErrorMessage = "此筆評價不存在!" };
				r.Description = rdvm.Description;
				_context.SaveChanges();
				return new ApiResultModel() { Status = true };
			}
			catch (Exception)
			{
				return new ApiResultModel() { Status = false, ErrorMessage = "上傳評價失敗!" };
			}
		}
		[HttpGet]
		public ApiResultModel GetCoupons()
		{
			var c = _context.Coupons.AsNoTracking().Include(c => c.StatusNavigation).Include(c => c.OrderDetails).Where(c => c.HolderAccount == loginAccount).Select(c => new
			{
				holderAccount = c.HolderAccount,
				couponId = c.CouponId,
				expirationDate = c.ExpirationDate,
				publisherAccount = c.PublisherAccount,
				discount = c.Discount,
				productCategoryName = c.ProductCategory.CategoryName,
				statusName = c.StatusNavigation.StatusName,
				orderId = c.OrderDetails.FirstOrDefault().OrderId == null ? 0 : c.OrderDetails.FirstOrDefault().OrderId,
			}).ToList();
			if (c == null) return new ApiResultModel() { Status = false, ErrorMessage = "此帳號不存在!" };
			return new ApiResultModel()
			{
				Status = true,
				Data = new
				{
					coupons = c,
				}
			};
		}
		[HttpGet]
		public ApiResultModel GetQuestions()
		{
			var c = _context.ClientCases.AsNoTracking().Where(c => c.UserAccount == loginAccount).Select(c => new
			{
				caseId = c.CaseId,
				description = c.Description,
				statusName = c.StatusNavigation.StatusName,
				reply = c.Reply,
				categoryName = c.QuestionCategory.CategoryName,
			}).ToList();
			if (c == null) return new ApiResultModel() { Status = false, ErrorMessage = "此帳號不存在!" };
			return new ApiResultModel()
			{
				Status = true,
				Data = new
				{
					clientCases = c,
				}
			};
		}
	}
}
