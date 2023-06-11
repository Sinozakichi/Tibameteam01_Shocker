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
                // * 商店代號
                MerchantID = _bankInfoModel.MerchantID,
                // * 回傳格式
                RespondType = "String",
                // * TimeStamp
                TimeStamp = taipeiStandardTimeOffset.ToUnixTimeSeconds().ToString(),
                // * 串接程式版本
                Version = version,
                // * 商店訂單編號
                MerchantOrderNo = Convert.ToString(model.OrderId),
                // * 訂單金額
                Amt = model.Amt,
                // * 商品資訊
                ItemDesc = model.ItemDesc,
                // 繳費有效期限(適用於非即時交易)
                ExpireDate = null,
                // 支付完成 返回商店網址
                ReturnURL = _bankInfoModel.ReturnURL,
                // 支付通知網址
                NotifyURL = _bankInfoModel.NotifyURL,
                // 商店取號網址
                CustomerURL = _bankInfoModel.CustomerURL,
                // 支付取消 返回商店網址
                ClientBackURL = null,
                // * 付款人電子信箱
                Email = model.Email,
                // 付款人電子信箱 是否開放修改(1=可修改 0=不可修改)
                EmailModify = 1,
                // 商店備註
                OrderComment = model.OrderComment,
                // 信用卡 一次付清啟用(1=啟用、0或者未有此參數=不啟用)
                CREDIT = 1,
                // WEBATM啟用(1=啟用、0或者未有此參數，即代表不開啟)
                WEBATM = 0,
                // ATM 轉帳啟用(1=啟用、0或者未有此參數，即代表不開啟)
                VACC = 0,
                // 超商代碼繳費啟用(1=啟用、0或者未有此參數，即代表不開啟)(當該筆訂單金額小於 30 元或超過 2 萬元時，即使此參數設定為啟用，MPG 付款頁面仍不會顯示此支付方式選項。)
                CVS = 0,
                // 超商條碼繳費啟用(1=啟用、0或者未有此參數，即代表不開啟)(當該筆訂單金額小於 20 元或超過 4 萬元時，即使此參數設定為啟用，MPG 付款頁面仍不會顯示此支付方式選項。)
                BARCODE = 0,
			};
			if (string.Equals(model.PayType, "CREDIT")) tradeInfo.CREDIT = 1;
			else if (string.Equals(model.PayType, "WEBATM")) tradeInfo.WEBATM = 1;
			else if (string.Equals(model.PayType, "VACC"))
			{
                // 設定繳費截止日期
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
                // 設定繳費截止日期
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

            // 將model 轉換為List<KeyValuePair<string, string>>, null值不轉
            List<KeyValuePair<string, string>> tradeData = LambdaUtil.ModelToKeyValuePairList<TradeInfo>(tradeInfo);
            // 將List<KeyValuePair<string, string>> 轉換為 key1=Value1&key2=Value2&key3=Value3...
            var tradeQueryPara = string.Join("&", tradeData.Select(x => $"{x.Key}={x.Value}"));
            // AES 加密
            inputModel.TradeInfo = CryptoUtil.EncryptAESHex(tradeQueryPara, _bankInfoModel.HashKey, _bankInfoModel.HashIV);
            // SHA256 加密
            inputModel.TradeSha = CryptoUtil.EncryptSHA256($"HashKey={_bankInfoModel.HashKey}&{inputModel.TradeInfo}&HashIV={_bankInfoModel.HashIV}");

            // 將model 轉換為List<KeyValuePair<string, string>>, null值不轉
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

        /// <summary>
        /// [智付通]金流介接(結果: 支付完成 返回商店網址)
        /// </summary>
        public IActionResult PaymentReturn()
		{
			if (Request.Form["Status"] == "SUCCESS")
			{
				ViewBag.Status = "付款成功";
			}
			else
			{
                ViewBag.Status = "付款失敗";
            }
			
			string HashKey = _bankInfoModel.HashKey;
			string HashIV = _bankInfoModel.HashIV;
            // TradeInfo 交易資料AES 加密
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
