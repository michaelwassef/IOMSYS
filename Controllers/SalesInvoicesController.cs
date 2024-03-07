using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class SalesInvoicesController : Controller
    {
        private readonly ISalesInvoicesService _salesInvoicesService;
        private readonly ISalesItemsService _salesItemsService;
        private readonly ISalesInvoiceItemsService _salesInvoiceItemsService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IBranchInventoryService _branchInventoryService;
        private readonly IPermissionsService _permissionsService;
        private readonly IProductsService _productsService;

        public SalesInvoicesController(ISalesInvoicesService salesInvoicesService, ISalesItemsService salesItemsService, ISalesInvoiceItemsService salesInvoiceItemsService, IPaymentTransactionService paymentTransactionService, IBranchInventoryService branchInventoryService, IPermissionsService permissionsService, IProductsService productsService)
        {
            _salesInvoicesService = salesInvoicesService;
            _salesItemsService = salesItemsService;
            _salesInvoiceItemsService = salesInvoiceItemsService;
            _paymentTransactionService = paymentTransactionService;
            _branchInventoryService = branchInventoryService;
            _permissionsService = permissionsService;
            _productsService = productsService;
        }

        public async Task<IActionResult> SalesPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "SalesInvoices", "SalesPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        public async Task<IActionResult> SalesInvoicesPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "SalesInvoices", "SalesInvoicesPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> LoadSalesInvoices()
        {
            var Invoices = await _salesInvoicesService.GetAllSalesInvoicesAsync();
            return Json(Invoices);
        }

        [HttpGet]
        public async Task<IActionResult> LoadSalesInvoicesByBranch(int branchId)
        {
            var Invoices = await _salesInvoicesService.GetAllSalesInvoicesByBranshAsync(branchId);
            return Json(Invoices);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewSaleInvoice([FromForm] string items, [FromForm] SalesInvoicesModel model)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "SalesInvoices", "SalesInvoicesPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            try
            {
                model.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                model.SalesItems = JsonConvert.DeserializeObject<List<SalesItemsModel>>(items);

                //if (!ModelState.IsValid)
                //    return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

                foreach (var item in model.SalesItems)
                {
                    var availableQuantity = await _productsService.GetAvailableQuantity(item.ProductId, item.ColorId, item.SizeId, model.BranchId);
                    if (item.Quantity > availableQuantity)
                    {
                        return Json(new { success = false, message = $"لا يوجد مخزون كافي للمنتج {item.ProductId} بالمقاس {item.SizeId} واللون {item.ColorId} في الفرع {model.BranchId}." });
                    }
                }

                decimal itemsTotal = model.SalesItems.Sum(item => item.Quantity * item.SellPrice);

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
                int invoiceId = await _salesInvoicesService.InsertSalesInvoiceAsync(model);

                if (invoiceId <= 0)
                    return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

                // Insert the items and link them to the invoice and update buyprice
                foreach (var item in model.SalesItems)
                {
                    item.IsReturn = false;
                    item.BranchId = model.BranchId;
                    item.SalesItemId = await _salesItemsService.InsertSalesItemAsync(item);
                    await _salesInvoiceItemsService.AddSalesItemToInvoiceAsync(new SalesInvoiceItemsModel { SalesInvoiceId = invoiceId, SalesItemId = item.SalesItemId });
                    await _branchInventoryService.AdjustInventoryQuantityAsync(item.ProductId, item.SizeId, item.ColorId, model.BranchId, -item.Quantity);
                }

                var paymentTransaction = new PaymentTransactionModel
                {
                    BranchId = model.BranchId,
                    PaymentMethodId = model.PaymentMethodId,
                    Amount = model.PaidUp,
                    TransactionType = "اضافة",
                    TransactionDate = model.SaleDate,
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

        //not finish
        [HttpPut]
        public async Task<IActionResult> UpdateSaleInvoice([FromBody] SalesInvoicesModel SaleInvoice)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "SalesInvoices", "UpdateSaleInvoice");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            try
            {
                SaleInvoice.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                decimal totalItemsAmount = await InvoiceTotalByItems(SaleInvoice.SalesInvoiceId);
                if (totalItemsAmount != SaleInvoice.TotalAmount)
                {
                    return BadRequest(new { ErrorMessage = "اجمالي الفاتورة لا يتوافق مع اجمالي الاصناف" });
                }

                decimal paidUp = SaleInvoice.PaidUp;
                decimal totalAmount = SaleInvoice.TotalAmount;
                decimal remainder = totalAmount - paidUp;

                // Check if PaidUp is less than or equal to TotalAmount
                if (paidUp > totalAmount)
                {
                    return BadRequest(new { ErrorMessage = "المدفوع لا يمكن ان يكون اكبر من اجمالي الفاتورة" });
                }

                // Check if Remainder is less than or equal to TotalAmount and PaidUp + Remainder equals TotalAmount
                if (remainder > totalAmount || paidUp + remainder != totalAmount || remainder != SaleInvoice.Remainder)
                {
                    return BadRequest(new { ErrorMessage = "رجاء مراجعة الباقي من اجمالي الفاتورة" });
                }

                if (paidUp == totalAmount) { SaleInvoice.IsFullPaidUp = true; }
                else { SaleInvoice.IsFullPaidUp = false; }

                // Update the invoice
                int updateResult = await _salesInvoicesService.UpdateSalesInvoiceAsync(SaleInvoice);
                if (updateResult <= 0)
                {
                    return BadRequest(new { ErrorMessage = "Could Not Update" });
                }

                // Retrieve the payment transaction associated with this invoice
                var paymentTransaction = await _paymentTransactionService.GetPaymentTransactionByInvoiceIdAsync(SaleInvoice.SalesInvoiceId);
                if (paymentTransaction != null)
                {
                    // Update transaction details as necessary
                    paymentTransaction.Amount = SaleInvoice.PaidUp;
                    paymentTransaction.BranchId = SaleInvoice.BranchId;
                    paymentTransaction.PaymentMethodId = SaleInvoice.PaymentMethodId;

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
                return Ok(new { SuccessMessage = "Updated Successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the invoice.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSaleInvoice(int invoiceId)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "SalesInvoices", "DeleteSaleInvoice");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }

            try
            {
                // Step 1: Retrieve all items associated with the invoice
                var items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(invoiceId);

                // Step 2: Remove links from SaleInvoiceItems
                foreach (var item in items)
                {
                    await _salesInvoiceItemsService.RemoveSalesItemFromInvoiceAsync(new SalesInvoiceItemsModel { SalesInvoiceId = invoiceId, SalesItemId = item.SalesItemId });
                }

                // Step 3: Delete items from SaleItems
                foreach (var item in items)
                {
                    await _branchInventoryService.AdjustInventoryQuantityAsync(item.ProductId, item.SizeId, item.ColorId, (int)item.BranchId, item.Quantity);
                    await _salesItemsService.DeleteSalesItemAsync(item.SalesItemId);
                }

                // Step 4: Delete the invoice
                int deleteSaleInvoicesResult = await _salesInvoicesService.DeleteSalesInvoiceAsync(invoiceId);
                if (deleteSaleInvoicesResult > 0)
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
            var items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(invoiceId);
            var totalAmount = items.Sum(item => item.Quantity * item.SellPrice);
            return totalAmount;
        }

        [HttpGet]
        public async Task<IActionResult> SaleInvoiceReport(int invoiceId)
        {
            try
            {
                var report = new invoice();
                report.Parameters["InvoiceId"].Value = invoiceId;
                report.CreateDocument();
                MemoryStream memoryStream = new MemoryStream();
                report.ExportToPdf(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Return the PDF file to the client
                return File(memoryStream, "application/pdf", "SaleInvoice.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while generating the report.", ExceptionMessage = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLastInvoiceId()
        {
            try
            {
                var lastInvoiceId = await _salesInvoicesService.GetLastInvoiceIdAsync();
                return Json(new { success = true, lastInvoiceId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving last invoice ID: " + ex.Message });
            }
        }

        public async Task<IActionResult> ReturnSalesPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "SalesInvoices", "ReturnSalesPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        public async Task<IActionResult> ReturnInvoice(int invoiceId)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "SalesInvoices", "ReturnInvoice");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }

            var invoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(invoiceId);
            if (invoice == null)
            {
                return RedirectToAction("SalesPage");
            }

            var items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(invoiceId);
            if (items == null || !items.Any())
            {
                return RedirectToAction("SalesPage");
            }

            invoice.SalesItems = (List<SalesItemsModel>)items;

            return View(invoice);
        }

    }
}
