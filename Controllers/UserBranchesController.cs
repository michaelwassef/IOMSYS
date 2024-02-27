using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class UserBranchesController : Controller
    {
        private readonly IUsersService _usersService;
        private readonly IBranchesService _branchesService;
        private readonly IUserBranchesService _userBranchesService;
        private readonly IPermissionsService _permissionsService;

        public UserBranchesController(IUsersService usersService, IBranchesService branchesService, IUserBranchesService userBranchesService, IPermissionsService permissionsService)
        {
            _usersService = usersService;
            _branchesService = branchesService;
            _userBranchesService = userBranchesService;
            _permissionsService = permissionsService;
        }

        [HttpGet]
        public async Task<IActionResult> AssignBranches()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "UserBranches", "AssignBranches");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }

            var users = await _usersService.GetAllUsersAsync();
            var branches = await _branchesService.GetAllBranchesAsync();
            var viewModel = new UserBranchAssignmentViewModel
            {
                Users = users,
                Branches = branches
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetBranchesForUser(int userId)
        {
            var branchIds = await _userBranchesService.GetBranchesForUserAsync(userId);
            return Json(branchIds);
        }

        [HttpPost]
        public async Task<IActionResult> SaveBranchAssignments(int SelectedUserId, List<int> selectedBranchIds)
        {
            try
            {
                if (SelectedUserId == 0 || selectedBranchIds == null)
                {
                    TempData["ErrorMessage"] = "يجب اختيار مستخدم و على الأقل فرع واحد";
                    return RedirectToAction("AssignBranches");
                }

                var currentBranchIds = await _userBranchesService.GetBranchesForUserAsync(SelectedUserId);

                var branchesToAdd = selectedBranchIds.Except(currentBranchIds);
                var branchesToRemove = currentBranchIds.Except(selectedBranchIds);

                // Add new branches
                foreach (var branchId in branchesToAdd)
                {
                    var userBranchesModel = new UserBranchesModel { UserId = SelectedUserId, BranchId = branchId };
                    await _userBranchesService.AddUserToBranchAsync(userBranchesModel);
                }

                // Remove branches
                foreach (var branchId in branchesToRemove)
                {
                    var userBranchesModel = new UserBranchesModel { UserId = SelectedUserId, BranchId = branchId };
                    await _userBranchesService.RemoveUserFromBranchAsync(userBranchesModel);
                }

                TempData["SuccessMessage"] = "تم التعديل بنجاح.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating branch assignments: {ex.Message}";
            }

            return RedirectToAction("AssignBranches");
        }

    }
}
