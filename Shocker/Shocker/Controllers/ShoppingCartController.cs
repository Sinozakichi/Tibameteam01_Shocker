using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Shocker.Models;
using Shocker.Models.ViewModels;

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
        string loginAccount = "User2";
        public IActionResult Index()
        {

            return View();
        }
        //此頁為產品列表, 按下開始購物後取得產品類別 名稱 單價 賣家 產品敘述 發行日期給產品列表 但還少加了圖片
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
            return Json(products);
        }
        //取得資料庫購物車內容 顯示到按結帳後出現的畫面 目前沒有新增連結 所以查網頁ShoppingCart/ShoppingCart
        public IActionResult ShoppingCart()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> GetShopping(/*[FromBody] ShoppingViewModel shoppingViewModel*/)
        {
               var d = _context.Shopping.Select(p => new
                {
                    Quantity= p.Quantity ,
                    ProductName = p.Product.ProductName,
                    ProductId =p.ProductId,
                    UnitPrice=p.Product.UnitPrice,
                });

                return Json(d);
            }
   
        string loginaccount = "User2";
        
        //(一)想做將商品加入購物車的功能 以下是自己跟延榮寫的 
        [HttpPost]
        public async Task<JsonResult> AddCart([FromBody] ShoppingViewModel shopping)
        {
            if (ModelState.IsValid)
            {
                //if (currentcar == null)
                //{
                Shopping s = new Shopping();
                /*Shopping s = new Shopping()*/
                s.ProductId = shopping.ProductId;
                s.Product.ProductName = shopping.ProductName;
                s.Product.UnitPrice = shopping.UnitPrice;
                s.Quantity = shopping.Quantity;
                _context.Shopping.Add(s);
                await _context.SaveChangesAsync();
                return Json(new { Result = "OK", Record = shopping });
            }
            else
            {
                return Json(new { Result = "Error", Message = "新增失敗" });
            }
        }
        //(二)想做將商品加入購物車的功能 以下是書上寫的判斷式
        public IActionResult AddCar([FromBody] ShoppingViewModel shoppingViewmodel)
        {
            if (ModelState.IsValid)
            {
                string UserId = loginaccount;//User.Identity.Name;
                var currentcar = _context.Shopping.Where(m => m.ProductId == shoppingViewmodel.ProductId && m.BuyerAccount == UserId).FirstOrDefault();
                if (currentcar == null)
                {
                    var product = _context.Shopping.Where(m => m.ProductId == shoppingViewmodel.ProductId).FirstOrDefault();
                    Shopping shopping = new Shopping();
                    shopping.ProductId = shoppingViewmodel.ProductId;
                    shopping.Quantity = 1;
                    _context.Shopping.Add(shopping);
                }
                else
                {
                    currentcar.Quantity += 1;
                }
                _context.SaveChanges();
                return View();
            }
            else
            {
                return RedirectToAction("ShoppingCart");
            }
        }
        //將商品從購物車移除
        public IActionResult Delete(int Id)
        {
            var shopping = _context.Shopping.Where(m => m.ProductId == 1).FirstOrDefault();
            _context.Shopping.Remove(shopping);
            _context.SaveChanges();
            return RedirectToAction("ShoppingCart");
        }

        //按結帳後出現的收件人填寫訂單資料
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Create(OrdersViewModel ordersViewModel)
        {

            if (ModelState.IsValid)
            {
                string UserId = loginaccount;//User.Identity.Name;
                                             //string guid = Guid.NewGuid().ToString();
                Orders order = new Orders();
                order.BuyerAccount = UserId;
                order.Address = ordersViewModel.Address;
                order.BuyerPhone = ordersViewModel.BuyerPhone;
                order.OrderDate = DateTime.Now;
                order.PayMethod = ordersViewModel.PayMethod;
                order.Status = "o1";
                _context.Orders.Add(order);
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


        //確認填寫完收件人訂單資料後要出現訂單明細
        public IActionResult OrderDetails(int OrderId)
        {
            var orderDetails = _context.OrderDetails.Where(m => m.OrderId == OrderId).ToList();
            return View(orderDetails);
        }
      
    }
}
