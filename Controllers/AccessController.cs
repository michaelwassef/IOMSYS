using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Security.Claims;

namespace IOMSYS.Controllers
{
    public class AccessController : Controller
    {
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

        //[HttpPost]
        //public async Task<IActionResult> Login1(AccessModel modelLogin, [FromServices] IAccessService userService)
        //{
        //    try
        //    {
        //        bool isAuthenticated = await userService.AuthenticateUserAsync(modelLogin);

        //        if (isAuthenticated)
        //        {
        //            List<Claim> claims = new List<Claim>
        //        {
        //            new Claim(ClaimTypes.NameIdentifier, modelLogin.UserName),
        //            new Claim(ClaimTypes.Role, GetRoleName(modelLogin.UserTypeId))
        //        };

        //            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        //            AuthenticationProperties properties = new AuthenticationProperties
        //            {
        //                AllowRefresh = true,
        //                IsPersistent = modelLogin.KeepLoggedIn
        //            };

        //            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
        //            return RedirectToAction("Index", "Home");

        //            // Redirect user based on userType
        //            //switch (modelLogin.UserTypeId)
        //            //{
        //            //    case 1: // GenralManager
        //            //        return RedirectToAction("GeneralManagerDashboard", "Dashboard");
        //            //    case 2: // BranchManager
        //            //        return RedirectToAction("BranchManagerDashboard", "Dashboard");
        //            //    case 3: // Employee
        //            //        return RedirectToAction("EmployeeDashboard", "Dashboard");
        //            //    default:
        //            //        {
        //            //            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //            //            return RedirectToAction("Login", "Access");
        //            //        }
        //            //}
        //        }

        //        TempData["ValidateMessage"] = "خطأ في اسم المستخدم او كلمة المرور او نوع المستخدم.";
        //        return View();
        //    }
        //    catch (SqlException)
        //    {
        //        TempData["ValidateMessage"] = "لا يسطتيع الاتصال بالسيرفر. الرجاء التحقق من الاتصال بقاعدة البيانات.";
        //        return View();
        //    }
        //    catch (Exception)
        //    {
        //        TempData["ValidateMessage"] = "حدث خطأ غير متوقع. الرجاء المحاولة لاحقًا.";
        //        return View();
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> Login(AccessModel modelLogin, [FromServices] IAccessService userService)
        {
            try
            {
                var authenticationResult = await userService.AuthenticateUserAsync(modelLogin);

                if (authenticationResult.IsAuthenticated)
                {
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, modelLogin.UserName),
                        new Claim(ClaimTypes.Role, GetRoleName(modelLogin.UserTypeId)),
                        new Claim("UserId", authenticationResult.UserId)
                    };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    AuthenticationProperties properties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = modelLogin.KeepLoggedIn
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
                    return RedirectToAction("Index", "Home");
                }

                TempData["ValidateMessage"] = "خطأ في اسم المستخدم او كلمة المرور او نوع المستخدم.";
                return View(modelLogin); // Pass modelLogin back to the view to retain input data
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
