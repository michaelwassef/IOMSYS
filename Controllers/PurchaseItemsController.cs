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
        private readonly IBranchInventoryService _branchInventoryService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IBranchesService _branchesService;

        public PurchaseItemsController(IPurchaseItemsService PurchaseItemsService, IPurchaseInvoiceItemsService purchaseInvoiceItemsService, IPurchaseInvoicesService purchaseInvoicesService, IProductsService productsService, IBranchInventoryService branchInventoryService, IPaymentTransactionService paymentTransactionService, IBranchesService branchesService)
        {
            _PurchaseItemsService = PurchaseItemsService;
            _purchaseInvoiceItemsService = purchaseInvoiceItemsService;
            _purchaseInvoicesService = purchaseInvoicesService;
            _ProductsService = productsService;
            _branchInventoryService = branchInventoryService;
            _paymentTransactionService = paymentTransactionService;
            _branchesService = branchesService;
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

                newPurchaseItems.ModDate = DateTime.Now;

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                int? BranchId = await _branchesService.SelectBranchIdByManagerIdAsync(userId);

                if (newPurchaseItems.BranchId != BranchId)
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية للاضافة لهذا الفرع" });
                }

                int addPurchaseItemsResult = await _PurchaseItemsService.InsertPurchaseItemAsync(newPurchaseItems);

                if (addPurchaseItemsResult > 0)
                {
                    if (newPurchaseItems.SellPrice != 0 || newPurchaseItems.BuyPrice != 0)
                    {
                        await _ProductsService.UpdateProductBuyandSellPriceAsync(newPurchaseItems.ProductId, newPurchaseItems.BuyPrice, newPurchaseItems.SellPrice);
                    }

                    await _branchInventoryService.AdjustInventoryQuantityAsync(
                           newPurchaseItems.ProductId,
                           newPurchaseItems.SizeId,
                           newPurchaseItems.ColorId,
                           newPurchaseItems.BranchId,
                           newPurchaseItems.Quantity);

                    return Json(new { success = true, message = "تم الادخال بنجاح وتم تحديث المخزون." });
                }
                else
                    return Json(new { success = false, message = "حدث خطأ اثناء الادخال" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Could not add", ExceptionMessage = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePurchaseItem([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var existingPurchaseItem = await _PurchaseItemsService.GetPurchaseItemByIdAsync(key);
                if (existingPurchaseItem == null)
                {
                    return NotFound(new { ErrorMessage = "Purchase item not found." });
                }

                var newPurchaseItem = new PurchaseItemsModel();
                JsonConvert.PopulateObject(values, newPurchaseItem);

                newPurchaseItem.ModDate = DateTime.Now;

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int quantityDifference = newPurchaseItem.Quantity - existingPurchaseItem.Quantity;

                int updatePurchaseItemsResult = await _PurchaseItemsService.UpdatePurchaseItemAsync(newPurchaseItem);

                if (updatePurchaseItemsResult > 0)
                {
                    if (quantityDifference != 0)
                    {
                        await _branchInventoryService.AdjustInventoryQuantityAsync(
                            newPurchaseItem.ProductId,
                            newPurchaseItem.SizeId,
                            newPurchaseItem.ColorId,
                            newPurchaseItem.BranchId,
                            quantityDifference);
                    }

                    return Ok(new { SuccessMessage = "Updated Successfully, inventory adjusted." });
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
                var purchaseItemId = Convert.ToInt32(formData["key"]);
                var purchaseItem = await _PurchaseItemsService.GetPurchaseItemByIdAsync(purchaseItemId);
                var purchaseInvoiceId = purchaseItem.PurchaseInvoiceId;

                if (purchaseItem == null)
                {
                    return NotFound(new { ErrorMessage = "Purchase item not found." });
                }

                var removeConnectionResult = await _purchaseInvoiceItemsService.RemoveItemFromPurchaseInvoiceAsync(new PurchaseInvoiceItemsModel
                {
                    PurchaseInvoiceId = purchaseInvoiceId,
                    PurchaseItemId = purchaseItemId
                });

                int deletePurchaseItemsResult = await _PurchaseItemsService.DeletePurchaseItemAsync(purchaseItemId);
                if (deletePurchaseItemsResult > 0)
                {
                    await _branchInventoryService.AdjustInventoryQuantityAsync(
                        purchaseItem.ProductId,
                        purchaseItem.SizeId,
                        purchaseItem.ColorId,
                        purchaseItem.BranchId,
                        -purchaseItem.Quantity);

                    await RecalculateInvoiceTotal(purchaseInvoiceId);
                    await DeleteInvoiceIfNoItems(purchaseInvoiceId);

                    return Ok(new { SuccessMessage = "Deleted Successfully, inventory adjusted." });
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
                    var paymentTransaction = await _paymentTransactionService.GetPaymentTransactionByInvoiceIdAsync(invoiceId);
                    if (paymentTransaction != null)
                    {
                        // Delete the payment transaction
                        var deleteTransactionResult = await _paymentTransactionService.DeletePaymentTransactionAsync((int)paymentTransaction.TransactionId);
                        if (deleteTransactionResult <= 0)
                        {
                            return BadRequest(new { ErrorMessage = "Failed to delete the related payment transaction." });
                        }
                    }
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
