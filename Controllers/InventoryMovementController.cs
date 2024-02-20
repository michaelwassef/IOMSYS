using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    public class InventoryMovementController : Controller
    {
        private readonly IInventoryMovementService _inventoryMovementService;


        public InventoryMovementController(IInventoryMovementService inventoryMovementService)
        {
            _inventoryMovementService = inventoryMovementService;
        }

        [HttpGet]
        public async Task<IActionResult> LoadinventoryMovementByToBranch([FromQuery] int BranchId)
        {
            var Items = await _inventoryMovementService.SelectInventoryMovementByToBranchIdAsync(BranchId);
            return Json(Items);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewinventoryMovement([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var model = new InventoryMovementModel();
                JsonConvert.PopulateObject(values, model);

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
    }
}
