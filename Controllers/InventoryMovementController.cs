using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Reports;
using IOMSYS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        private readonly IBranchInventoryService _branchInventoryService;
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
            _branchInventoryService = branchInventoryService;
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

            var salesInvoiceModel = new SalesInvoicesModel
            {
                CustomerId = 1, 
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

                    if (salesInvoiceModel.BranchId == 0)
                    {
                        salesInvoiceModel.BranchId = model.FromBranchId;
                        var tobranchname = await _branchesService.SelectBranchByIdAsync(model.ToBranchId);
                        salesInvoiceModel.Notes = "منقوله الي فرع " + tobranchname.BranchName;
                    }

                    // Fetch sell price and create SalesItem
                    var buyPrice = await FetchBuyPriceForProduct(model.ProductId);
                    salesInvoiceModel.SalesItems.Add(new SalesItemsModel
                    {
                        ProductId = model.ProductId,
                        SizeId = model.SizeId,
                        ColorId = model.ColorId,
                        Quantity = model.Quantity,
                        SellPrice = buyPrice,
                        BranchId = model.FromBranchId,
                        IsReturn = false,
                    });
                    // Add to temporary store for updating later
                    pendingMovements.Add(model);
                }

                decimal itemsTotal = salesInvoiceModel.SalesItems.Sum(item => item.Quantity * item.SellPrice);
                salesInvoiceModel.TotalAmount = itemsTotal;
                salesInvoiceModel.Remainder = itemsTotal;

                var salesInvoiceResult = await AddNewSaleInvoice(salesInvoiceModel);
                if (salesInvoiceResult<=0)
                {
                    return Json(new { success = false, message = "حذث خطا اثناء عملية النقل" });
                }

                foreach (var model in pendingMovements)
                {
                    model.SalesInvoiceId = salesInvoiceResult;
                    model.MovementDate = DateTime.Now;
                    model.IsApproved = false;
                    await _inventoryMovementService.MoveInventoryAsync(model);
                }

                return Json(new { success = true, message = "تم نقل الكميات بنجاح وإنشاء قاتوره البيع." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveOrRejectMovement([FromForm] int movementId, [FromForm] bool isApproved)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "InventoryMovement", "ApproveOrRejectMovement");
            if (!hasPermission) { return BadRequest(new { message = "ليس لديك صلاحية" }); }
            int PurchaseInvoiceId = 0;
            try
            {
                var movement = await _inventoryMovementService.SelectInventoryMovementByIdAsync(movementId);
                var salesinovice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(movement.SalesInvoiceId);

                if (!isApproved)
                {
                    var deleteResult = await DeleteMoveSaleInvoice(movement.SalesInvoiceId);
                    if (deleteResult<=0)
                    {
                        return Json(new { success = false, message = "حدث خطا اثناء الحذف" });
                    }
                }
                else
                {
                    var frombranchname = await _branchesService.SelectBranchByIdAsync(salesinovice.BranchId);

                    var purchaseInvoicesModel = new PurchaseInvoicesModel
                    {
                        SupplierId = 1,
                        BranchId = movement.ToBranchId,
                        PaymentMethodId = 1,
                        UserId = userId,
                        PurchaseDate = DateTime.Now,
                        PurchaseItems = new List<PurchaseItemsModel>(),
                        Notes = "منقوله من " + frombranchname.BranchName,
                    };

                    var buyPrice = await FetchBuyPriceForProduct(movement.ProductId);
                    purchaseInvoicesModel.PurchaseItems.Add(new PurchaseItemsModel
                    {
                        ProductId = movement.ProductId,
                        SizeId = movement.SizeId,
                        ColorId = movement.ColorId,
                        Quantity = movement.Quantity,
                        BuyPrice = buyPrice,
                        BranchId = movement.ToBranchId,
                    });
                    PurchaseInvoiceId = await AddNewPurchaseInvoice(purchaseInvoicesModel);
                }

                var result = await _inventoryMovementService.ApproveOrRejectInventoryMovementAsync(movementId, isApproved, PurchaseInvoiceId);
                
                if (result)
                {
                    return Json(new { success = true, message = isApproved ? "Movement approved successfully." : "Movement rejected successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Error processing request." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred.", exceptionMessage = ex.Message });
            }
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

        public async Task<int> AddNewPurchaseInvoice(PurchaseInvoicesModel model)
        {
            try
            {
                model.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                decimal itemsTotal = model.PurchaseItems.Sum(item => item.Quantity * item.BuyPrice);

                model.TotalAmount = itemsTotal;
                model.PaidUp = 0;
                model.Remainder = itemsTotal;
                model.IsFullPaidUp = false;
                model.SupplierId = 1;

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
    }
}
