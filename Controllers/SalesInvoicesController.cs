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
        private readonly ICustomersService _customersService;
        private readonly IPurchaseInvoicesService _purchaseInvoicesService;

        public SalesInvoicesController(ISalesInvoicesService salesInvoicesService, ISalesItemsService salesItemsService, ISalesInvoiceItemsService salesInvoiceItemsService, IPaymentTransactionService paymentTransactionService, IBranchInventoryService branchInventoryService, IPermissionsService permissionsService, IProductsService productsService, ICustomersService customersService, IPurchaseInvoicesService purchaseInvoicesService)
        {
            _salesInvoicesService = salesInvoicesService;
            _salesItemsService = salesItemsService;
            _salesInvoiceItemsService = salesInvoiceItemsService;
            _paymentTransactionService = paymentTransactionService;
            _branchInventoryService = branchInventoryService;
            _permissionsService = permissionsService;
            _productsService = productsService;
            _customersService = customersService;
            _purchaseInvoicesService = purchaseInvoicesService;
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

        //[HttpPost]
        //public async Task<IActionResult> AddNewSaleInvoice([FromForm] string items, [FromForm] SalesInvoicesModel model)
        //{
        //    int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        //    var hasPermission = await _permissionsService.HasPermissionAsync(userId, "SalesInvoices", "SalesInvoicesPage");
        //    if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
        //    try
        //    {
        //        model.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        //        model.SalesItems = JsonConvert.DeserializeObject<List<SalesItemsModel>>(items);

        //        foreach (var item in model.SalesItems)
        //        {
        //            var availableQuantity = await _productsService.GetAvailableQuantity(item.ProductId, item.ColorId, item.SizeId, model.BranchId);
        //            var unit = await _productsService.SelectProductByIdAsync(item.ProductId);
        //            if (unit.UnitId == 1)
        //            {
        //                if (item.Quantity != Math.Floor(item.Quantity))
        //                {
        //                    return Json(new { success = false, message = $"لا يمكن ادخال {unit.ProductName} بهذه الكمية : {item.Quantity}" });
        //                }
        //            }
        //            if (item.Quantity > availableQuantity)
        //            {
        //                return Json(new { success = false, message = $"لا يوجد مخزون كافي للمنتج {item.ProductId} بالمقاس {item.SizeId} واللون {item.ColorId} في الفرع {model.BranchId}." });
        //            }
        //        }

        //        decimal itemsTotal = model.SalesItems.Sum(item => item.Quantity * item.SellPrice);

        //        decimal paidUp = model.PaidUp;
        //        decimal totalAmount = model.TotalAmount;
        //        decimal remainder = totalAmount - paidUp;

        //        // Check if PaidUp is less than or equal to TotalAmount
        //        if (paidUp > totalAmount)
        //        {
        //            return Json(new { success = false, message = "المدفوع لا يمكن ان يكون اكبر من اجمالي الفاتورة" });
        //        }

        //        // Check if Remainder is less than or equal to TotalAmount and PaidUp + Remainder equals TotalAmount
        //        if (remainder > totalAmount || paidUp + remainder != totalAmount || remainder != model.Remainder)
        //        {
        //            return Json(new { success = false, message = "رجاء مراجعة الباقي من اجمالي الفاتورة" });
        //        }

        //        if (paidUp == totalAmount) { model.IsFullPaidUp = true; }
        //        else { model.IsFullPaidUp = false; }

        //        // Insert the invoice
        //        int invoiceId = await _salesInvoicesService.InsertSalesInvoiceAsync(model);

        //        if (invoiceId <= 0)
        //            return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

        //        // Insert the items and link them to the invoice and update buyprice
        //        foreach (var item in model.SalesItems)
        //        {
        //            item.IsReturn = false;
        //            item.BranchId = model.BranchId;
        //            item.ModDate = DateTime.Now;
        //            item.ModUser = userId;
        //            item.SalesItemId = await _salesItemsService.InsertSalesItemAsync(item);
        //            await _salesInvoiceItemsService.AddSalesItemToInvoiceAsync(new SalesInvoiceItemsModel { SalesInvoiceId = invoiceId, SalesItemId = item.SalesItemId });
        //            await _branchInventoryService.AdjustInventoryQuantityAsync(item.ProductId, item.SizeId, item.ColorId, model.BranchId, -item.Quantity);
        //        }

        //        if (model.PaidUp > 0) { await RecordPaymentTransaction(model, invoiceId); }

        //        return Json(new { success = true, message = "تم حفظ الفاتورة باصنافها بنجاح" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" + ex.Message });
        //    }
        //}

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

                foreach (var item in model.SalesItems)
                {
                    var availableQuantity = await _productsService.GetAvailableQuantity(item.ProductId, item.ColorId, item.SizeId, model.BranchId);
                    var unit = await _productsService.SelectProductByIdAsync(item.ProductId);
                    if (unit.UnitId == 1)
                    {
                        if (item.Quantity != Math.Floor(item.Quantity))
                        {
                            return Json(new { success = false, message = $"لا يمكن ادخال {unit.ProductName} بهذه الكمية : {item.Quantity}" });
                        }
                    }
                    if (item.Quantity > availableQuantity)
                    {
                        return Json(new { success = false, message = $"لا يوجد مخزون كافي للمنتج {item.ProductId} بالمقاس {item.SizeId} واللون {item.ColorId} في الفرع {model.BranchId}." });
                    }
                }
                decimal itemsTotal = model.SalesItems.Sum(item => item.Quantity * item.SellPrice);
                decimal willpaidUp = 0;
                var sums = await _customersService.SelectCustomerSumsByIdAsync(model.CustomerId);
                if (sums.RemainderSum > 0)
                {
                    willpaidUp = model.PaidUp;
                    model.PaidUp = 0;
                    model.Remainder = itemsTotal;
                }
                else
                {
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
                }

                // Insert the invoice
                int invoiceId = await _salesInvoicesService.InsertSalesInvoiceAsync(model);
                decimal amountUtilized = await _paymentTransactionService.ProcessInvoicesAndUpdateBalancesS(model.CustomerId, model.BranchId, willpaidUp, model.PaymentMethodId);

                if (invoiceId <= 0)
                    return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

                // Insert the items and link them to the invoice and update buyprice
                foreach (var item in model.SalesItems)
                {
                    item.IsReturn = false;
                    item.BranchId = model.BranchId;
                    item.ModDate = DateTime.Now;
                    item.ModUser = userId;
                    item.SalesItemId = await _salesItemsService.InsertSalesItemAsync(item);
                    await _salesInvoiceItemsService.AddSalesItemToInvoiceAsync(new SalesInvoiceItemsModel { SalesInvoiceId = invoiceId, SalesItemId = item.SalesItemId });
                    await _branchInventoryService.AdjustInventoryQuantityAsync(item.ProductId, item.SizeId, item.ColorId, model.BranchId, -item.Quantity);
                }

                if (model.PaidUp > 0) { await RecordPaymentTransaction(model, invoiceId); }
                return Json(new { success = true, message = "تم حفظ الفاتورة باصنافها بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" + ex.Message });
            }
        }

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
                var oldpaid = await _salesInvoicesService.GetSalesInvoiceByIdAsync(SaleInvoice.SalesInvoiceId);

                decimal paidUp = SaleInvoice.PaidUp;
                decimal totalAmount = SaleInvoice.TotalAmount;
                decimal remainder = totalAmount - paidUp;

                if (paidUp > totalAmount)
                {
                    return BadRequest(new { ErrorMessage = "المدفوع لا يمكن ان يكون اكبر من اجمالي الفاتورة" });
                }

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
                    return BadRequest(new { ErrorMessage = "حدث خطأ اثناء التعديل" });
                }

                if (SaleInvoice.PaidUp > oldpaid.PaidUp)
                {
                    SaleInvoice.PaidUp = SaleInvoice.PaidUp - oldpaid.PaidUp;
                    await RecordPaymentTransaction(SaleInvoice, SaleInvoice.SalesInvoiceId);
                }

                return Ok(new { SuccessMessage = "تم التعديل بنجاح" });
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
                    await _permissionsService.LogActionAsync(userId, "DELETE", "SalesItem", item.SalesItemId, "Delete Sales Item #" + item.SalesItemId + " ProductId : " + item.ProductId + " SizeId : " + item.SizeId + " ColorId : " + item.ColorId + " Qty : " + item.Quantity + " From Invoice #" + invoiceId, (int)item.BranchId);
                    await _salesItemsService.DeleteSalesItemAsync(item.SalesItemId);
                }

                // Step 4: Delete the invoice
                var invoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(invoiceId);
                int deleteSaleInvoicesResult = await _salesInvoicesService.DeleteSalesInvoiceAsync(invoiceId);
                if (deleteSaleInvoicesResult > 0)
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
                    await _permissionsService.LogActionAsync(userId, "DELETE", "SalesInvoices", invoiceId, "Delete Sales Invoice #" + invoiceId + " Customer : " + invoice.CustomerName + " PaymentMethod : " + invoice.PaymentMethodName + " TotalAmount : " + invoice.TotalAmount + " TotalDiscount : " + invoice.TotalDiscount + " PaidUp : " + invoice.PaidUp + " Remainder : " + invoice.Remainder + " SaleDate :" + invoice.SaleDate + " Branch : " + invoice.BranchName, invoice.BranchId);
                    return Ok(new { SuccessMessage = "تم الحذف بنجاح" });
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
        public async Task<IActionResult> SaleInvoiceCusReport(int invoiceId)
        {
            try
            {
                var report = new invoiceCus();
                report.Parameters["InvoiceId"].Value = invoiceId;
                var invoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(invoiceId);
                report.Parameters["CustomerId"].Value = invoice.CustomerId;
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

        private async Task RecordPaymentTransaction(SalesInvoicesModel model, int invoiceId)
        {
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
                Details = "دفعة من فاتورة المبيعات #" + invoiceId + " - " + model.Notes,
            };
            await _paymentTransactionService.InsertPaymentTransactionAsync(paymentTransaction);
        }
    }
}
