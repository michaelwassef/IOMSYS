using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Reports;
using IOMSYS.Services;
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
        private readonly ISalesInvoicesService _salesInvoicesService;
        private readonly ISalesItemsService _salesItemsService;
        private readonly ISalesInvoiceItemsService _salesInvoiceItemsService;

        public PurchaseInvoicesController(IPurchaseInvoicesService purchaseInvoicesService, IPurchaseItemsService purchaseItemsService, IPurchaseInvoiceItemsService purchaseInvoiceItemsService, IProductsService productsService, IPaymentTransactionService paymentTransactionService, IBranchInventoryService branchInventoryService, IPermissionsService permissionsService, ISalesInvoicesService salesInvoicesService, ISalesItemsService salesItemsService, ISalesInvoiceItemsService salesInvoiceItemsService)
        {
            _purchaseInvoicesService = purchaseInvoicesService;
            _purchaseItemsService = purchaseItemsService;
            _purchaseInvoiceItemsService = purchaseInvoiceItemsService;
            _ProductsService = productsService;
            _paymentTransactionService = paymentTransactionService;
            _branchInventoryService = branchInventoryService;
            _permissionsService = permissionsService;
            _salesInvoicesService = salesInvoicesService;
            _salesItemsService = salesItemsService;
            _salesInvoiceItemsService = salesInvoiceItemsService;
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

        public async Task<IActionResult> AllPaymentsNotMade()
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
            var purchaseInvoices = await _purchaseInvoicesService.GetNotPaidPurchaseInvoicesByBranchAsync(PaidUpDate, branchId);
            return Json(purchaseInvoices);
        }

        [HttpGet]
        public async Task<IActionResult> LoadAllNotPaidPurchaseInvoicesByBranch(int branchId)
        {
            var purchaseInvoices = await _purchaseInvoicesService.GetAllNotPaidPurchaseInvoicesByBranchAsync(branchId);
            return Json(purchaseInvoices);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewPurchaseInvoice([FromForm] string items, [FromForm] PurchaseInvoicesModel model)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            bool hasPermission = await _permissionsService.HasPermissionAsync(userId, "PurchaseInvoices", "PurchaseInvoicesPage");
            if (!hasPermission) return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });

            try
            {
                model.UserId = userId;
                model.PurchaseItems = JsonConvert.DeserializeObject<List<PurchaseItemsModel>>(items);

                decimal itemsTotal = model.PurchaseItems.Sum(item => item.Quantity * item.BuyPrice);
                if (itemsTotal != model.TotalAmount)
                    return Json(new { success = false, message = "المجموع الفرعي للأصناف لا يتطابق مع إجمالي المبلغ المعلن في الفاتورة." });

                if (model.PaidUp > model.TotalAmount)
                    return Json(new { success = false, message = "المدفوع لا يمكن ان يكون اكبر من اجمالي الفاتورة" });

                model.Remainder = model.TotalAmount - model.PaidUp;
                model.IsFullPaidUp = model.PaidUp == model.TotalAmount;

                decimal paymentBalance = await _paymentTransactionService.GetBranchAccountBalanceByPaymentAsync(model.BranchId, model.PaymentMethodId);
                if (model.PaidUp > paymentBalance)
                    return Json(new { success = false, message = "لا يوجد رصيد بالخزنة" });

                int invoiceId = await _purchaseInvoicesService.InsertPurchaseInvoiceAsync(model);
                if (invoiceId <= 0)
                    return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

                await ProcessPurchaseItems(model, invoiceId);

                if(model.PaidUp > 0) {
                    model.Notes = "دفعة من فاتورة المشتريات #" + model.PurchaseInvoiceId;
                    await RecordPaymentTransaction(model, invoiceId); }
                
                if (model.SupplierId == 4)
                {
                    await CreateAndLinkSalesInvoice(model, userId, invoiceId);
                }

                return Json(new { success = true, message = "تم حفظ الفاتورة باصنافها بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" + ex.Message });
            }
        }

        private async Task ProcessPurchaseItems(PurchaseInvoicesModel model, int invoiceId)
        {
            foreach (var item in model.PurchaseItems)
            {
                item.BranchId = model.BranchId;
                item.Statues = 0;
                item.ModDate = DateTime.Now;
                item.PurchaseItemId = await _purchaseItemsService.InsertPurchaseItemAsync(item);

                await _purchaseInvoiceItemsService.AddItemToPurchaseInvoiceAsync(new PurchaseInvoiceItemsModel { PurchaseInvoiceId = invoiceId, PurchaseItemId = item.PurchaseItemId });
                await _ProductsService.UpdateProductBuyandSellPriceAsync(item.ProductId, item.BuyPrice, item.SellPrice);
                await _branchInventoryService.AdjustInventoryQuantityAsync(item.ProductId, item.SizeId, item.ColorId, model.BranchId, item.Quantity);
            }
        }

        private async Task RecordPaymentTransaction(PurchaseInvoicesModel model, int invoiceId)
        {
            var paymentTransaction = new PaymentTransactionModel
            {
                BranchId = model.BranchId,
                PaymentMethodId = model.PaymentMethodId,
                Amount = -model.PaidUp,
                TransactionType = "خصم",
                TransactionDate = model.PurchaseDate,
                ModifiedUser = model.UserId,
                ModifiedDate = DateTime.Now,
                InvoiceId = invoiceId,
                Details = model.Notes,
            };
            await _paymentTransactionService.InsertPaymentTransactionAsync(paymentTransaction);
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
                Details = model.Notes,
            };
            await _paymentTransactionService.InsertPaymentTransactionAsync(paymentTransaction);
        }

        private async Task CreateAndLinkSalesInvoice(PurchaseInvoicesModel model, int userId, int invoiceId)
        {
            var salesInvoiceModel = new SalesInvoicesModel
            {
                BranchId = 7,
                PaymentMethodId = 1,
                CustomerId = 3,
                UserId = userId,
                SaleDate = DateTime.Now,
                IsReturn = false,
                IsFullPaidUp = false,
                SalesItems = model.PurchaseItems.Select(item => new SalesItemsModel
                {
                    ProductId = item.ProductId,
                    SizeId = item.SizeId,
                    ColorId = item.ColorId,
                    Quantity = item.Quantity,
                    SellPrice = item.SellPrice,
                    BranchId = 7,
                    IsReturn = false,
                }).ToList()
            };

            salesInvoiceModel.TotalAmount = model.TotalAmount;
            salesInvoiceModel.PaidUp = model.PaidUp;
            salesInvoiceModel.Remainder = model.Remainder;

            int salesInvoiceResult = await AddNewSaleInvoice(salesInvoiceModel);
            if (salesInvoiceResult > 0)
            {
                var existingInvoice = await _purchaseInvoicesService.GetPurchaseInvoiceByIdAsync(invoiceId);
                if (existingInvoice != null)
                {
                    existingInvoice.SalesInvoiceId = salesInvoiceResult;
                    await _purchaseInvoicesService.UpdatePurchaseInvoiceAsync2(existingInvoice);
                }
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
                var oldpaid = existingInvoice.PaidUp;
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

                var paymentBalance = await _paymentTransactionService.GetBranchAccountBalanceByPaymentAsync(existingInvoice.BranchId, existingInvoice.PaymentMethodId);
                if (paidUp > paymentBalance)
                {
                    return BadRequest(new { ErrorMessage = "لا يوجد رصيد بالخزنة" });
                }

                // Update the invoice
                int updateResult = await _purchaseInvoicesService.UpdatePurchaseInvoiceAsync(existingInvoice);

                var saleInvoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(existingInvoice.SalesInvoiceId);
                if (saleInvoice != null)
                {
                    saleInvoice.TotalAmount = existingInvoice.TotalAmount;
                    saleInvoice.PaidUp = existingInvoice.PaidUp;
                    saleInvoice.Remainder = saleInvoice.TotalAmount - saleInvoice.PaidUp;
                    saleInvoice.IsFullPaidUp = saleInvoice.PaidUp == saleInvoice.TotalAmount;

                    int updateSales = await _salesInvoicesService.UpdateSalesInvoiceAsync(saleInvoice);

                    if (saleInvoice.PaidUp > oldpaid) {
                        saleInvoice.Notes = "دفعة من فاتورة المبيعات #" + saleInvoice.SalesInvoiceId;
                        saleInvoice.PaidUp = saleInvoice.PaidUp - oldpaid;
                        await RecordPaymentTransaction(saleInvoice, saleInvoice.SalesInvoiceId); 
                    }                    
                }

                if (updateResult > 0)
                {
                    var updatedInvoice = await _purchaseInvoicesService.GetPurchaseInvoiceByIdAsync(existingInvoice.PurchaseInvoiceId);
                    if (updatedInvoice.PaidUp > oldpaid) 
                    {
                        updatedInvoice.Notes = "دفعة من فاتورة المشتريات #" + existingInvoice.PurchaseInvoiceId;
                        updatedInvoice.PaidUp = updatedInvoice.PaidUp - oldpaid;
                        await RecordPaymentTransaction(updatedInvoice, updatedInvoice.PurchaseInvoiceId); 
                    }

                    return Ok(new { SuccessMessage = "تم تعديل الفاتورة بنجاح.", UpdatedInvoice = updatedInvoice });
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "حدث خطأ اثناء التعديل" });
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
                    return BadRequest(new { ErrorMessage = "حدث خطأ اثناء الحذف" });
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

        public async Task<int> AddNewSaleInvoice(SalesInvoicesModel model)
        {
            try
            {
                int invoiceId = await _salesInvoicesService.InsertSalesInvoiceAsync(model);

                if (invoiceId <= 0)
                    return 0;

                foreach (var item in model.SalesItems)
                {
                    item.SalesItemId = await _salesItemsService.InsertSalesItemAsync(item);
                    await _salesInvoiceItemsService.AddSalesItemToInvoiceAsync(new SalesInvoiceItemsModel { SalesInvoiceId = invoiceId, SalesItemId = item.SalesItemId });
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
                    Details = "نقل منتجات من المصنع لفرع",
                };

                await _paymentTransactionService.InsertPaymentTransactionAsync(paymentTransaction);
                return invoiceId;
            }
            catch
            {
                return 0;
            }
        }
    }
}
