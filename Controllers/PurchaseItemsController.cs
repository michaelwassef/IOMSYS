using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class PurchaseItemsController : Controller
    {
        private readonly IPurchaseItemsService _PurchaseItemsService;

        public PurchaseItemsController(IPurchaseItemsService PurchaseItemsService)
        {
            _PurchaseItemsService = PurchaseItemsService;
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
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeletePurchaseItem([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deletePurchaseItemsResult = await _PurchaseItemsService.DeletePurchaseItemAsync(key);
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
