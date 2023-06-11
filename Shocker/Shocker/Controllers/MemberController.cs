using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Shocker.Models;
using Shocker.Models.ViewModels;
using System.Security.Claims;

namespace Shocker.Controllers
{
    
    public class MemberController : Controller
    {
        private readonly db_a98a02_thm101team1001Context _DBcontext;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public MemberController(db_a98a02_thm101team1001Context context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _DBcontext = context;
            _environment = environment;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> SignIn(LoginViewModel model)
        {
            var user = _DBcontext.Users
                .FirstOrDefault(x => x.Id == model.Id && x.Password == model.Password);

            if (user == null)
            {
                ViewBag.ErrorMessage = "帳號或密碼有誤，登入失敗";
                return View("SignIn");
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Id),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(claimsPrincipal);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            
            if (!ModelState.IsValid) 
            { 
                return View("Register");
            }

            var user = _DBcontext.Users.FirstOrDefault(x=>x.Id==model.Id);
            if(user != null)
            {
                ViewBag.ErrorMessage = "帳號已經存在";
                return View("Register");
            }

            _DBcontext.Users.Add(new Users()
            {
                Id = model.Id,
                AccountType = "-",
                Password = model.Password,
                NickName = model.NickName,
                Gender = model.Gender,
                BirthDate = model.BirthDate,
                Email = model.Email,
                Phone = model.Phone,
                Role = "user",
                RegisterDate = DateTime.Now
                
            });

            _DBcontext.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
