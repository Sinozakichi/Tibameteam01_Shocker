using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shocker.Models;
using Shocker.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Shocker.Controllers
{
    public class CustomerQuestionController : Controller
    {
        private readonly db_a98a02_thm101team1001Context _context;

        public CustomerQuestionController(db_a98a02_thm101team1001Context context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }


        //[HttpPost]
        //public ApiResultModel ple([FromBody] CustomerQAViewModel cqavm)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            IEnumerable<string> QC = ModelState["QuestionCategoryId"]?.Errors.Select(x => x.ErrorMessage);
        //            IEnumerable<string> DC = ModelState["Description"]?.Errors.Select(x => x.ErrorMessage);
        //            return new ApiResultModel { Data = new { QCErrorMessage = QC, DCErrorMessage = DC }, Status = false, ErrorMessage = "出錯啦!" };
        //        };
        //        var loginAccount = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
        //        if (loginAccount == null) return new ApiResultModel { Status = false, ErrorMessage = "請重新登入帳號" };

        //        if (cqavm != null && ModelState.IsValid)
        //        {
        //            ClientCases clientCases = new ClientCases()
        //            {
        //                Status = "cc0",
        //                UserAccount = "User1",  //登入使用者
        //                Description = cqavm.Description,
        //                QuestionCategoryId = cqavm.QuestionCategoryId,
        //            };
        //            _context.ClientCases.Add(clientCases);
        //            _context.SaveChanges();
        //            return new ApiResultModel { ErrorMessage = "成功送出", Status = true };

        //        }
        //        else { return new ApiResultModel { Status = false, ErrorMessage = "未傳送成功" }; };

        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResultModel { ErrorMessage = "出蟲啦~", Status = false };
        //    }
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Index(CustomerQAViewModel cqavm)
        {
            try
            {
                var loginAccount = User.Claims.FirstOrDefault(x => x.ValueType == ClaimTypes.Name);

                if (loginAccount == null) { return View();};

                if (cqavm != null && ModelState.IsValid)
                {
                    ClientCases clientCases = new ClientCases()
                    {
                        Status = "cc0",
                        UserAccount = "User1",  //登入使用者要判斷
                        Description = cqavm.Description,
                        QuestionCategoryId = cqavm.QuestionCategoryId,
                    };
                    _context.ClientCases.Add(clientCases);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "成功送出";
                    return RedirectToAction("Index");

                }
                else { ViewBag.fal = "送出失敗"; }

                return View();
            }
            catch (Exception ex) 
            {
                return View(ex.Message);
            }
        }

        public async Task<IActionResult> QA()
        {
            return View();
        }
        public IActionResult GetProductlist()
        {
            return View();
        }
        [HttpPost]
        public ApiResultModel GetProductlistApi() {
            var p = _context.Products.Include(x => x.Pictures).Include(x => x.ProductCategory).Select(x => new
            {
                path = x.Pictures.FirstOrDefault().Path,

                productName = x.ProductName,
                unitPrice = x.UnitPrice,
                productId = x.ProductId,
                categoryId = x.ProductCategoryId,
                categoryName = x.ProductCategory.CategoryName,
            });
            return new ApiResultModel { Status = true ,ErrorMessage="成功",Data=p};
        }
        public ApiResultModel GetProductDetailApi(int id)
        {
            var pd = _context.Products.Include(x=>x.Pictures).AsNoTracking().FirstOrDefault(x=>x.ProductId == id);
            if (pd == null) {return new ApiResultModel() { Status = false, ErrorMessage = "id回傳失敗" }; }
           
            var p = _context.Products.Include(x => x.Pictures).Include(x => x.ProductCategory).Select(x => new
            {
                path = x.Pictures.FirstOrDefault().Path,

                productName = x.ProductName,
                unitPrice = x.UnitPrice,
                productId = x.ProductId,
                categoryId=x.ProductCategoryId,
                categoryName = x.ProductCategory.CategoryName,
            });
            return new ApiResultModel()
            {
                Status = true,
                Data = new
                {
                    productDetail = new{
                        productName = pd.ProductName,
                        unitPrice = pd.UnitPrice,
                        productId = pd.ProductId,
                        description = pd.Description,
                        path = pd.Pictures.FirstOrDefault().Path,
                        currency = pd.Currency,
                        categoryName = pd.ProductCategory.CategoryName,
                    },
                    Products = p
                }
            };
        }
    }
}

