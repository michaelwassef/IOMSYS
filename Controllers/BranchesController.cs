using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class BranchesController : Controller
    {
        private readonly IBranchesService _branchesService;
        private readonly IPermissionsService _permissionsService;
        private readonly IAccessService _accessService;

        public BranchesController(IBranchesService branchesService, IPermissionsService permissionsService, IAccessService accessService)
        {
            _branchesService = branchesService;
            _permissionsService = permissionsService;
            _accessService = accessService;
        }

        public async Task<IActionResult> BranchesPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Branches", "BranchesPage");
            if (!hasPermission)
            {
                return RedirectToAction("AccessDenied", "Access");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadBranches()
        {
            var branches = await _branchesService.GetAllBranchesAsync();
            return Json(branches);
        }

        [HttpGet]
        public async Task<IActionResult> LoadBranchesByUser()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var branches = await _branchesService.GetAllBranchesByUserAsync(userId);
            return Json(branches);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewBranch([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Branches", "AddNewBranch");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var values = formData["values"];
                var newbranch = new BranchesModel();
                JsonConvert.PopulateObject(values, newbranch);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _branchesService.SelectBranchIdByManagerIdAsync((int)newbranch.BranchMangerId);
                if (user != null)
                {
                    return BadRequest(new { ErrorMessage = "لا يمكن جعل مستخدم مسئول لاكثر من مخزن واحد" });
                }

                int addBranchResult = await _branchesService.InsertBranchAsync(newbranch);

                if (addBranchResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "POST", "Branches", addBranchResult, "Insert New Branch : " + newbranch.BranchName, 0);
                    return Ok(new { SuccessMessage = "Successfully Added" });
                }
                else
                    return BadRequest(new { ErrorMessage = "Could Not Add" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "Could not add", ExceptionMessage = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBranch([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Branches", "UpdateBranch");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var branch = await _branchesService.SelectBranchByIdAsync(key);

                JsonConvert.PopulateObject(values, branch);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _branchesService.SelectBranchIdByManagerIdAsync((int)branch.BranchMangerId);
                if (user != null)
                {
                    return BadRequest(new { ErrorMessage = "لا يمكن جعل مستخدم مدير لاكثر من فرع واحد" });
                }

                int updateBranchResult = await _branchesService.UpdateBranchAsync(branch);

                if (updateBranchResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "PUT", "Branches", key, "Update Branch : " + branch.BranchName, 0);
                    return Ok(new { SuccessMessage = "Updated Successfully" });
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Could Not Update" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Branch.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBranch([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Branches", "DeleteBranch");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var branch = await _branchesService.SelectBranchByIdAsync(key);
                int deleteBranchResult = await _branchesService.DeleteBranchAsync(key);
                if (deleteBranchResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "DELETE", "Branches", key, "Delete Branch : "+branch.BranchName, 0);
                    return Ok(new { SuccessMessage = "Deleted Successfully" });
                }
                else
                    return BadRequest(new { ErrorMessage = "Could Not Delete" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckPasswordForManagerOfBranch([FromBody] BranchesModel branchesModel)
        {
            try
            {
                var UserId = await _branchesService.SelectUserIdByBranchIdAsync(branchesModel.BranchId);
                var UserIdInt = Convert.ToInt32(UserId);
                var password = PasswordHasher.HashPassword(branchesModel.Password);

                bool IsManger = await _accessService.CheckPassword(UserIdInt, password);
                if (IsManger)
                    return Ok(new { SuccessMessage = "Successfully" });

                else return BadRequest(new { ErrorMessage = "An error occurred while checking the password." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while checking the password.", ExceptionMessage = ex.Message });
            }
        }

    }
}
