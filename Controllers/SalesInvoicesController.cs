using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class SalesInvoicesController : Controller
    {
        private readonly ISalesInvoicesService _salesInvoicesService;
        private readonly ISalesItemsService _salesItemsService;
        private readonly ISalesInvoiceItemsService _salesInvoiceItemsService;
        private readonly IProductsService _productsService;
        private readonly IPaymentTransactionService _paymentTransactionService;

        public SalesInvoicesController(ISalesInvoicesService salesInvoicesService, ISalesItemsService salesItemsService, ISalesInvoiceItemsService salesInvoiceItemsService, IProductsService productsService, IPaymentTransactionService paymentTransactionService)
        {
            _salesInvoicesService = salesInvoicesService;
            _salesItemsService = salesItemsService;
            _salesInvoiceItemsService = salesInvoiceItemsService;
            _productsService = productsService;
            _paymentTransactionService = paymentTransactionService;
        }

        public IActionResult SalesPage()
        {
            return View();
        }

        public IActionResult SalesInvoicesPage()
        {
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
            try
            {
                model.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                model.SalesItems = JsonConvert.DeserializeObject<List<SalesItemsModel>>(items);

                //if (!ModelState.IsValid)
                //    return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

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

                // Insert the invoice
                int invoiceId = await _salesInvoicesService.InsertSalesInvoiceAsync(model);

                if (invoiceId <= 0)
                    return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

                // Insert the items and link them to the invoice and update buyprice
                foreach (var item in model.SalesItems)
                {
                    item.SalesItemId = await _salesItemsService.InsertSalesItemAsync(item);
                    await _salesInvoiceItemsService.AddSalesItemToInvoiceAsync(new SalesInvoiceItemsModel { SalesInvoiceId = invoiceId, SalesItemId = item.SalesItemId });
                }

                var paymentTransaction = new PaymentTransactionModel
                {
                    BranchId = model.BranchId,
                    PaymentMethodId = model.PaymentMethodId,
                    Amount = model.TotalAmount,
                    TransactionType = "اضافة",
                    TransactionDate = model.SaleDate,
                    ModifiedUser = model.UserId,
                    ModifiedDate = DateTime.Now
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
        public async Task<IActionResult> UpdateSaleInvoice([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var SaleInvoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(key);
                JsonConvert.PopulateObject(values, SaleInvoice);

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

                // Update the invoice
                int updateResult = await _salesInvoicesService.UpdateSalesInvoiceAsync(SaleInvoice);
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
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeleteSaleInvoice([FromForm] IFormCollection formData)
        {
            try
            {
                var invoiceId = Convert.ToInt32(formData["key"]);

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
                    await _salesItemsService.DeleteSalesItemAsync(item.SalesItemId);
                }

                // Step 4: Delete the invoice
                int deleteSaleInvoicesResult = await _salesInvoicesService.DeleteSalesInvoiceAsync(invoiceId);
                if (deleteSaleInvoicesResult > 0)
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
                var report = new SaleInvoice();
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
    }
}
