using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class UserPermissionsController : Controller
    {
        private readonly IUsersService _usersService;
        private readonly IPermissionsService _permissionsService;

        public UserPermissionsController(IUsersService usersService, IPermissionsService permissionsService)
        {
            _usersService = usersService;
            _permissionsService = permissionsService;
        }

        [HttpGet]
        public async Task<IActionResult> AssignPermission()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "UserPermissions", "AssignPermission");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }

            var users = await _usersService.GetAllUsersAsync();
            var permissions = await _usersService.GetAllPermissions();
            var viewModel = new UserPermissionsAssignmentViewModel
            {
                Users = users,
                Permissions = permissions
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissionsForUser(int userId)
        {
            var branchIds = await _usersService.GetPermissionsForUserAsync(userId);
            return Json(branchIds);
        }

        public async Task<IActionResult> SavePermissionsAssignments(int SelectedUserId, List<int> SelectedPermissionsIds)
        {
            try
            {
                if (SelectedUserId == 0 || SelectedPermissionsIds == null)
                {
                    TempData["ErrorMessage"] = "يجب اختيار مستخدم و على الأقل فرع واحد";
                    return RedirectToAction("AssignBranches");
                }

                var currentBranchIds = await _usersService.GetPermissionsForUserAsync(SelectedUserId);

                var permissionToAdd = SelectedPermissionsIds.Except(currentBranchIds);
                var permissionToRemove = currentBranchIds.Except(SelectedPermissionsIds);

                // Add new branches
                foreach (var permissionId in permissionToAdd)
                {
                    var userpermissionModel = new UserPermissionModel { UserId = SelectedUserId, PermissionId = permissionId };
                    await _usersService.AddPermissionToUserAsync(userpermissionModel);
                }

                // Remove branches
                foreach (var permissionId in permissionToRemove)
                {
                    var userBranchesModel = new UserPermissionModel { UserId = SelectedUserId, PermissionId = permissionId };
                    await _usersService.RemovePermissionFromUserAsync(userBranchesModel);
                }

                TempData["SuccessMessage"] = "تم التعديل بنجاح.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating branch assignments: {ex.Message}";
            }

            return RedirectToAction("AssignPermission");
        }
    }
}
