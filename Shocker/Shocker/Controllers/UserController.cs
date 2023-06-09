﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Shocker.Models;
using Shocker.Models.ViewModels;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Shocker.Controllers
{
	[Route("{controller}/{action}/{id?}")]
	public class UserController : Controller
	{
		private readonly db_a98a02_thm101team1001Context _context;
		private readonly IWebHostEnvironment _environment;

		public UserController(db_a98a02_thm101team1001Context context, IWebHostEnvironment environment)
		{
			_context = context;
			_environment = environment;
		}
		[Authorize]
		public IActionResult MyAccount(string tab)//點選用戶資訊編輯的菜單選項時，帶一個tab的參數，依據參數abcde呈現不同的Partial View
		{
			ViewBag.Tab = tab;
			return View();
		}
		[Authorize]
		[HttpGet]
		public ApiResultModel GetAccount()//要接登入驗證那裡傳回的Users.Account參數，找出User表裡Account欄位符合登入帳號的該筆資料，將資料物件包成JSON傳到前端，先暫時寫死，等登入的參數
		{
			var loginAccount = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (loginAccount == null) return new ApiResultModel { Status = false, ErrorMessage = "找不到此帳號" };

			var a = _context.Users.Include(x => x.Addresses).AsNoTracking().FirstOrDefault(u => u.Id == loginAccount.Value);
			if (a == null) return new ApiResultModel { Status = false, ErrorMessage = "此帳號不存在!" };
			var ad = _context.Addresses.Include(u => u.UserAccountNavigation).AsNoTracking().Where(u => u.UserAccountNavigation.Id == loginAccount.Value).Select(u => new
			{
				address = u.Address,
			}).ToList();
			if (ad == null) return new ApiResultModel { Status = false, ErrorMessage = "此帳號不存在!" };
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			};

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
		[Authorize]
		[HttpPost]
		public ApiResultModel UpdateAccount([FromBody] UserViewModel uvm)//更新User資訊
		{
			if (!ModelState.IsValid)
			{
				IEnumerable<string> nicknameerror = ModelState["NickName"]?.Errors.Select(e => e.ErrorMessage);
				IEnumerable<string> emailerror = ModelState["Email"]?.Errors?.Select(e => e.ErrorMessage);
				return new ApiResultModel()
				{
					Status = false,
					Data = new
					{
						nicknameError = nicknameerror,
						emailError = emailerror,
					},
					ErrorMessage = "格式有誤!"
				};
			}
			else
			{
				try
				{
					var u = _context.Users.FirstOrDefault(u => u.Id == uvm.Id);
					if (u == null) return new ApiResultModel() { Status = false, ErrorMessage = "此帳號不存在!" };
					u.NickName = uvm.NickName;
					u.Gender = uvm.Gender;
					u.BirthDate = uvm.BirthDate;
					u.Email = uvm.Email;

					if (uvm.Address != "")
					{
						_context.Addresses.Add(new Addresses
						{
							Address = uvm.Address,
							UserAccount = uvm.Id,
						});
					}
					_context.SaveChanges();
					return new ApiResultModel() { Status = true };
				}
				catch (Exception)
				{
					return new ApiResultModel() { Status = false, ErrorMessage = "上傳帳號資訊失敗!" };
				}
			}

		}
		[Authorize]
		[HttpPost]
		public ApiResultModel UpdatePassword([FromBody] PasswordViewModel pvm)//更新User資訊
		{
			if (!ModelState.IsValid)
			{
				IEnumerable<string> passworderror = ModelState["Password"]?.Errors.Select(e => e.ErrorMessage);
				return new ApiResultModel()
				{
					Status = false,
					Data = new
					{
						passwordError = passworderror,
					},
					ErrorMessage = "密碼格式有誤!"
				};
			}
			else
			{
				try
				{
					var u = _context.Users.FirstOrDefault(u => u.Id == pvm.Id);
					if (u == null) return new ApiResultModel() { Status = false, ErrorMessage = "此帳號不存在!" };
					u.Password = pvm.Password;
					_context.SaveChanges();
					return new ApiResultModel() { Status = true };
				}
				catch (Exception)
				{
					return new ApiResultModel() { Status = false, ErrorMessage = "更新密碼失敗!" };
				}
			}

		}
		[Authorize]
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
		[Authorize]
		[HttpGet]
		public ApiResultModel GetOrders()//抓取登入的Account的所有的Orders的以下欄位資訊，暫時寫死，等登入傳入的參數
		{
			var loginAccount = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (loginAccount == null) return new ApiResultModel { Status = false, ErrorMessage = "找不到此帳號" };

			var o = _context.Orders.AsNoTracking().Include(x => x.StatusNavigation).Where(x => x.BuyerAccount == loginAccount.Value).Select(x => new
			{
				buyerAccount = x.BuyerAccount,
				orderId = x.OrderId,
				payMethod = x.PayMethod,
				status = x.StatusNavigation.StatusName,
			}).ToList();
			if (o == null) return new ApiResultModel() { Status = false, ErrorMessage = "此帳號不存在!" };
			var od = _context.OrderDetails.AsNoTracking().Include(x => x.Order).Where(x => x.Order.BuyerAccount == loginAccount.Value).Select(x => new
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
		[Authorize]
		[HttpPost]
		public ApiResultModel CancelOrders([FromBody] CancelOrdersViewModel covm)//更改Order狀態為已取消
		{
			try
			{
				var o = _context.Orders.Include(x => x.OrderDetails).ThenInclude(x => x.Product).FirstOrDefault(x => x.OrderId == covm.OrderId);
				if (o == null) return new ApiResultModel() { Status = false, ErrorMessage = "此訂單不存在!" };
				o.Status = "o5";//o5=已取消
				foreach (var od in o.OrderDetails)//尋覽每一個此筆Order裡的OrderDetail，並將他們的狀態一併改為已取消
				{
					od.Status = "od5";//od5=已取消
					od.Product.Sales -= od.Quantity;
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
		[Authorize]
		public IActionResult UserOrderDetails()
		{
			return View();
		}
		[Authorize]
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
					picture = $"{x.Product.Pictures.FirstOrDefault().PictureId}-{x.Product.Pictures.FirstOrDefault().Path}",
					statusName = x.StatusNavigation.StatusName,
					description = x.Order.Ratings.FirstOrDefault(s => s.ProductId == x.ProductId).Description ?? "",
					starCount = x.Order.Ratings.FirstOrDefault(s => s.ProductId == x.ProductId).StarCount == null ? 0 : x.Order.Ratings.FirstOrDefault(s => s.ProductId == x.ProductId).StarCount
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
		[Authorize]
		[HttpPost]
		public ApiResultModel TakeProduct([FromBody] TakeProductViewModel tpvm)
		{
			try
			{
				var od = _context.OrderDetails.FirstOrDefault(od => od.OrderId == tpvm.OrderId && od.ProductId == tpvm.ProductId);
				if (od == null) return new ApiResultModel { Status = false, ErrorMessage = "此筆訂單明細不存在!" };
				od.Status = "od3";//od3=已收貨

				int checkallget = 0;
				var o = _context.Orders.Include(o => o.OrderDetails).FirstOrDefault(o => o.OrderId == tpvm.OrderId);
				foreach (var ord in o.OrderDetails)//尋覽每一個此筆Order裡的OrderDetail，並檢查他們的狀態
				{
					if (ord.Status == "od1" || ord.Status == "od2" || ord.Status == "od4" || ord.Status == "od5")
					{
						checkallget += 1;
					}
				}
				if (checkallget == 0)
				{
					o.Status = "o3";//o3=已收貨
					o.ArrivalDate = DateTime.Now;
				}
				_context.SaveChanges();
				return new ApiResultModel() { Status = true };
			}
			catch (Exception)
			{
				return new ApiResultModel() { Status = false, ErrorMessage = "取貨失敗!" };
			}
		}
		[Authorize]
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
					Status = "r0",
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
		[Authorize]
		[HttpPost]
		public ApiResultModel UpdateOdReturnreason([FromBody] ReturnreasonViewModel rrvm)
		{
			if (!ModelState.IsValid)
			{
				IEnumerable<string> returnreasonerror = ModelState["ReturnReason"]?.Errors.Select(e => e.ErrorMessage);
				return new ApiResultModel()
				{
					Status = false,
					Data = new
					{
						returnReasonError = returnreasonerror,
					},
					ErrorMessage = "字數不足!"
				};
			}
			else
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

		}
		[Authorize]
		[HttpPost]
		public ApiResultModel UpdateRatingDescription([FromBody] RatingDescriptionViewModel rdvm)
		{
			if (!ModelState.IsValid)
			{
				IEnumerable<string> ratingdescriptionerror = ModelState["Description"]?.Errors.Select(e => e.ErrorMessage);
				return new ApiResultModel()
				{
					Status = false,
					Data = new
					{
						ratingDescriptionError = ratingdescriptionerror,
					},
					ErrorMessage = "字數不足!"
				};
			}
			else
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

		}
		[Authorize]
		[HttpGet]
		public ApiResultModel GetCoupons()
		{
			var loginAccount = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (loginAccount == null) return new ApiResultModel { Status = false, ErrorMessage = "找不到此帳號" };

			var c = _context.Coupons.AsNoTracking().Include(c => c.StatusNavigation).Include(c => c.OrderDetails).Where(c => c.HolderAccount == loginAccount.Value).Select(c => new
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
		[Authorize]
		[HttpGet]
		public ApiResultModel GetQuestions()
		{
			var loginAccount = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
			if (loginAccount == null) return new ApiResultModel { Status = false, ErrorMessage = "找不到此帳號" };

			var c = _context.ClientCases.AsNoTracking().Where(c => c.UserAccount == loginAccount.Value).Select(c => new
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
