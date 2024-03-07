using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Security.Claims;

namespace IOMSYS.Controllers
{
    public class AccessController : Controller
    {
        private readonly IAccessService _accessService;

        public AccessController(IAccessService accessService)
        {
            _accessService = accessService;
        }

        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AccessModel modelLogin, [FromServices] IAccessService userService)
        {
            try
            {
                modelLogin.Password = PasswordHasher.HashPassword(modelLogin.Password);

                var authenticationResult = await userService.AuthenticateUserAsync(modelLogin);

                if (authenticationResult.IsAuthenticated)
                {
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, modelLogin.UserName),
                        new Claim(ClaimTypes.Role, GetRoleName(authenticationResult.UserTypeId)),
                        new Claim("UserId", authenticationResult.UserId)
                    };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    AuthenticationProperties properties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = modelLogin.KeepLoggedIn
                    };

                    string username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
                    return RedirectToAction("Index", "Home");
                }
                else if (!authenticationResult.IsActive)
                {
                    TempData["ValidateMessage"] = "خطأ في اسم المستخدم او كلمة المرور او نوع المستخدم.";
                    return View(modelLogin);
                }
                else
                {
                    // Handle authentication failure
                    TempData["ValidateMessage"] = "خطأ في اسم المستخدم او كلمة المرور او نوع المستخدم.";
                    return View(modelLogin);
                }
            }
            catch (SqlException)
            {
                TempData["ValidateMessage"] = "لا يسطتيع الاتصال بالسيرفر. الرجاء التحقق من الاتصال بقاعدة البيانات.";
                return View(modelLogin);
            }
            catch (Exception)
            {
                TempData["ValidateMessage"] = "حدث خطأ غير متوقع. الرجاء المحاولة لاحقًا.";
                return View(modelLogin);
            }
        }

        private string GetRoleName(int userType)
        {
            switch (userType)
            {
                case 1:
                    return "GenralManager";
                case 2:
                    return "BranchManager";
                case 3:
                    return "Employee";
                default:
                    return "";
            }
        }

    }
}
