using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IProductsService _ProductsService;
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly IBranchInventoryService _branchInventoryService;
        private readonly IPurchaseItemsService _PurchaseItemsService;
        private readonly IPermissionsService _permissionsService;

        public ProductsController(IProductsService productsService, IInventoryMovementService inventoryMovementService, IBranchInventoryService branchInventoryService, IPurchaseItemsService purchaseItemsService, IPermissionsService permissionsService)
        {
            _ProductsService = productsService;
            _inventoryMovementService = inventoryMovementService;
            _branchInventoryService = branchInventoryService;
            _PurchaseItemsService = purchaseItemsService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> ProductsPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Products", "ProductsPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadProducts()
        {
            var Products = await _ProductsService.GetAllProductsAsync();
            return Json(Products);
        }

        [HttpGet]
        public async Task<IActionResult> LoadProductById(int ProductId)
        {
            var Products = await _ProductsService.SelectProductByIdAsync(ProductId);
            return Json(Products);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewProduct([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Products", "AddNewProduct");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var values = formData["values"];
                var newProduct = new ProductsModel();
                JsonConvert.PopulateObject(values, newProduct);

                if (newProduct.BuyPrice > newProduct.SellPrice)
                {
                    return BadRequest(new { ErrorMessage = "لا يمكن ان يكون سعر البيع اصغر من الشراء" });
                }
                if ((newProduct.SellPrice - newProduct.MaxDiscount) < newProduct.BuyPrice)
                {
                    return BadRequest(new { ErrorMessage = "لا يمكن ان يكون الخصم اكبر من سعر الشراء" });
                }

                int addProductResult = await _ProductsService.InsertProductAsync(newProduct);

                if (addProductResult > 0)
                    return Ok(new { SuccessMessage = "Successfully Added" });
                else
                    return BadRequest(new { ErrorMessage = "Could Not Add" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "Could not add", ExceptionMessage = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Products", "UpdateProduct");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var Product = await _ProductsService.SelectProductByIdAsync(key);

                JsonConvert.PopulateObject(values, Product);

                if (Product.BuyPrice > Product.SellPrice)
                {
                    return BadRequest(new { ErrorMessage = "لا يمكن ان يكون سعر البيع اصغر من الشراء" });
                }
                if (Product.MaxDiscount > Product.SellPrice)
                {
                    return BadRequest(new { ErrorMessage = "لا يمكن ان يكون الخصم اكبر من سعر الشراء" });
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updateProductResult = await _ProductsService.UpdateProductAsync(Product);

                if (updateProductResult > 0)
                {
                    return Ok(new { SuccessMessage = "Updated Successfully" });
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Could Not Update" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Product.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProduct([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Products", "DeleteProduct");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deleteProductResult = await _ProductsService.DeleteProductAsync(key);
                if (deleteProductResult > 0)
                    return Ok(new { SuccessMessage = "Deleted Successfully" });
                else
                    return BadRequest(new { ErrorMessage = "Could Not Delete" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

        public async Task<IActionResult> WarehousePage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Products", "WarehousePage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductsInWarehouse()
        {
            var Products = await _ProductsService.GetAllProductsInWarehouseAsync();
            return Json(Products);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductsInWarehouseByBranch(int branchId)
        {
            var Products = await _ProductsService.GetAllProductsInWarehouseAsync(branchId);
            return Json(Products);
        }

        public async Task<IActionResult> WarehouseMovementsPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Products", "WarehouseMovementsPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWarehouseMovementsByBranch(int branchId, DateTime fromdate, DateTime todate)
        {
            var Products = await _ProductsService.WarehouseMovementsAsync(branchId, fromdate, todate);
            return Json(Products);
        }

        public async Task<IActionResult> WarehouseDeficienciesPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Products", "WarehouseDeficienciesPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMinQuantityProductsInWarehouseByBranch(int branchId)
        {
            var Products = await _ProductsService.GetMinQuantityProductsInWarehouseAsync(branchId);
            return Json(Products);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSizes([FromQuery] int ProductId)
        {
            var Products = await _ProductsService.GetAvailableSizesForProduct(ProductId);
            return Json(Products);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableColors([FromQuery] int ProductId)
        {
            var Products = await _ProductsService.GetAvailableColorsForProduct(ProductId);
            return Json(Products);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailable(int productId, int colorId, int sizeId, int BranchId)
        {
            var sizesAndColors = await _ProductsService.GetAvailableQuantity(productId, colorId, sizeId, BranchId);
            return Ok(sizesAndColors);
        }

        [HttpGet]
        public async Task<IActionResult> GetWhereAvailableQuantity(int productId, int colorId, int sizeId, int branchId)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Products", "GetWhereAvailableQuantity");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }

            var sizesAndColors = await _ProductsService.GetAvailableQuantity(productId, colorId, sizeId, branchId);

            var availabilityText = "يوجد بالفرع لديك كميات من المنتج المختار";

            if (sizesAndColors == 0)
            {
                availabilityText = await _ProductsService.GetWhereAvailableQuantityAsync(productId, colorId, sizeId);
            }

            return Ok(availabilityText);
        }

        [HttpGet]
        public async Task<IActionResult> WholesalePrice()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Products", "WholesalePrice");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> TotalBranchInventoryReport(int BranchId)
        {
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                var report = new TotalBranchInventoryReport();
                report.Parameters["BranchId"].Value = BranchId;
                report.CreateDocument();
                report.ExportToPdf(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return File(memoryStream, "application/pdf", "TotalBranchReport.pdf");
            }
            catch (Exception ex)
            {
                memoryStream.Dispose();
                return BadRequest(new { ErrorMessage = "An error occurred while generating the report.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteInventoryItem(int id)
        {
            // First, check if ID exists in PurchaseItems
            var purchaseItem = await _PurchaseItemsService.GetPurchaseItemsByIDitemAsync(id);
            if (purchaseItem != null)
            {
                // It's a purchase item
                var result = await _PurchaseItemsService.DeletePurchaseItemAsync(id);
                if (result > 0)
                {
                    await _branchInventoryService.AdjustInventoryQuantityAsync(
                        purchaseItem.ProductId, purchaseItem.SizeId, purchaseItem.ColorId, purchaseItem.BranchId, -purchaseItem.Quantity);
                    return Ok(new { SuccessMessage = "Deleted Successfully" });
                }
                return result == 0 ? NotFound() : BadRequest();
            }
            else
            {
                var movement = await _inventoryMovementService.SelectInventoryMovementByIdAsync(id);
                if (movement != null)
                {
                    // It's an inventory movement
                    var result = await _inventoryMovementService.DeleteMovementAsync(id);
                    if (result > 0)
                    {
                        await _branchInventoryService.AdjustInventoryQuantityAsync(
                            movement.ProductId, movement.SizeId, movement.ColorId, movement.ToBranchId, -movement.Quantity);
                        return Ok(new { SuccessMessage = "Deleted Successfully" });
                    }
                    return result == 0 ? NotFound() : BadRequest();
                }
            }

            // If we reach here, the ID was not found in either table
            return NotFound();
        }

    }
}
