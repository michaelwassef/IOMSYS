using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]

    public class UsersController : Controller
    {
        private readonly IUsersService _UsersService;
        private readonly IUserTypesService _userTypesService;
        private readonly IPermissionsService _permissionsService;

        public UsersController(IUsersService usersService, IUserTypesService userTypesService, IPermissionsService permissionsService)
        {
            _UsersService = usersService;
            _userTypesService = userTypesService;
            _permissionsService = permissionsService;
        }

        [HttpGet]
        public async Task<IActionResult> UserOpenSession()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var user = await _UsersService.SelectUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user.UserName);
        }

        public async Task<IActionResult> UsersPageAsync()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Users", "UsersPageAsync");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadUsers()
        {
            var Users = await _UsersService.GetAllUsersAsync();
            return Json(Users);
        }

        [HttpGet]
        public async Task<IActionResult> LoadUserTypes()
        {
            var userTypes = await _userTypesService.GetAllUserTypesAsync();
            return Json(userTypes);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewUser([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Users", "AddNewUser");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var values = formData["values"];
                var newUser = new UsersModel();
                JsonConvert.PopulateObject(values, newUser);

                newUser.Password = PasswordHasher.HashPassword(newUser.Password);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addUserResult = await _UsersService.InsertUserAsync(newUser);

                if (addUserResult > 0)
                    return Ok(new { SuccessMessage = "Successfully Added" });
                else
                    return BadRequest(new { ErrorMessage = "Could Not Add" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "Could not add", ExceptionMessage = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Users", "UpdateUser");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var User = await _UsersService.SelectUserByIdAsync(key);

                var originalPassword = User.Password;

                JsonConvert.PopulateObject(values, User);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (originalPassword != User.Password)
                {
                    User.Password = PasswordHasher.HashPassword(User.Password);
                }

                int updateUserResult = await _UsersService.UpdateUserAsync(User);

                if (updateUserResult > 0)
                {
                    return Ok(new { SuccessMessage = "Updated Successfully" });
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Could Not Update" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the User.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Users", "DeleteUser");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deleteUserResult = await _UsersService.DeleteUserAsync(key);
                if (deleteUserResult > 0)
                    return Ok(new { SuccessMessage = "Deleted Successfully" });
                else
                    return BadRequest(new { ErrorMessage = "Could Not Delete" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

    }
}
