using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Shocker.Areas.Admin.Models.ViewModels;
using Shocker.Models;

namespace Shocker.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomerReplyController : Controller
    {
        private readonly db_a98a02_thm101team1001Context _context;

        public CustomerReplyController(db_a98a02_thm101team1001Context context)
        {
            _context = context;
        }

        // GET: Admin/CustomerReply
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetQA()
        {
            var clientQA = _context.ClientCases
                .Select(y => new
                {
                    CaseId = y.CaseId,
                    QuestionCategoryId = y.QuestionCategoryId,
                    UserAccount = y.UserAccount,
                    AdminAccount = y.AdminAccount,
                    Description = y.Description,
                    Status = y.Status,
                    CloseDate = y.CloseDate,
                    Reply = y.Reply,
                    Email = y.UserAccountNavigation.Email,
                });
            return Json(clientQA);
        }
        [HttpPost]
        public async Task<JsonResult> ReplyQA([FromBody] ClientCaseViewModels ccvm)
        {
            if (ccvm.Reply.IsNullOrEmpty()) { return Json(new { Message = "不可回復空白" }); }

            if (ccvm!=null && ModelState.IsValid)
            {
                ClientCases cc = await _context.ClientCases.FindAsync(ccvm.CaseId);
                if (cc == null)
                {
                    return Json(new { Message = "案件不存在" });
                }
                cc.AdminAccount = ccvm.AdminAccount;
                if (cc.Status == "cc0")
                {
                    cc.Status = "cc1";
                }
                else
                {
                    return Json(new { Message = "已結案" });
                }
                cc.Reply = ccvm.Reply;
                cc.CloseDate = DateTime.Now;
                _context.Update(cc);
                await _context.SaveChangesAsync();
                return Json(new { Message = "回復成功" });
            }
            else
            {
                return Json(new { Message = "格式錯誤" });
            };
        }
        [HttpPost]
        public async Task<JsonResult> FilterQA([FromBody] ClientCaseFilterViewModels ccf)
        {
            return Json(_context.ClientCases.Where(c =>
                c.CaseId == ccf.CaseId ||
                c.UserAccount.Contains(ccf.UserAccount) ||
                c.Status.Contains(ccf.Status) ||
                c.QuestionCategory.CategoryName.Contains(ccf.CategoryName) ||
                c.AdminAccountNavigation.Email.Contains(ccf.Email) ||
                c.StatusNavigation.StatusName.Contains(ccf.StatusName)
                ).Select(c => new
                {
                    CaseId = c.CaseId,
                    UserAccount = c.UserAccount,
                    Status = c.Status,
                    QuestionCategoryId = c.QuestionCategoryId,
                    Email = c.UserAccountNavigation.Email,
                    StatusName = c.StatusNavigation.StatusName,
                    CloseDate = c.CloseDate,
                }));
        }
    }
}
