using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class PurchaseItemsController : Controller
    {
        private readonly IPurchaseItemsService _PurchaseItemsService;
        private readonly IPurchaseInvoiceItemsService _purchaseInvoiceItemsService;
        private readonly IPurchaseInvoicesService _purchaseInvoicesService;
        private readonly IProductsService _ProductsService;
        private readonly IBranchInventoryService _branchInventoryService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IBranchesService _branchesService;
        private readonly IPermissionsService _permissionsService;

        public PurchaseItemsController(IPurchaseItemsService PurchaseItemsService, IPurchaseInvoiceItemsService purchaseInvoiceItemsService, IPurchaseInvoicesService purchaseInvoicesService, IProductsService productsService, IBranchInventoryService branchInventoryService, IPaymentTransactionService paymentTransactionService, IBranchesService branchesService, IPermissionsService permissionsService)
        {
            _PurchaseItemsService = PurchaseItemsService;
            _purchaseInvoiceItemsService = purchaseInvoiceItemsService;
            _purchaseInvoicesService = purchaseInvoicesService;
            _ProductsService = productsService;
            _branchInventoryService = branchInventoryService;
            _paymentTransactionService = paymentTransactionService;
            _branchesService = branchesService;
            _permissionsService = permissionsService;
        }

        [HttpGet]
        public async Task<IActionResult> LoadPurchaseItems()
        {
            var PurchaseItems = await _PurchaseItemsService.GetAllPurchaseItemsAsync();
            return Json(PurchaseItems);
        }

        [HttpGet]
        public async Task<IActionResult> LoadFactoryItem(int branchId)
        {
            var PurchaseItems = await _PurchaseItemsService.GetAllFactoryItemsByBranchAsync(branchId);
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
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission2 = await _permissionsService.HasPermissionAsync(userId, "PurchaseItems", "AddNewPurchaseItem");
            if (!hasPermission2)
            { return Json(new { success = false, message = "ليس لديك صلاحية للاضافة" }); }
            try
            {
                var values = formData["values"];
                var newPurchaseItems = new PurchaseItemsModel();
                JsonConvert.PopulateObject(values, newPurchaseItems);

                newPurchaseItems.ModDate = DateTime.Now;

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int? BranchId = await _branchesService.SelectBranchIdByManagerIdAsync(userId);

                if (newPurchaseItems.BranchId != BranchId)
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية للاضافة لهذا الفرع" });
                }

                var unit = await _ProductsService.SelectProductByIdAsync(newPurchaseItems.ProductId);
                if (unit.UnitId == 1)
                {
                    if (newPurchaseItems.Quantity != Math.Floor(newPurchaseItems.Quantity))
                    {
                        return Json(new { success = false, message = $"لا يمكن ادخال {unit.ProductName} بهذه الكمية : {newPurchaseItems.Quantity}" });
                    }
                }

                if (newPurchaseItems.Quantity < 0)
                {
                    var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PurchaseItems", "AddDamagedProducts");
                    if (!hasPermission)
                    {
                        return Json(new { success = false, message = "ليس لديك صلاحية لاضافة التوالف" });
                    }
                    newPurchaseItems.Statues = 2;
                    var checkava = await _ProductsService.GetAvailableQuantity(newPurchaseItems.ProductId, newPurchaseItems.ColorId, newPurchaseItems.SizeId, newPurchaseItems.BranchId);
                    if (checkava < -newPurchaseItems.Quantity)
                    {
                        return Json(new { success = false, message = "لا توجد الكمية المطلوبة لعمل تالف" });
                    }
                }
                else
                {
                    newPurchaseItems.Statues = 3;
                }
                newPurchaseItems.ModUser = userId;
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

        [HttpPost]
        public async Task<IActionResult> AddNewFactoryItem([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission2 = await _permissionsService.HasPermissionAsync(userId, "PurchaseItems", "AddNewFactoryItem");
            if (!hasPermission2)
            { return Json(new { success = false, message = "ليس لديك صلاحية للتصنيع" }); }
            try
            {
                var values = formData["values"];
                var newPurchaseItems = new PurchaseItemsModel();
                JsonConvert.PopulateObject(values, newPurchaseItems);

                newPurchaseItems.ModDate = DateTime.Now;

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int? BranchId = await _branchesService.SelectBranchIdByManagerIdAsync(userId);

                if (newPurchaseItems.BranchId != BranchId)
                {
                    return Json(new { success = false, message = "ليس لديك صلاحية للاضافة لهذا الفرع" });
                }

                if (newPurchaseItems.Quantity < 0)
                {
                    return Json(new { success = false, message = "لا يمكن ادخال ارقام سالبة" });
                }


                var unit = await _ProductsService.SelectProductByIdAsync(newPurchaseItems.ProductId);
                if (unit.UnitId == 1)
                {
                    if (newPurchaseItems.Quantity != Math.Floor(newPurchaseItems.Quantity))
                    {
                        return Json(new { success = false, message = $"لا يمكن ادخال {unit.ProductName} بهذه الكمية : {newPurchaseItems.Quantity}" });
                    }
                }

                decimal ava = await _ProductsService.GetAvailableQuantity(newPurchaseItems.ProductId, newPurchaseItems.ColorId, newPurchaseItems.SizeId, newPurchaseItems.BranchId);
                if (newPurchaseItems.Quantity > ava)
                {
                    return Json(new { success = false, message = "لا توجد كمية كافية" });
                }
                newPurchaseItems.Quantity = -newPurchaseItems.Quantity;
                newPurchaseItems.Statues = 1;
                newPurchaseItems.ModUser = userId;
                int addPurchaseItemsResult = await _PurchaseItemsService.InsertPurchaseItemAsync(newPurchaseItems);

                if (addPurchaseItemsResult > 0)
                {
                    await _branchInventoryService.AdjustInventoryQuantityAsync(newPurchaseItems.ProductId, newPurchaseItems.SizeId, newPurchaseItems.ColorId, newPurchaseItems.BranchId, newPurchaseItems.Quantity);
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

        //not finish
        [HttpPut]
        public async Task<IActionResult> UpdatePurchaseItem([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
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

                decimal quantityDifference = newPurchaseItem.Quantity - existingPurchaseItem.Quantity;
                newPurchaseItem.ModUser = userId;
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
        public async Task<IActionResult> DeletePurchaseItem([FromForm] IFormCollection formData)
        {
            try
            {
                var purchaseItemId = Convert.ToInt32(formData["key"]);
                var purchaseItem = await _PurchaseItemsService.GetPurchaseItemByIdAsync(purchaseItemId);
                var purchaseInvoiceId = purchaseItem.PurchaseInvoiceId;

                var availableQty = await _branchInventoryService.GetInventoryByProductAndBranchAsync(purchaseItem.ProductId, purchaseItem.SizeId, purchaseItem.ColorId, purchaseItem.BranchId);
                if (availableQty.AvailableQty - purchaseItem.Quantity < 0) { return BadRequest(new { ErrorMessage = "لا يمكنك الحذف لقد بيع من الكمية المراد حذفها" }); }

                var removeConnectionResult = await _purchaseInvoiceItemsService.RemoveItemFromPurchaseInvoiceAsync(new PurchaseInvoiceItemsModel
                {
                    PurchaseInvoiceId = purchaseInvoiceId,
                    PurchaseItemId = purchaseItemId
                });

                int deletePurchaseItemsResult = await _PurchaseItemsService.DeletePurchaseItemAsync(purchaseItemId);
                if (deletePurchaseItemsResult > 0)
                {
                    await _branchInventoryService.AdjustInventoryQuantityAsync(purchaseItem.ProductId, purchaseItem.SizeId, purchaseItem.ColorId, purchaseItem.BranchId, -purchaseItem.Quantity);

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

        [HttpDelete]
        public async Task<IActionResult> DeletePurchaseItemWithoutInvoices(int purchaseItemId)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PurchaseItems", "DeletePurchaseItemWithoutInvoices");
            if (!hasPermission) { return BadRequest(new { message = "ليس لديك صلاحية" }); }
            try
            {
                var purchaseItem = await _PurchaseItemsService.GetPurchaseItemWithoutInvoiceByIdAsync(purchaseItemId);
                var availableQty = await _branchInventoryService.GetInventoryByProductAndBranchAsync(purchaseItem.ProductId, purchaseItem.SizeId, purchaseItem.ColorId, purchaseItem.BranchId);
                if (availableQty.AvailableQty - purchaseItem.Quantity < 0){ return BadRequest(new { ErrorMessage = "لا يمكنك الحذف لقد بيع من الكمية المراد حذفها" }); }

                int deletePurchaseItemsResult = await _PurchaseItemsService.DeletePurchaseItemAsync(purchaseItemId);
                if (deletePurchaseItemsResult > 0)
                {
                    var adjustInventoryquantity = await _branchInventoryService.AdjustInventoryQuantityAsync(purchaseItem.ProductId, purchaseItem.SizeId, purchaseItem.ColorId, purchaseItem.BranchId, -purchaseItem.Quantity);
                    await _permissionsService.LogActionAsync(userId, "DELETE", "PurchaseItems", purchaseItemId, "Delete purchaseItemId : " + purchaseItemId+" ProductId : "+ purchaseItem.ProductId+" SizeId : "+ purchaseItem.SizeId+" ColorId : "+ purchaseItem.ColorId+" Qty : "+ purchaseItem.Quantity, purchaseItem.BranchId);
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
                var items = await _PurchaseItemsService.GetPurchaseItemsByInvoiceIdAsync(invoiceId);

                int deleteResult = await _purchaseInvoicesService.DeletePurchaseInvoiceAsync(invoiceId);
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
                return BadRequest(new { ErrorMessage = "حدث خطأ اثناء الحذف." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

    }
}
