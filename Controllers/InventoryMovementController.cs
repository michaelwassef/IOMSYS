using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class InventoryMovementController : Controller
    {
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly IBranchesService _branchesService;
        private readonly ISalesInvoicesService _salesInvoicesService;
        private readonly ISalesItemsService _salesItemsService;
        private readonly IPermissionsService _permissionsService;
        private readonly IProductsService _productsService;
        private readonly ISalesInvoiceItemsService _salesInvoiceItemsService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IPurchaseInvoicesService _purchaseInvoicesService;
        private readonly IPurchaseItemsService _purchaseItemsService;
        private readonly IPurchaseInvoiceItemsService _purchaseInvoiceItemsService;

        public InventoryMovementController(IInventoryMovementService inventoryMovementService, IBranchesService branchesService, ISalesInvoicesService salesInvoicesService, ISalesItemsService salesItemsService, IPermissionsService permissionsService, IProductsService productsService, ISalesInvoiceItemsService salesInvoiceItemsService, IPaymentTransactionService paymentTransactionService, IBranchInventoryService branchInventoryService, IPurchaseInvoicesService purchaseInvoicesService, IPurchaseItemsService purchaseItemsService, IPurchaseInvoiceItemsService purchaseInvoiceItemsService)
        {
            _inventoryMovementService = inventoryMovementService;
            _branchesService = branchesService;
            _salesInvoicesService = salesInvoicesService;
            _salesItemsService = salesItemsService;
            _permissionsService = permissionsService;
            _productsService = productsService;
            _salesInvoiceItemsService = salesInvoiceItemsService;
            _paymentTransactionService = paymentTransactionService;
            _purchaseInvoicesService = purchaseInvoicesService;
            _purchaseItemsService = purchaseItemsService;
            _purchaseInvoiceItemsService = purchaseInvoiceItemsService;
        }

        [HttpGet]
        public async Task<IActionResult> LoadinventoryMovementByToBranch([FromQuery] int BranchId)
        {
            var Items = await _inventoryMovementService.SelectInventoryMovementByToBranchIdAsync(BranchId);
            return Json(Items);
        }

        [HttpGet]
        public async Task<IActionResult> LoadHangingWarehouse()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            int? BranchId = await _branchesService.SelectBranchIdByManagerIdAsync(userId);
            var Items = await _inventoryMovementService.SelectHangingWarehouseByToBranchIdAsync(BranchId);
            return Json(Items);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewInventoryMovement([FromBody] InventoryMovementBatchModel batchModel)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "InventoryMovement", "AddNewinventoryMovement");
            if (!hasPermission) { return BadRequest(new { message = "ليس لديك صلاحية" }); }

            var fromBranchIds = batchModel.Items.Select(item => item.FromBranchId).Distinct().ToList();
            var toBranchIds = batchModel.Items.Select(item => item.ToBranchId).Distinct().ToList();
            int key = 0;
            if (fromBranchIds.Count > 1 || toBranchIds.Count > 1)
            {
                return BadRequest(new { message = "يجب النقل من فرع واحد والي فرع واحد" });
            }

            foreach (var item in batchModel.Items)
            {
                var unit = await _productsService.SelectProductByIdAsync(item.ProductId);
                if (unit.UnitId == 1)
                {
                    if (item.Quantity != Math.Floor(item.Quantity))
                    {
                        return Json(new { success = false, message = $"لا يمكن ادخال {unit.ProductName} بهذه الكمية : {item.Quantity}" });
                    }
                }
            }

            var processedItems = new HashSet<string>();
            foreach (var item in batchModel.Items)
            {
                string itemIdentifier = $"{item.ProductId}-{item.SizeId}-{item.ColorId}-{item.Quantity}-{item.FromBranchId}-{item.ToBranchId}";
                if (processedItems.Contains(itemIdentifier))
                {
                    return Json(new { success = false, message = $"تم الكشف عن عنصر مكرر: {item.ProductId}، ولا يمكن معالجة نفس العنصر مرتين في نفس الدفعة." });
                }
                else
                {
                    processedItems.Add(itemIdentifier);
                }
            }

            int tobranchx = 0;
            int frombranchx = 0;
            var salesInvoiceModel = new SalesInvoicesModel
            {
                BranchId = 0,
                PaymentMethodId = 5,
                UserId = userId,
                SaleDate = DateTime.Now,
                IsReturn = false,
                PaidUp = 0,
                IsFullPaidUp = false,
                SalesItems = new List<SalesItemsModel>(),
            };
            var pendingMovements = new List<InventoryMovementModel>();
            try
            {
                foreach (var model in batchModel.Items)
                {
                    int? BranchId = await _branchesService.SelectBranchIdByManagerIdAsync(userId);
                    if (model.FromBranchId != BranchId)
                    {
                        return Json(new { success = false, message = "ليس لديك صلاحية لنقل من هذا الفرع" });
                    }

                    decimal ava = await _productsService.GetAvailableQuantity(model.ProductId, model.ColorId, model.SizeId, model.FromBranchId);
                    decimal remainingQty = ava;
                    foreach (var existingItem in salesInvoiceModel.SalesItems)
                    {
                        if (existingItem.ProductId == model.ProductId &&
                            existingItem.SizeId == model.SizeId &&
                            existingItem.ColorId == model.ColorId)
                        {
                            remainingQty -= existingItem.Quantity;
                        }
                    }
                    if (model.Quantity > remainingQty)
                    {
                        continue;
                    }

                    frombranchx = model.FromBranchId;
                    tobranchx = model.ToBranchId;

                    if (salesInvoiceModel.BranchId == 0)
                    {
                        salesInvoiceModel.BranchId = model.FromBranchId;
                        var tobranch = await _branchesService.SelectBranchByIdAsync(model.ToBranchId);
                        salesInvoiceModel.Notes = "منقوله الي فرع " + tobranch.BranchName;
                        salesInvoiceModel.CustomerId = tobranch.CustomerId;
                    }

                    decimal price = await FetchBuyPriceForProduct(model.ProductId);

                    salesInvoiceModel.SalesItems.Add(new SalesItemsModel
                    {
                        ProductId = model.ProductId,
                        SizeId = model.SizeId,
                        ColorId = model.ColorId,
                        Quantity = model.Quantity,
                        SellPrice = price,
                        BranchId = model.FromBranchId,
                        IsReturn = false,
                        ModDate = DateTime.UtcNow,
                        ModUser = userId,
                    });
                    pendingMovements.Add(model);
                }

                var toBranchxx = await _branchesService.SelectBranchByIdAsync(tobranchx);
                var fromBranchxx = await _branchesService.SelectBranchByIdAsync(frombranchx);

                decimal itemsTotal = salesInvoiceModel.SalesItems.Sum(item => item.Quantity * item.SellPrice);
                salesInvoiceModel.TotalAmount = itemsTotal;
                salesInvoiceModel.PaidUp = 0;
                salesInvoiceModel.Remainder = itemsTotal;
                salesInvoiceModel.IsFullPaidUp = false;
                int salesInvoiceResult = 0;

                if (batchModel.makeInvoice)
                {
                    salesInvoiceResult = await AddNewSaleInvoice(salesInvoiceModel);
                    foreach (var model in pendingMovements)
                    {
                        var salesItem = salesInvoiceModel.SalesItems.FirstOrDefault(si => si.ProductId == model.ProductId && si.SizeId == model.SizeId && si.ColorId == model.ColorId && si.BranchId == model.FromBranchId && si.Quantity == model.Quantity);

                        if (salesItem != null)
                        {
                            model.SaleItemId = salesItem.SalesItemId;
                        }
                        model.SalesInvoiceId = salesInvoiceResult;
                        model.MovementDate = DateTime.Now;
                        model.IsApproved = false;
                        model.MakeInvoice = batchModel.makeInvoice;
                        model.ModUser = userId;
                        key = await _inventoryMovementService.MoveInventoryAsync(model);
                    }
                }
                else
                {
                    foreach (var model in pendingMovements)
                    {
                        model.MovementDate = DateTime.Now;
                        model.IsApproved = false;
                        model.MakeInvoice = batchModel.makeInvoice;
                        model.ModUser = userId;
                        key = await _inventoryMovementService.MoveInventoryAsync(model);
                    }
                }
                if (batchModel.makeInvoice)
                {
                    await _permissionsService.LogActionAsync(userId, "POST", "InventoryMovement", key, "Insert New InventoryMovement With Invoice From : " + fromBranchxx.BranchName, frombranchx);
                    return Json(new { success = true, message = "تم نقل الكميات بنجاح وإنشاء فاتوره المبيعات." });
                }
                else
                {
                    await _permissionsService.LogActionAsync(userId, "POST", "InventoryMovement", key, "Insert New InventoryMovement Without Invoice From : " + fromBranchxx.BranchName, frombranchx);
                    return Json(new { success = true, message = "تم نقل الكميات بنجاح ولم يتم إنشاء فاتوره المبيعات." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveOrRejectMovements([FromBody] ApproveRejectModel model)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            if (!await _permissionsService.HasPermissionAsync(userId, "InventoryMovement", "ApproveOrRejectMovement"))
            {
                return BadRequest(new { message = "ليس لديك صلاحية" });
            }
            List<int> successfulMovements = new List<int>();
            List<int> failedMovements = new List<int>();
            var tobranchnameid = 0;
            var tobranchname = "";
            if (!model.IsApproved)
            {
                foreach (var movementId in model.MovementIds)
                {
                    var movement = await _inventoryMovementService.SelectInventoryMovementByIdAsync(movementId);
                    tobranchname = movement.ToBranchName;
                    if (movement.SaleItemId.HasValue)
                    {
                        await DeleteMoveSaleItem(movement.SaleItemId.Value);
                        await UpdateSalesInvoiceTotals(movement.SalesInvoiceId);
                    }
                    failedMovements.Add(movementId);
                }
                foreach (var movementId in failedMovements)
                {
                    var result = await _inventoryMovementService.ApproveOrRejectInventoryMovementAsync(movementId, false, 0, false);
                }
                await _permissionsService.LogActionAsync(userId, "POST", "InventoryMovement", 0, "Reject Movements To Branch : " + tobranchname, tobranchnameid);
                return Ok(new { success = true, message = "تم رفض النقل بنجاح" });
            }

            List<PurchaseItemsModel> purchaseItems = new List<PurchaseItemsModel>();
            int? fromBranchId = null;
            int? toBranchId = null;
            int SalesInvoiceId = 0;

            foreach (var movementId in model.MovementIds)
            {
                var movement = await _inventoryMovementService.SelectInventoryMovementByIdAsync(movementId);
                fromBranchId = movement.FromBranchId;
                toBranchId = movement.ToBranchId;
                SalesInvoiceId = movement.SalesInvoiceId;

                decimal price = await FetchBuyPriceForProduct(movement.ProductId);

                var item = new PurchaseItemsModel
                {
                    ProductId = movement.ProductId,
                    Quantity = movement.Quantity,
                    BuyPrice = price,
                    BranchId = movement.ToBranchId,
                    ColorId = movement.ColorId,
                    SizeId = movement.SizeId,
                    ModDate = DateTime.Now,
                    ModUser = userId,
                };
                if (movement.MakeInvoice)
                {
                    item.Statues = 5;
                }
                else
                {
                    item.Statues = 4;
                }
                purchaseItems.Add(item);
                successfulMovements.Add(movementId);
            }

            var fromBranch = await _branchesService.SelectBranchByIdAsync(fromBranchId.Value);
            var toBranch = await _branchesService.SelectBranchByIdAsync(toBranchId.Value);

            decimal amountToSpend = purchaseItems.Sum(item => item.Quantity * item.BuyPrice);
            decimal amountUtilized = await _paymentTransactionService.ProcessInvoicesAndUpdateBalances(toBranch.SupplierId, fromBranchId.Value, amountToSpend);

            var purchaseInvoiceModel = new PurchaseInvoicesModel
            {
                SupplierId = fromBranch.SupplierId,
                BranchId = toBranchId.Value,
                PaymentMethodId = 5,
                UserId = userId,
                PurchaseDate = DateTime.Now,
                PurchaseItems = purchaseItems,
                Notes = "منقوله من " + fromBranch.BranchName,
                TotalAmount = amountToSpend,
                PaidUp = amountUtilized,
                Remainder = amountToSpend - amountUtilized,
                IsFullPaidUp = (amountToSpend - amountUtilized) == 0,
                SalesInvoiceId = SalesInvoiceId,
            };

            int PurchaseInvoiceId = 0;
            PurchaseInvoiceId = await AddNewPurchaseInvoice(purchaseInvoiceModel);

            decimal amountUtilized2 = await _paymentTransactionService.ProcessInvoicesAndUpdateBalancesS(toBranch.CustomerId, fromBranch.BranchId, amountUtilized, 5);
            decimal amountUtilized3 = await _paymentTransactionService.ProcessInvoicesAndUpdateBalancesS(fromBranch.CustomerId, toBranch.BranchId, amountUtilized, 5);

            foreach (var movementId in successfulMovements)
            {
                var movement = await _inventoryMovementService.SelectInventoryMovementByIdAsync(movementId);
                var result = await _inventoryMovementService.ApproveOrRejectInventoryMovementAsync(movementId, true, PurchaseInvoiceId, movement.MakeInvoice);
            }

            return Json(new { success = true, message = "تمت عملية النقل وعمل فاتورة المشتريات" });
        }

        public async Task<int> AddNewSaleInvoice(SalesInvoicesModel model)
        {
            try
            {
                int invoiceId = await _salesInvoicesService.InsertSalesInvoiceAsync(model);
                var invoiceadded = await _salesInvoicesService.GetSalesInvoiceByIdAsync(invoiceId);
                if (invoiceId <= 0)
                    return 0;

                foreach (var item in model.SalesItems)
                {
                    item.SalesItemId = await _salesItemsService.InsertSalesItemAsync(item);
                    await _salesInvoiceItemsService.AddSalesItemToInvoiceAsync(new SalesInvoiceItemsModel { SalesInvoiceId = invoiceId, SalesItemId = item.SalesItemId });
                }

                if (invoiceadded.PaidUp > 0)
                {
                    model.Notes = "دفعة من فاتورة المبيعات #" + invoiceId;
                    await RecordPaymentTransaction(invoiceadded, invoiceId);
                }
                return invoiceId;
            }
            catch
            {
                return 0;
            }
        }
        public async Task<int> AddNewPurchaseInvoice(PurchaseInvoicesModel model)
        {
            try
            {
                model.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                int flag = 0;
                int invoiceId = 0;
                var invoiceadded = await _purchaseInvoicesService.GetPurchaseInvoiceByIdAsync(invoiceId);
                foreach (var item in model.PurchaseItems)
                {
                    if (item.Statues != 4)
                    {
                        flag++;
                    }
                }
                if (flag > 0)
                {
                    invoiceId = await _purchaseInvoicesService.InsertPurchaseInvoiceAsync(model);
                    invoiceadded = await _purchaseInvoicesService.GetPurchaseInvoiceByIdAsync(invoiceId);
                }

                if (invoiceId <= 0)
                    return 0;

                foreach (var item in model.PurchaseItems)
                {
                    item.BranchId = model.BranchId;
                    item.ModDate = DateTime.Now;
                    if (item.Statues == 4)
                    {
                        item.PurchaseItemId = await _purchaseItemsService.InsertPurchaseItemAsync(item);
                    }
                    else
                    {
                        item.PurchaseItemId = await _purchaseItemsService.InsertPurchaseItemAsync(item);
                        await _purchaseInvoiceItemsService.AddItemToPurchaseInvoiceAsync(new PurchaseInvoiceItemsModel { PurchaseInvoiceId = invoiceId, PurchaseItemId = item.PurchaseItemId });
                    }
                }

                if (invoiceadded.PaidUp > 0)
                {
                    model.Notes = "دفعة من فاتورة المشتريات #" + invoiceId;
                    await RecordPaymentTransaction(invoiceadded, invoiceId);
                }
                return invoiceId;
            }
            catch
            {
                return 0;
            }
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

        public async Task<decimal> FetchSellPriceForProduct(int productId)
        {
            var product = await _productsService.SelectProductByIdAsync(productId);
            if (product != null)
            {
                return product.SellPrice;
            }
            return 0;
        }
        public async Task<decimal> FetchBuyPriceForProduct(int productId)
        {
            var product = await _productsService.SelectProductByIdAsync(productId);
            if (product != null)
            {
                return product.BuyPrice;
            }
            return 0;
        }
        public async Task<int> DeleteMoveSaleInvoice(int invoiceId)
        {
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
                    await _salesItemsService.DeleteSalesItemAsync(item.SalesItemId);
                }

                // Step 4: Delete the invoice
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
                            return 0;
                        }
                    }
                }
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public async Task<int> DeleteMoveSaleItem(int invoiceItemId)
        {
            try
            {
                var invoiceId = await _salesInvoiceItemsService.SelectInvoiceConnectToItemAsync(invoiceItemId);
                await _salesInvoiceItemsService.RemoveSalesItemFromInvoiceAsync(new SalesInvoiceItemsModel { SalesInvoiceId = invoiceId, SalesItemId = invoiceItemId });
                await _salesItemsService.DeleteSalesItemAsync(invoiceItemId);
                await DeleteInvoiceIfNoItems(invoiceId);
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public async Task UpdateSalesInvoiceTotals(int salesInvoiceId)
        {
            var remainingItems = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(salesInvoiceId);
            decimal newTotalAmount = remainingItems.Sum(item => item.Quantity * item.SellPrice);
            var invoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(salesInvoiceId);
            if (invoice != null)
            {
                invoice.TotalAmount = newTotalAmount;
                if (newTotalAmount > invoice.PaidUp)
                {
                    invoice.PaidUp = newTotalAmount - invoice.PaidUp;
                }
                else
                {
                    invoice.PaidUp = invoice.PaidUp - newTotalAmount;
                }
                invoice.Remainder = invoice.TotalAmount - newTotalAmount;
                await _salesInvoicesService.UpdateSalesInvoiceAsync(invoice);
                var purchase = await _purchaseInvoicesService.GetPurchaseInvoiceBySalesIdAsync(salesInvoiceId);
                if (purchase != null)
                {
                    purchase.PaidUp = invoice.PaidUp;
                    purchase.Remainder = invoice.Remainder;
                    await _purchaseInvoicesService.UpdatePurchaseInvoiceAsync(purchase);
                }
            }
        }
        public async Task<IActionResult> DeleteInvoiceIfNoItems(int invoiceId)
        {
            try
            {
                var items = await _salesItemsService.GetSaleItemsByInvoiceIdAsync(invoiceId);
                if (!items.Any())
                {
                    int deleteResult = await _salesInvoicesService.DeleteSalesInvoiceAsync(invoiceId);
                    var purchase = await _purchaseInvoicesService.GetPurchaseInvoiceBySalesIdAsync(invoiceId);
                    if (purchase != null)
                    {
                        var items2 = await _purchaseItemsService.GetPurchaseItemsByInvoiceIdAsync(invoiceId);

                        foreach (var item in items2)
                        {
                            await _purchaseInvoiceItemsService.RemoveItemFromPurchaseInvoiceAsync(new PurchaseInvoiceItemsModel { PurchaseInvoiceId = invoiceId, PurchaseItemId = item.PurchaseItemId });
                        }
                        foreach (var item in items2)
                        {
                            await _purchaseItemsService.DeletePurchaseItemAsync(item.PurchaseItemId);
                        }
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
                        }
                    }
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

        [HttpPost]
        public async Task<IActionResult> SupplierPaidUp(int SupplierId, int BranchId, int PaymentMethodId, decimal Amount)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            if (!await _permissionsService.HasPermissionAsync(userId, "InventoryMovement", "SupplierPaidUp"))
            {
                return BadRequest(new { message = "ليس لديك صلاحية" });
            }
            decimal accountBalance = await _paymentTransactionService.GetBranchAccountBalanceByPaymentAsync(BranchId, PaymentMethodId);
            if (Amount > accountBalance)
            {
                return BadRequest(new { message = "لا يوجد رصيد بالخزنة المختاره" });
            }
            decimal amountUtilized = await _paymentTransactionService.ProcessInvoicesAndUpdateBalances(SupplierId, BranchId, Amount);

            await _permissionsService.LogActionAsync(userId, "POST", "InventoryMovement", 0, "Supplier : " + SupplierId + " has been paid : " + amountUtilized + " FromBranch : " + BranchId, BranchId);
            return Ok(new { success = true, message = $" تم دفع للمورد {amountUtilized}" });
        }

        [HttpPost]
        public async Task<IActionResult> CustomerPaidUp(int CustomerId, int BranchId, int PaymentMethodId, decimal Amount)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            if (!await _permissionsService.HasPermissionAsync(userId, "InventoryMovement", "CustomerPaidUp"))
            {
                return BadRequest(new { message = "ليس لديك صلاحية" });
            }
            decimal amountUtilized = await _paymentTransactionService.ProcessInvoicesAndUpdateBalancesS(CustomerId, BranchId, Amount, PaymentMethodId);
            await _permissionsService.LogActionAsync(userId, "POST", "InventoryMovement", 0, "Customer : " + CustomerId + " has paid : " + amountUtilized + " ToBranch : " + BranchId, BranchId);
            return Ok(new { success = true, message = $" تم تحصيل من العميل {amountUtilized}" });
        }
    }
}
