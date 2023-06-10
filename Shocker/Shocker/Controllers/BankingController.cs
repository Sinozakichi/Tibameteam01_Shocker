using Microsoft.AspNetCore.Mvc;
using Shocker.Models;
using Shocker.Models.Banking;
using Shocker.Models.ViewModels;
using Shocker.Utility;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Shocker.Controllers
{
    public class BankingController : Controller
	{
		private readonly db_a98a02_thm101team1001Context _context;
		private readonly IWebHostEnvironment _environment;
		private readonly IConfiguration _configuration;

		public BankingController(db_a98a02_thm101team1001Context context, IWebHostEnvironment environment, IConfiguration configuration)
		{
			_context = context;
			_environment = environment;
			_configuration = configuration;
		}

		private BankInfoModel _bankInfoModel = new()
		{
			MerchantID = "MS149051454",
			HashKey = "ibbQy58FRSf0D5ffF18SbQFDibc8Gn1r",
			HashIV = "CXqLErYe6D2QvqMP",
			ReturnURL = "https://localhost:7259/Banking/PaymentReturn",
			NotifyURL = "",
			CustomerURL = "",
			AuthUrl = "https://ccore.newebpay.com/MPG/mpg_gateway",
			CloseUrl = "https://core.newebpay.com/API/CreditCard/Close"
		};
		public IActionResult Index()
		{
			return View();
		}


		[HttpPost]
		public async Task PaymentAsync(PaymentAsyncViewModel model)
		{
			string version = "1.5";
			DateTimeOffset taipeiStandardTimeOffset = DateTimeOffset.Now.ToOffset(new TimeSpan(8, 0, 0));
			TradeInfo tradeInfo = new()
			{
				MerchantID = _bankInfoModel.MerchantID,
				RespondType = "String",
				TimeStamp = taipeiStandardTimeOffset.ToUnixTimeSeconds().ToString(),
				Version = version,
				MerchantOrderNo = Convert.ToString(model.OrderId),
				Amt = model.Amt,
				ItemDesc = model.ItemDesc,
				ExpireDate = null,
				ReturnURL = _bankInfoModel.ReturnURL,
				NotifyURL = _bankInfoModel.NotifyURL,
				CustomerURL = _bankInfoModel.CustomerURL,
				ClientBackURL = null,
				Email = model.Email,
				EmailModify = 1,
				OrderComment = model.OrderComment,
				CREDIT = 1,
				WEBATM = 0,
				VACC = 0,
				CVS = 0,
				BARCODE = 0,
			};
			if (string.Equals(model.PayType, "CREDIT")) tradeInfo.CREDIT = 1;
			else if (string.Equals(model.PayType, "WEBATM")) tradeInfo.WEBATM = 1;
			else if (string.Equals(model.PayType, "VACC"))
			{
				tradeInfo.ExpireDate = taipeiStandardTimeOffset.AddDays(1).ToString("yyyyMMdd");
				tradeInfo.VACC = 1;
			}
			else if (string.Equals(model.PayType, "CVS"))
			{
				tradeInfo.ExpireDate = taipeiStandardTimeOffset.AddDays(1).ToString("yyyyMMdd");
				tradeInfo.CVS = 1;
			}
			else if (string.Equals(model.PayType, "BARCODE"))
			{
				tradeInfo.ExpireDate = taipeiStandardTimeOffset.AddDays(1).ToString("yyyyMMdd");
				tradeInfo.BARCODE = 1;
			}
			Atom<string> result = new Atom<string>()
			{
				IsSuccess = true
			};
			var inputModel = new SpgatewayInputModel
			{
				MerchantID = _bankInfoModel.MerchantID,
				Version = version
			};
			List<KeyValuePair<string, string>> tradeData = LambdaUtil.ModelToKeyValuePairList<TradeInfo>(tradeInfo);
			var tradeQueryPara = string.Join("&", tradeData.Select(x => $"{x.Key}={x.Value}"));
			inputModel.TradeInfo = CryptoUtil.EncryptAESHex(tradeQueryPara, _bankInfoModel.HashKey, _bankInfoModel.HashIV);
			inputModel.TradeSha = CryptoUtil.EncryptSHA256($"HashKey={_bankInfoModel.HashKey}&{inputModel.TradeInfo}&HashIV={_bankInfoModel.HashIV}");

			List<KeyValuePair<string, string>> postData = LambdaUtil.ModelToKeyValuePairList<SpgatewayInputModel>(inputModel);
			Response.Clear();
			StringBuilder s = new();
			s.Append("<html>");
			s.AppendFormat("<body onload='document.forms[\"form\"].submit()'>");
			s.AppendFormat("<form name='form' action='{0}' method='post'>", _bankInfoModel.AuthUrl);
			foreach (KeyValuePair<string, string> item in postData)
			{
				s.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", item.Key, item.Value);
			}
			s.Append("</form></body></html>");
			byte[] bytes = Encoding.ASCII.GetBytes(s.ToString());
			await HttpContext.Response.Body.WriteAsync(bytes);
		}

		public IActionResult PaymentReturn()
		{
			if (Request.Form["Status"] == "SUCCESS")
			{
				ViewBag.Status = "付款成功";
			}
			else ViewBag.Status = "付款失敗";
			string HashKey = _bankInfoModel.HashKey;
			string HashIV = _bankInfoModel.HashIV;
			string TradeInfoDecrypt = CryptoUtil.DecryptAESHex(Request.Form["TradeInfo"], HashKey, HashIV);
			NameValueCollection decryptTradeCollection = HttpUtility.ParseQueryString(TradeInfoDecrypt);
			ViewBag.MerchantOrderNo = decryptTradeCollection["MerchantOrderNo"];
			ViewBag.Amt = decryptTradeCollection["Amt"];
			ViewBag.PayTime = decryptTradeCollection["PayTime"];
			if (Request.Form["Status"] == "SUCCESS")
			{
				var order = _context.Orders.Find(Convert.ToInt32(decryptTradeCollection["MerchantOrderNo"]));
				order.Status = "o1";
				_context.Update(order);
				_context.SaveChanges();
			}
				return View();
		}
	}
}
