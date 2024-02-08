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
        private readonly IPurchaseItemsService _purchaseItemsService;
        private readonly IPurchaseInvoiceItemsService _purchaseInvoiceItemsService;

        public PurchaseInvoicesController(IPurchaseInvoicesService purchaseInvoicesService, IPurchaseItemsService purchaseItemsService, IPurchaseInvoiceItemsService purchaseInvoiceItemsService)
        {
            _purchaseInvoicesService = purchaseInvoicesService;
            _purchaseItemsService = purchaseItemsService;
            _purchaseInvoiceItemsService = purchaseInvoiceItemsService;
        }

        public IActionResult PurchasePage()
        {
            return View();
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
        public async Task<IActionResult> AddNewPurchaseInvoice([FromForm] string items, [FromForm] PurchaseInvoicesModel model)
        {
            try
            {
                model.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                model.PurchaseItems = JsonConvert.DeserializeObject<List<PurchaseItemsModel>>(items);

                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

                // Insert the invoice
                int invoiceId = await _purchaseInvoicesService.InsertPurchaseInvoiceAsync(model);
                if (invoiceId <= 0)
                    return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

                // Insert the items and link them to the invoice
                foreach (var item in model.PurchaseItems)
                {
                    item.PurchaseItemId = await _purchaseItemsService.InsertPurchaseItemAsync(item);
                    await _purchaseInvoiceItemsService.AddItemToPurchaseInvoiceAsync(new PurchaseInvoiceItemsModel { PurchaseInvoiceId = invoiceId, PurchaseItemId = item.PurchaseItemId });
                }
                return Json(new { success = true, message = "تم حفظ الفاتورة باصنافها بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري"+ ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePurchaseInvoice([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var purchaseInvoice = await _purchaseInvoicesService.GetPurchaseInvoiceByIdAsync(key);
                JsonConvert.PopulateObject(values, purchaseInvoice);

                purchaseInvoice.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Update the invoice
                int updateResult = await _purchaseInvoicesService.UpdatePurchaseInvoiceAsync(purchaseInvoice);
                if (updateResult <= 0)
                    return BadRequest(new { ErrorMessage = "Could Not Update" });

                return Ok(new { SuccessMessage = "Updated Successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the invoice.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePurchaseInvoice([FromForm] IFormCollection formData)
        {
            try
            {
                var invoiceId = Convert.ToInt32(formData["key"]);

                // Step 1: Retrieve all items associated with the invoice
                var items = await _purchaseItemsService.GetPurchaseItemsByInvoiceIdAsync(invoiceId);

                // Step 2: Remove links from PurchaseInvoiceItems
                foreach (var item in items)
                {
                    await _purchaseInvoiceItemsService.RemoveItemFromPurchaseInvoiceAsync(new PurchaseInvoiceItemsModel { PurchaseInvoiceId = invoiceId, PurchaseItemId = item.PurchaseItemId });
                }

                // Step 3: Delete items from PurchaseItems
                foreach (var item in items)
                {
                    await _purchaseItemsService.DeletePurchaseItemAsync(item.PurchaseItemId);
                }

                // Step 4: Delete the invoice
                int deletePurchaseInvoicesResult = await _purchaseInvoicesService.DeletePurchaseInvoiceAsync(invoiceId);
                if (deletePurchaseInvoicesResult > 0)
                {
                    return Ok(new { SuccessMessage = "Deleted Successfully" });
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Could Not Delete" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

    }
}
