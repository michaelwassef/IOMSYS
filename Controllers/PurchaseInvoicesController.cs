using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class PurchaseInvoicesController : Controller
    {
        private readonly IPurchaseInvoicesService _purchaseInvoicesService;
        private readonly IPurchaseItemsService _purchaseItemsService;
        private readonly IPurchaseInvoiceItemsService _purchaseInvoiceItemsService;
        private readonly IProductsService _ProductsService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IBranchInventoryService _branchInventoryService;
        private readonly IPermissionsService _permissionsService;

        public PurchaseInvoicesController(IPurchaseInvoicesService purchaseInvoicesService, IPurchaseItemsService purchaseItemsService, IPurchaseInvoiceItemsService purchaseInvoiceItemsService, IProductsService productsService, IPaymentTransactionService paymentTransactionService, IBranchInventoryService branchInventoryService, IPermissionsService permissionsService)
        {
            _purchaseInvoicesService = purchaseInvoicesService;
            _purchaseItemsService = purchaseItemsService;
            _purchaseInvoiceItemsService = purchaseInvoiceItemsService;
            _ProductsService = productsService;
            _paymentTransactionService = paymentTransactionService;
            _branchInventoryService = branchInventoryService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> PurchasePage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PurchaseInvoices", "PurchasePage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        public async Task<IActionResult> PurchaseInvoicesPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PurchaseInvoices", "PurchaseInvoicesPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        public async Task<IActionResult> PaymentsNotMade()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PurchaseInvoices", "PaymentsNotMade");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadPurchaseInvoices()
        {
            var purchaseInvoices = await _purchaseInvoicesService.GetAllPurchaseInvoicesAsync();
            return Json(purchaseInvoices);
        }

        [HttpGet]
        public async Task<IActionResult> LoadPurchaseInvoicesByBranch(int branchId)
        {
            var purchaseInvoices = await _purchaseInvoicesService.GetAllPurchaseInvoicesByBranchAsync(branchId);
            return Json(purchaseInvoices);
        }

        [HttpGet]
        public async Task<IActionResult> LoadNotPaidPurchaseInvoicesByBranch(DateTime PaidUpDate, int branchId)
        {
            var purchaseInvoices = await _purchaseInvoicesService.GetAllNotPaidPurchaseInvoicesByBranchAsync(PaidUpDate, branchId);
            return Json(purchaseInvoices);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewPurchaseInvoice([FromForm] string items, [FromForm] PurchaseInvoicesModel model)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PurchaseInvoices", "PurchaseInvoicesPage");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }

            try
            {
                model.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                model.PurchaseItems = JsonConvert.DeserializeObject<List<PurchaseItemsModel>>(items);

                //if (!ModelState.IsValid)
                //    return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

                decimal itemsTotal = model.PurchaseItems.Sum(item => item.Quantity * item.BuyPrice);

                // Compare itemsTotal with the TotalAmount of the invoice
                if (itemsTotal != model.TotalAmount)
                {
                    return Json(new { success = false, message = "المجموع الفرعي للأصناف لا يتطابق مع إجمالي المبلغ المعلن في الفاتورة." });
                }

                decimal paidUp = model.PaidUp;
                decimal totalAmount = model.TotalAmount;
                decimal remainder = totalAmount - paidUp;

                // Check if PaidUp is less than or equal to TotalAmount
                if (paidUp > totalAmount)
                {
                    return Json(new { success = false, message = "المدفوع لا يمكن ان يكون اكبر من اجمالي الفاتورة" });
                }

                // Check if Remainder is less than or equal to TotalAmount and PaidUp + Remainder equals TotalAmount
                if (remainder > totalAmount || paidUp + remainder != totalAmount || remainder != model.Remainder)
                {
                    return Json(new { success = false, message = "رجاء مراجعة الباقي من اجمالي الفاتورة" });
                }

                if (paidUp == totalAmount) { model.IsFullPaidUp = true; }
                else { model.IsFullPaidUp = false; }

                // Insert the invoice
                int invoiceId = await _purchaseInvoicesService.InsertPurchaseInvoiceAsync(model);
                if (invoiceId <= 0)
                    return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

                // Insert the items and link them to the invoice and update buyprice
                foreach (var item in model.PurchaseItems)
                {
                    item.BranchId = model.BranchId;
                    item.Statues = 1;
                    item.ModDate = DateTime.Now;
                    item.PurchaseItemId = await _purchaseItemsService.InsertPurchaseItemAsync(item);
                    await _purchaseInvoiceItemsService.AddItemToPurchaseInvoiceAsync(new PurchaseInvoiceItemsModel { PurchaseInvoiceId = invoiceId, PurchaseItemId = item.PurchaseItemId });
                    await _ProductsService.UpdateProductBuyandSellPriceAsync(item.ProductId, item.BuyPrice, item.SellPrice);
                    await _branchInventoryService.AdjustInventoryQuantityAsync(
                        item.ProductId,
                        item.SizeId,
                        item.ColorId,
                        model.BranchId,
                        item.Quantity);
                }

                var paymentTransaction = new PaymentTransactionModel
                {
                    BranchId = model.BranchId,
                    PaymentMethodId = model.PaymentMethodId,
                    Amount = model.PaidUp,
                    TransactionType = "خصم",
                    TransactionDate = model.PurchaseDate,
                    ModifiedUser = model.UserId,
                    ModifiedDate = DateTime.Now,
                    InvoiceId = invoiceId,
                };
                await _paymentTransactionService.InsertPaymentTransactionAsync(paymentTransaction);

                return Json(new { success = true, message = "تم حفظ الفاتورة باصنافها بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" + ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePurchaseInvoice(int id, [FromBody] JsonElement data)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PurchaseInvoices", "UpdatePurchaseInvoice");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }

            try
            {
                var existingInvoice = await _purchaseInvoicesService.GetPurchaseInvoiceByIdAsync(id);

                if (data.TryGetProperty("values", out JsonElement valuesElement))
                {
                    var values = valuesElement.ToString();
                    JsonConvert.PopulateObject(values, existingInvoice);
                }

                decimal totalItemsAmount = await InvoiceTotalByItems(existingInvoice.PurchaseInvoiceId);
                decimal paidUp = existingInvoice.PaidUp;

                if (existingInvoice.PaidUp != null)
                {
                    decimal remainder = totalItemsAmount - paidUp;
                    if (paidUp > totalItemsAmount)
                    {
                        return BadRequest(new { ErrorMessage = "المدفوع لا يمكن ان يكون اكبر من اجمالي الفاتورة" });
                    }
                    if (remainder > totalItemsAmount || paidUp + remainder != totalItemsAmount || remainder != existingInvoice.Remainder)
                    {
                        return BadRequest(new { ErrorMessage = "رجاء مراجعة الباقي من اجمالي الفاتورة" });
                    }
                }

                if (paidUp == totalItemsAmount) { existingInvoice.IsFullPaidUp = true; }
                else { existingInvoice.IsFullPaidUp = false; }

                // Update the invoice
                int updateResult = await _purchaseInvoicesService.UpdatePurchaseInvoiceAsync(existingInvoice);
                if (updateResult > 0)
                {
                    // Retrieve the complete updated purchase invoice
                    var updatedInvoice = await _purchaseInvoicesService.GetPurchaseInvoiceByIdAsync(existingInvoice.PurchaseInvoiceId);

                    // Retrieve the payment transaction associated with this invoice
                    var paymentTransaction = await _paymentTransactionService.GetPaymentTransactionByInvoiceIdAsync(existingInvoice.PurchaseInvoiceId);
                    if (paymentTransaction != null)
                    {
                        // Update transaction details as necessary
                        paymentTransaction.Amount = existingInvoice.PaidUp;
                        paymentTransaction.BranchId = existingInvoice.BranchId;
                        paymentTransaction.PaymentMethodId = existingInvoice.PaymentMethodId;

                        var updateTransactionResult = await _paymentTransactionService.UpdatePaymentTransactionAsync(paymentTransaction);
                        if (updateTransactionResult <= 0)
                        {
                            return BadRequest(new { ErrorMessage = "Could not update the related payment transaction." });
                        }
                    }
                    else
                    {
                        return BadRequest(new { ErrorMessage = "No related payment transaction found for update." });
                    }

                    return Ok(new { SuccessMessage = "Invoice and related payment transaction updated successfully.", UpdatedInvoice = updatedInvoice });
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Could Not Update Invoice" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the invoice.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePurchaseInvoice(int invoiceId)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PurchaseInvoices", "DeletePurchaseInvoice");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }

            try
            {
                // Step 1: Retrieve all items associated with the invoice
                var items = await _purchaseItemsService.GetPurchaseItemsByInvoiceIdAsync(invoiceId);

                // Step 2: Remove links from PurchaseInvoiceItems
                foreach (var item in items)
                {
                    await _branchInventoryService.AdjustInventoryQuantityAsync(
                        item.ProductId,
                        item.SizeId,
                        item.ColorId,
                        item.BranchId,
                        -item.Quantity);

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

        private async Task<decimal> InvoiceTotalByItems(int invoiceId)
        {
            var items = await _purchaseItemsService.GetPurchaseItemsByInvoiceIdAsync(invoiceId);
            var totalAmount = items.Sum(item => item.Quantity * item.BuyPrice);
            return totalAmount;
        }
    }
}
