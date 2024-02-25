using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Mvc;

namespace IOMSYS.Controllers
{
    public class InventoryMovementController : Controller
    {
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly IBranchesService _branchesService;

        public InventoryMovementController(IInventoryMovementService inventoryMovementService, IBranchesService branchesService)
        {
            _inventoryMovementService = inventoryMovementService;
            _branchesService = branchesService;
        }

        [HttpGet]
        public async Task<IActionResult> LoadinventoryMovementByToBranch([FromQuery] int BranchId)
        {
            var Items = await _inventoryMovementService.SelectInventoryMovementByToBranchIdAsync(BranchId);
            return Json(Items);
        }

        [HttpGet]
        public async Task<IActionResult> LoadHangingWarehouse()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            int? BranchId = await _branchesService.SelectBranchIdByManagerIdAsync(userId);
            var Items = await _inventoryMovementService.SelectHangingWarehouseByToBranchIdAsync(BranchId);
            return Json(Items);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewinventoryMovement([FromForm] InventoryMovementModel model)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                int? BranchId = await _branchesService.SelectBranchIdByManagerIdAsync(userId);
                if(model.FromBranchId != BranchId)
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية لنقل من هذا الفرع" });
                }

                model.MovementDate = DateTime.Now;
                model.IsApproved = false;

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addPurchaseItemsResult = await _inventoryMovementService.MoveInventoryAsync(model);

                if (addPurchaseItemsResult > 0)
                {
                    return Json(new { success = true, message = "تم نقل الكمية بنجاح" });
                }
                else
                    return Json(new { success = false, message = "حدث خطأ اثناء النقل" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Could not add", ExceptionMessage = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveOrRejectMovement([FromForm] int movementId, [FromForm] bool isApproved)
        {
            try
            {
                var result = await _inventoryMovementService.ApproveOrRejectInventoryMovementAsync(movementId, isApproved);
                if (result)
                {
                    return Json(new { success = true, message = isApproved ? "Movement approved successfully." : "Movement rejected successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Error processing request." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred.", exceptionMessage = ex.Message });
            }
        }
    }
}
