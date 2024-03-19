using DevExpress.PivotGrid.PivotTable;
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
            if (fromBranchIds.Count > 1 || toBranchIds.Count > 1)
            {
                return BadRequest(new { message = "يجب النقل من فرع واحد والي فرع واحد" });
            }

            int tobranchx = 0;
            int frombranchx = 0;
            var salesInvoiceModel = new SalesInvoicesModel
            {
                BranchId = 0,
                PaymentMethodId = 1,
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

                    frombranchx = model.FromBranchId;
                    tobranchx = model.ToBranchId;

                    if (salesInvoiceModel.BranchId == 0)
                    {
                        salesInvoiceModel.BranchId = model.FromBranchId;
                        var tobranch = await _branchesService.SelectBranchByIdAsync(model.ToBranchId);
                        salesInvoiceModel.Notes = "منقوله الي فرع " + tobranch.BranchName;
                        salesInvoiceModel.CustomerId = tobranch.CustomerId;
                    }

                    decimal price = await (model.ToBranchId == 1 ? FetchBuyPriceForProduct(model.ProductId) : FetchSellPriceForProduct(model.ProductId));

                    salesInvoiceModel.SalesItems.Add(new SalesItemsModel
                    {
                        ProductId = model.ProductId,
                        SizeId = model.SizeId,
                        ColorId = model.ColorId,
                        Quantity = model.Quantity,
                        SellPrice = price,
                        BranchId = model.FromBranchId,
                        IsReturn = false,
                    });
                    pendingMovements.Add(model);
                }

                var toBranchxx = await _branchesService.SelectBranchByIdAsync(tobranchx);
                var fromBranchxx = await _branchesService.SelectBranchByIdAsync(frombranchx);

                decimal itemsTotal = salesInvoiceModel.SalesItems.Sum(item => item.Quantity * item.SellPrice);
                decimal amountUtilized = await _paymentTransactionService.ProcessInvoicesAndUpdateBalancesS(fromBranchxx.CustomerId, toBranchxx.BranchId, itemsTotal);
                salesInvoiceModel.TotalAmount = itemsTotal;
                salesInvoiceModel.PaidUp = amountUtilized;
                salesInvoiceModel.Remainder = itemsTotal - amountUtilized;
                if (salesInvoiceModel.Remainder == 0)
                {
                    salesInvoiceModel.IsFullPaidUp = true;
                }
                else
                {
                    salesInvoiceModel.IsFullPaidUp = false;
                }

                var salesInvoiceResult = await AddNewSaleInvoice(salesInvoiceModel);

                foreach (var model in pendingMovements)
                {
                    model.SalesInvoiceId = salesInvoiceResult;
                    model.MovementDate = DateTime.Now;
                    model.IsApproved = false;
                    await _inventoryMovementService.MoveInventoryAsync(model);
                }

                return Json(new { success = true, message = "تم نقل الكميات بنجاح وإنشاء فاتوره البيع." });
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

            if (!model.IsApproved)
            {
                foreach (var movementId in model.MovementIds)
                {
                    var movement = await _inventoryMovementService.SelectInventoryMovementByIdAsync(movementId);
                    await DeleteMoveSaleInvoice(movement.SalesInvoiceId);
                    failedMovements.Add(movementId);
                }
                foreach (var movementId in failedMovements)
                {
                    var result = await _inventoryMovementService.ApproveOrRejectInventoryMovementAsync(movementId, false, 0);
                }
                return Ok(new { success = true, message = "Movements rejected." });
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

                decimal price = await (movement.ToBranchId == 1 ? FetchBuyPriceForProduct(movement.ProductId) : FetchSellPriceForProduct(movement.ProductId));

                var item = new PurchaseItemsModel
                {
                    ProductId = movement.ProductId,
                    Quantity = movement.Quantity,
                    BuyPrice = price,
                    BranchId = movement.ToBranchId,
                    ColorId = movement.ColorId,
                    SizeId = movement.SizeId,
                    Statues = 1,
                    ModDate = DateTime.Now,
                };
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
                PaymentMethodId = 1,
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

            int PurchaseInvoiceId = await AddNewPurchaseInvoice(purchaseInvoiceModel);
            foreach (var movementId in successfulMovements)
            {
                var result = await _inventoryMovementService.ApproveOrRejectInventoryMovementAsync(movementId, true, PurchaseInvoiceId);
            }

            return Json(new { success = true, message = "تمت عملية النقل وعمل فاتورة المشتريات" });
        }



        public async Task<int> AddNewSaleInvoice(SalesInvoicesModel model)
        {
            try
            {
                // Insert the invoice
                int invoiceId = await _salesInvoicesService.InsertSalesInvoiceAsync(model);

                if (invoiceId <= 0)
                    return 0;

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
                    Amount = model.PaidUp,
                    TransactionType = "اضافة",
                    TransactionDate = model.SaleDate,
                    ModifiedUser = model.UserId,
                    ModifiedDate = DateTime.Now,
                    InvoiceId = invoiceId,
                    Details = "نقل منتجات بين فروع"
                };

                await _paymentTransactionService.InsertPaymentTransactionAsync(paymentTransaction);
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

                int invoiceId = await _purchaseInvoicesService.InsertPurchaseInvoiceAsync(model);
                if (invoiceId <= 0)
                    return 0;

                foreach (var item in model.PurchaseItems)
                {
                    item.BranchId = model.BranchId;
                    item.Statues = 1;
                    item.ModDate = DateTime.Now;
                    item.PurchaseItemId = await _purchaseItemsService.InsertPurchaseItemAsync(item);
                    await _purchaseInvoiceItemsService.AddItemToPurchaseInvoiceAsync(new PurchaseInvoiceItemsModel { PurchaseInvoiceId = invoiceId, PurchaseItemId = item.PurchaseItemId });
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
                    Details = model.Notes,
                };
                await _paymentTransactionService.InsertPaymentTransactionAsync(paymentTransaction);

                return invoiceId;
            }
            catch
            {
                return 0;
            }
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
                    var paymentTransaction = await _paymentTransactionService.GetPaymentTransactionByInvoiceIdAsync(invoiceId);
                    if (paymentTransaction != null)
                    {
                        // Delete the payment transaction
                        var deleteTransactionResult = await _paymentTransactionService.DeletePaymentTransactionAsync((int)paymentTransaction.TransactionId);
                        if (deleteTransactionResult <= 0)
                        {
                            return 0;
                        }
                    }
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

    }
}
