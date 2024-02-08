using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class PurchaseItemsController : Controller
    {
        private readonly IPurchaseItemsService _PurchaseItemsService;
        private readonly IPurchaseInvoiceItemsService _purchaseInvoiceItemsService;

        public PurchaseItemsController(IPurchaseItemsService PurchaseItemsService, IPurchaseInvoiceItemsService purchaseInvoiceItemsService)
        {
            _PurchaseItemsService = PurchaseItemsService;
            _purchaseInvoiceItemsService = purchaseInvoiceItemsService;
        }

        public IActionResult PurchaseItemsPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadPurchaseItems()
        {
            var PurchaseItems = await _PurchaseItemsService.GetAllPurchaseItemsAsync();
            return Json(PurchaseItems);
        }

        [HttpGet]
        public async Task<IActionResult> LoadPurchaseItemsByInvoiceId([FromQuery] int purchaseInvoiceId)
        {
            var purchaseItems = await _PurchaseItemsService.GetPurchaseItemsByInvoiceIdAsync(purchaseInvoiceId);
            return Json(purchaseItems);
        }


        [HttpPost]
        public async Task<IActionResult> AddNewPurchaseItem([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var newPurchaseItems = new PurchaseItemsModel();
                JsonConvert.PopulateObject(values, newPurchaseItems);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addPurchaseItemsResult = await _PurchaseItemsService.InsertPurchaseItemAsync(newPurchaseItems);

                if (addPurchaseItemsResult > 0)
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
        public async Task<IActionResult> UpdatePurchaseItem([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var PurchaseItems = await _PurchaseItemsService.GetPurchaseItemByIdAsync(key);
                JsonConvert.PopulateObject(values, PurchaseItems);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updatePurchaseItemsResult = await _PurchaseItemsService.UpdatePurchaseItemAsync(PurchaseItems);

                if (updatePurchaseItemsResult > 0)
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
                return BadRequest(new { ErrorMessage = "An error occurred while updating the PurchaseItems.", ExceptionMessage = ex.Message });
            }
        }


        [HttpDelete]
        public async Task<IActionResult> DeletePurchaseItem([FromForm] IFormCollection formData)
        {
            try
            {
                // Assume formData contains both the purchase item ID and the associated invoice ID
                var purchaseItemId = Convert.ToInt32(formData["key"]);
                var purchaseInvoiceIdModel = await _PurchaseItemsService.GetPurchaseItemByIdAsync(purchaseItemId);
                var purchaseInvoiceId = purchaseInvoiceIdModel.PurchaseInvoiceId;

                // Step 1: Remove the connection between the invoice and the item
                var removeConnectionResult = await _purchaseInvoiceItemsService.RemoveItemFromPurchaseInvoiceAsync(new PurchaseInvoiceItemsModel
                {
                    PurchaseInvoiceId = purchaseInvoiceId,
                    PurchaseItemId = purchaseItemId
                });

                // Step 2: Delete the purchase item
                int deletePurchaseItemsResult = await _PurchaseItemsService.DeletePurchaseItemAsync(purchaseItemId);
                if (deletePurchaseItemsResult > 0)
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
