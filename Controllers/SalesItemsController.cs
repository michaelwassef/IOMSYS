using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IOMSYS.Controllers
{
    public class SalesItemsController : Controller
    {
        private readonly ISalesInvoicesService _salesInvoicesService;
        private readonly ISalesItemsService _salesItemsService;
        private readonly ISalesInvoiceItemsService _salesInvoiceItemsService;
        private readonly IBranchInventoryService _branchInventoryService;
        private readonly IPaymentTransactionService _paymentTransactionService;

        public SalesItemsController(ISalesInvoicesService salesInvoicesService, ISalesItemsService salesItemsService, ISalesInvoiceItemsService salesInvoiceItemsService, IBranchInventoryService branchInventoryService, IPaymentTransactionService paymentTransactionService)
        {
            _salesInvoicesService = salesInvoicesService;
            _salesInvoiceItemsService = salesInvoiceItemsService;
            _salesItemsService = salesItemsService;
            _branchInventoryService = branchInventoryService;
            _paymentTransactionService = paymentTransactionService;
        }

        [HttpGet]
        public async Task<IActionResult> LoadSalesItems()
        {
            var Items = await _salesItemsService.GetAllSalesItemsAsync();
            return Json(Items);
        }

        [HttpGet]
        public async Task<IActionResult> LoadSaleItemsByInvoiceId([FromQuery] int SalesInvoiceId)
        {
            var Items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(SalesInvoiceId);
            return Json(Items);
        }

        [HttpDelete]
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeleteSaleItem([FromForm] IFormCollection formData)
        {
            try
            {
                // Assume formData contains both the sales item ID and the associated invoice ID
                var salesItemId = Convert.ToInt32(formData["key"]);
                var salesInvoiceIdModel = await _salesItemsService.GetSalesItemByIdAsync(salesItemId);
                int salesInvoiceId = salesInvoiceIdModel.SalesInvoiceId;

                // Step 1: Remove the connection between the invoice and the item
                var removeConnectionResult = await _salesInvoiceItemsService.RemoveSalesItemFromInvoiceAsync(new SalesInvoiceItemsModel
                {
                    SalesInvoiceId = salesInvoiceId,
                    SalesItemId = salesItemId
                });

                // Step 2: Delete the sales item
                int deletesalesItemsResult = await _salesItemsService.DeleteSalesItemAsync(salesItemId);
                if (deletesalesItemsResult > 0)
                {
                    await _branchInventoryService.AdjustInventoryQuantityAsync(salesInvoiceIdModel.ProductId, salesInvoiceIdModel.SizeId, salesInvoiceIdModel.ColorId, (int)salesInvoiceIdModel.BranchId, salesInvoiceIdModel.Quantity);
                    await RecalculateInvoiceTotal(salesInvoiceId);
                    await DeleteInvoiceIfNoItems(salesInvoiceId);
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
                var items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(invoiceId);
                var totalAmount = items.Sum(item => item.Quantity * item.SellPrice);

                var invoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(invoiceId);
                if (invoice != null)
                {
                    invoice.TotalAmount = totalAmount;
                    var updateResult = await _salesInvoicesService.UpdateSalesInvoiceAsync(invoice);
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
                var items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(invoiceId);

                // Step 2: If no items are associated, proceed to delete the invoice
                int deleteResult = await _salesInvoicesService.DeleteSalesInvoiceAsync(invoiceId);
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
