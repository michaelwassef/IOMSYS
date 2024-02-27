using IOMSYS.IServices;
using IOMSYS.Models;
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

        public BranchesController(IBranchesService branchesService, IPermissionsService permissionsService)
        {
            _branchesService = branchesService;
            _permissionsService = permissionsService;
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

                int addBranchResult = await _branchesService.InsertBranchAsync(newbranch);

                if (addBranchResult > 0)
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

                int updateBranchResult = await _branchesService.UpdateBranchAsync(branch);

                if (updateBranchResult > 0)
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
                int deleteBranchResult = await _branchesService.DeleteBranchAsync(key);
                if (deleteBranchResult > 0)
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
