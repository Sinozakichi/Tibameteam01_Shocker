using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shocker.Models;
using Shocker.Models.ViewModels;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CustomerQAViewModel cqavm)
        {
            if (cqavm != null && ModelState.IsValid)
            {
                ClientCases clientCases = new ClientCases()
                {
                    Status = "cc0",
                    UserAccount = "User1",
                    Description = cqavm.Description,
                    QuestionCategoryId = cqavm.QuestionCategoryId,
                };
                _context.ClientCases.Add(clientCases);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));

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

