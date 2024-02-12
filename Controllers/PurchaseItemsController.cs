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
        private readonly IPurchaseInvoiceItemsService _purchaseInvoiceItemsService;
        private readonly IPurchaseInvoicesService _purchaseInvoicesService;
        private readonly IProductsService _ProductsService;

        public PurchaseItemsController(IPurchaseItemsService PurchaseItemsService, IPurchaseInvoiceItemsService purchaseInvoiceItemsService, IPurchaseInvoicesService purchaseInvoicesService, IProductsService productsService)
        {
            _PurchaseItemsService = PurchaseItemsService;
            _purchaseInvoiceItemsService = purchaseInvoiceItemsService;
            _purchaseInvoicesService = purchaseInvoicesService;
            _ProductsService = productsService;
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

        //[HttpPost]
        //public async Task<IActionResult> AddNewPurchaseItem([FromForm] IFormCollection formData)
        //{
        //    try
        //    {
        //        var values = formData["values"];
        //        var newPurchaseItems = new PurchaseItemsModel();
        //        JsonConvert.PopulateObject(values, newPurchaseItems);

        //        if (!ModelState.IsValid)
        //            return BadRequest(ModelState);

        //        int addPurchaseItemsResult = await _PurchaseItemsService.InsertPurchaseItemAsync(newPurchaseItems);

        //        if (addPurchaseItemsResult > 0)
        //        {
        //            await _ProductsService.UpdateProductBuyandSellPriceAsync(newPurchaseItems.ProductId, newPurchaseItems.BuyPrice, newPurchaseItems.SellPrice);
        //            return Ok(new { SuccessMessage = "Successfully Added" });
        //        }
        //        else
        //            return BadRequest(new { ErrorMessage = "Could Not Add" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { ErrorMessage = "Could not add", ExceptionMessage = ex.Message });
        //    }
        //}

        //[HttpPut]
        //public async Task<IActionResult> UpdatePurchaseItem([FromForm] IFormCollection formData)
        //{
        //    try
        //    {
        //        var key = Convert.ToInt32(formData["key"]);
        //        var values = formData["values"];
        //        var PurchaseItems = await _PurchaseItemsService.GetPurchaseItemByIdAsync(key);
        //        JsonConvert.PopulateObject(values, PurchaseItems);

        //        if (!ModelState.IsValid)
        //            return BadRequest(ModelState);

        //        int updatePurchaseItemsResult = await _PurchaseItemsService.UpdatePurchaseItemAsync(PurchaseItems);

        //        if (updatePurchaseItemsResult > 0)
        //        {
        //            return Ok(new { SuccessMessage = "Updated Successfully" });
        //        }
        //        else
        //        {
        //            return BadRequest(new { ErrorMessage = "Could Not Update" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { ErrorMessage = "An error occurred while updating the PurchaseItems.", ExceptionMessage = ex.Message });
        //    }
        //}

        [HttpDelete]
        [Authorize(Roles = "GenralManager")]
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
                {
                    await RecalculateInvoiceTotal(purchaseInvoiceId);
                    await DeleteInvoiceIfNoItems(purchaseInvoiceId);
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

        private async Task<bool> RecalculateInvoiceTotal(int invoiceId)
        {
            try
            {
                var items = await _PurchaseItemsService.GetPurchaseItemsByInvoiceIdAsync(invoiceId);
                var totalAmount = items.Sum(item => item.Quantity * item.BuyPrice);

                var invoice = await _purchaseInvoicesService.GetPurchaseInvoiceByIdAsync(invoiceId);
                if (invoice != null)
                {
                    invoice.TotalAmount = totalAmount;
                    var updateResult = await _purchaseInvoicesService.UpdatePurchaseInvoiceAsync(invoice);
                    return updateResult > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IActionResult> DeleteInvoiceIfNoItems(int invoiceId)
        {
            try
            {
                // Step 1: Check if any items are associated with this invoice
                var items = await _PurchaseItemsService.GetPurchaseItemsByInvoiceIdAsync(invoiceId);

                // Step 2: If no items are associated, proceed to delete the invoice
                int deleteResult = await _purchaseInvoicesService.DeletePurchaseInvoiceAsync(invoiceId);
                if (deleteResult > 0)
                {
                    return Ok(new { SuccessMessage = "Invoice deleted successfully." });
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Could not delete the invoice." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

    }
}
