using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IOMSYS.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> LoadReturnSalesItems(int BranchId)
        {
            var Items = await _salesItemsService.GetAllReturnSalesItemsAsync(BranchId);
            return Json(Items);
        }

        [HttpGet]
        public async Task<IActionResult> LoadSaleItemsByInvoiceId([FromQuery] int SalesInvoiceId)
        {
            var Items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(SalesInvoiceId);
            return Json(Items);
        }

        [HttpDelete]
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
                    await RecalculateInvoiceTotalwhenDelete(salesInvoiceId);
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

        [HttpPut]
        public async Task<IActionResult> ReturnSaleItem(int salesItemId)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            try
            {
                // Step 1: Retrieve sales item details
                var salesInvoiceIdModel = await _salesItemsService.GetSalesItemByIdAsync(salesItemId);
                if (salesInvoiceIdModel == null)
                    return BadRequest(new { ErrorMessage = "Sales item not found." });

                int salesInvoiceId = salesInvoiceIdModel.SalesInvoiceId;

                // Step 2: Remove the connection between the invoice and the item
                var removeConnectionResult = await _salesInvoiceItemsService.RemoveSalesItemFromInvoiceAsync(new SalesInvoiceItemsModel
                {
                    SalesInvoiceId = salesInvoiceId,
                    SalesItemId = salesItemId
                });

                // Step 3: Return the sales item
                int deleteSalesItemResult = await _salesItemsService.ReturnSalesItemAsync(salesItemId);
                if (deleteSalesItemResult <= 0)
                    return BadRequest(new { ErrorMessage = "Failed to delete sales item." });

                // Step 4: Adjust inventory quantity, recalculate invoice total, and delete invoice if necessary
                await _branchInventoryService.AdjustInventoryQuantityAsync(salesInvoiceIdModel.ProductId, salesInvoiceIdModel.SizeId, salesInvoiceIdModel.ColorId, (int)salesInvoiceIdModel.BranchId, salesInvoiceIdModel.Quantity);

                // Step 5: Calculate returned amount
                var returnedAmount = salesInvoiceIdModel.Quantity * salesInvoiceIdModel.SellPrice;

                // Step 6: Retrieve sales invoice details
                var salesInvoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(salesInvoiceId);

                // Step 7: Create payment transaction and update sales invoice
                var paymentTransaction = new PaymentTransactionModel
                {
                    BranchId = salesInvoice.BranchId,
                    PaymentMethodId = salesInvoice.PaymentMethodId,
                    Amount = -returnedAmount,
                    TransactionType = "خصم",
                    TransactionDate = DateTime.Now,
                    ModifiedUser = userId,
                    ModifiedDate = DateTime.Now,
                    Details = "مرتجع فاتورة # " + salesInvoice.SalesInvoiceId,
                    InvoiceId = salesInvoice.SalesInvoiceId,
                };
                await _paymentTransactionService.InsertPaymentTransactionAsync(paymentTransaction);
                await RecalculateInvoiceTotal(salesInvoiceId, returnedAmount);
                await DeleteReturnInvoiceIfNoItems(salesInvoiceId);
                return Ok(new { SuccessMessage = "تم الاسترجاع بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

        private async Task<bool> RecalculateInvoiceTotalwhenDelete(int invoiceId)
        {
            try
            {
                var items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(invoiceId);
                var totalAmount = items.Sum(item => item.Quantity * item.SellPrice);

                var invoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(invoiceId);
                if (invoice != null)
                {
                    invoice.TotalAmount = totalAmount;
                    invoice.PaidUp = invoice.TotalAmount;
                    var updateResult = await _salesInvoicesService.UpdateSalesInvoiceAsync(invoice);
                    if (totalAmount > invoice.PaidUp)
                    {
                        invoice.PaidUp = totalAmount - invoice.PaidUp;
                        invoice.Notes = "مرتجع فاتورة رقم #" + invoice.SalesInvoiceId;
                        await RecordPaymentTransaction(invoice, invoice.SalesInvoiceId);
                    }
                    return updateResult > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> RecalculateInvoiceTotal(int invoiceId, decimal returnedAmount)
        {
            try
            {
                var items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(invoiceId);
                var totalAmount = items.Sum(item => item.Quantity * item.SellPrice);

                var invoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(invoiceId);
                if (invoice != null)
                {
                    invoice.TotalAmount = totalAmount;
                    invoice.PaidUp = invoice.PaidUp - returnedAmount;
                    invoice.Remainder = invoice.TotalAmount - invoice.Remainder;
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

        private async Task RecordPaymentTransaction(SalesInvoicesModel model, int invoiceId)
        {
            var paymentTransaction = new PaymentTransactionModel
            {
                BranchId = model.BranchId,
                PaymentMethodId = model.PaymentMethodId,
                Amount = -model.PaidUp,
                TransactionType = "خصم",
                TransactionDate = model.SaleDate,
                ModifiedUser = model.UserId,
                ModifiedDate = DateTime.Now,
                InvoiceId = invoiceId,
                Details = model.Notes,
            };
            await _paymentTransactionService.InsertPaymentTransactionAsync(paymentTransaction);
        }

        public async Task<IActionResult> DeleteInvoiceIfNoItems(int invoiceId)
        {
            try
            {
                var items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(invoiceId);
                if (!items.Any())
                {
                    int deleteResult = await _salesInvoicesService.DeleteSalesInvoiceAsync(invoiceId);
                    if (deleteResult > 0)
                    {
                        var paymentTransactions = await _paymentTransactionService.GetPaymentTransactionsByInvoiceIdAsync(invoiceId);

                        if (paymentTransactions != null && paymentTransactions.Any())
                        {
                            bool deleteFailed = false;

                            foreach (var paymentTransaction in paymentTransactions)
                            {
                                var deleteTransactionResult = await _paymentTransactionService.DeletePaymentTransactionAsync((int)paymentTransaction.TransactionId);

                                if (deleteTransactionResult <= 0)
                                {
                                    deleteFailed = true;
                                    break;
                                }
                            }
                            if (deleteFailed)
                            {
                                return BadRequest(new { ErrorMessage = "Failed to delete one or more related payment transactions." });
                            }
                        }

                        return Ok(new { SuccessMessage = "تم الحذف بنجاح" });
                    }
                    else
                    {
                        return BadRequest(new { ErrorMessage = "حدث خطأ اثناء الحذف." });
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

        public async Task<IActionResult> DeleteReturnInvoiceIfNoItems(int invoiceId)
        {
            try
            {
                var items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(invoiceId);
                int deleteResult = 0;
                if (!items.Any())
                {
                    deleteResult = await _salesInvoicesService.UpdateReturnSalesInvoiceAsync(invoiceId);
                    if (deleteResult > 0)
                    {
                        return Ok(new { SuccessMessage = "Invoice deleted successfully." });
                    }
                    else
                    {
                        return BadRequest(new { ErrorMessage = "Could not delete the invoice." });
                    }
                }
                return Ok(new { SuccessMessage = "Nothing Delete." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

    }
}