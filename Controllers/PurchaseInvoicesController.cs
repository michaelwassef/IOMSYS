using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class PurchaseInvoicesController : Controller
    {
        private readonly IPurchaseInvoicesService _purchaseInvoicesService;

        public PurchaseInvoicesController(IPurchaseInvoicesService purchaseInvoicesService)
        {
            _purchaseInvoicesService = purchaseInvoicesService;
        }

        public IActionResult PurchaseInvoicesPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadPurchaseInvoices()
        {
            var purchaseInvoices = await _purchaseInvoicesService.GetAllPurchaseInvoicesAsync();
            return Json(purchaseInvoices);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewPurchaseInvoice([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var newPurchaseInvoices = new PurchaseInvoicesModel();
                JsonConvert.PopulateObject(values, newPurchaseInvoices);
                newPurchaseInvoices.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addPurchaseInvoicesResult = await _purchaseInvoicesService.InsertPurchaseInvoiceAsync(newPurchaseInvoices);

                if (addPurchaseInvoicesResult > 0)
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
        public async Task<IActionResult> UpdatePurchaseInvoice([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var PurchaseInvoices = await _purchaseInvoicesService.GetPurchaseInvoiceByIdAsync(key);
                JsonConvert.PopulateObject(values, PurchaseInvoices);

                PurchaseInvoices.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updatePurchaseInvoicesResult = await _purchaseInvoicesService.UpdatePurchaseInvoiceAsync(PurchaseInvoices);

                if (updatePurchaseInvoicesResult > 0)
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
                return BadRequest(new { ErrorMessage = "An error occurred while updating the PurchaseInvoices.", ExceptionMessage = ex.Message });
            }
        }


        [HttpDelete]
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeletePurchaseInvoice([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deletePurchaseInvoicesResult = await _purchaseInvoicesService.DeletePurchaseInvoiceAsync(key);
                if (deletePurchaseInvoicesResult > 0)
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
