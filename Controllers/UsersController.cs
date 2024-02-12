using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]

    public class UsersController : Controller
    {
        private readonly IUsersService _UsersService;
        private readonly IUserTypesService _userTypesService;

        public UsersController(IUsersService usersService, IUserTypesService userTypesService)
        {
            _UsersService = usersService;
            _userTypesService = userTypesService;
        }

        [Authorize(Roles = "GenralManager")]
        public IActionResult UsersPageAsync()
        {
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
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> AddNewUser([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var newUser = new UsersModel();
                JsonConvert.PopulateObject(values, newUser);

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
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> UpdateUser([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var User = await _UsersService.SelectUserByIdAsync(key);

                JsonConvert.PopulateObject(values, User);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

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
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeleteUser([FromForm] IFormCollection formData)
        {
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
