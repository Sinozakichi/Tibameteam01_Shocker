using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Index( CustomerQAViewModel cqavm)
        {
            if (ModelState.IsValid)
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
    }
}

