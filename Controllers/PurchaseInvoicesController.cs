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
        private readonly IProductsService _ProductsService;

        public PurchaseInvoicesController(IPurchaseInvoicesService purchaseInvoicesService, IPurchaseItemsService purchaseItemsService, IPurchaseInvoiceItemsService purchaseInvoiceItemsService, IProductsService productsService)
        {
            _purchaseInvoicesService = purchaseInvoicesService;
            _purchaseItemsService = purchaseItemsService;
            _purchaseInvoiceItemsService = purchaseInvoiceItemsService;
            _ProductsService = productsService;
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

                // Insert the invoice
                int invoiceId = await _purchaseInvoicesService.InsertPurchaseInvoiceAsync(model);
                if (invoiceId <= 0)
                    return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" });

                // Insert the items and link them to the invoice and update buyprice
                foreach (var item in model.PurchaseItems)
                {
                    item.PurchaseItemId = await _purchaseItemsService.InsertPurchaseItemAsync(item);
                    await _purchaseInvoiceItemsService.AddItemToPurchaseInvoiceAsync(new PurchaseInvoiceItemsModel { PurchaseInvoiceId = invoiceId, PurchaseItemId = item.PurchaseItemId });
                    await _ProductsService.UpdateProductBuyandSellPriceAsync(item.ProductId, item.BuyPrice,item.SellPrice);
                }
                return Json(new { success = true, message = "تم حفظ الفاتورة باصنافها بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ ما اثناء الاضافه حاول مرة اخري" + ex.Message });
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

                decimal paidUp = purchaseInvoice.PaidUp;
                decimal totalAmount = purchaseInvoice.TotalAmount; 
                decimal remainder = totalAmount - paidUp;

                // Check if PaidUp is less than or equal to TotalAmount
                if (paidUp > totalAmount)
                {
                    return BadRequest(new { ErrorMessage = "المدفوع لا يمكن ان يكون اكبر من اجمالي الفاتورة" });
                }

                // Check if Remainder is less than or equal to TotalAmount and PaidUp + Remainder equals TotalAmount
                if (remainder > totalAmount || paidUp + remainder != totalAmount || remainder != purchaseInvoice.Remainder)
                {
                    return BadRequest(new { ErrorMessage = "رجاء مراجعة الباقي من اجمالي الفاتورة" });
                }

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
