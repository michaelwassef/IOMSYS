using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class ProductTypesController : Controller
    {
        private readonly IProductTypesService _productTypesService;
        private readonly IPermissionsService _permissionsService;

        public ProductTypesController(IProductTypesService productTypesService, IPermissionsService permissionsService)
        {
            _productTypesService = productTypesService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> ProductTypesPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "ProductTypes", "ProductTypesPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadProductTypes()
        {
            var ProductTypes = await _productTypesService.GetAllProductTypesAsync();
            return Json(ProductTypes);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewProductType([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "ProductTypes", "AddNewProductType");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var values = formData["values"];
                var newProductType = new ProductTypesModel();
                JsonConvert.PopulateObject(values, newProductType);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addProductTypeResult = await _productTypesService.InsertProductTypeAsync(newProductType);

                if (addProductTypeResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "POST", "ProductTypes", addProductTypeResult, "Insert New ProductType : " + newProductType.ProductTypeName, 0);
                    return Ok(new { SuccessMessage = "Successfully Added" });
                }
                else
                    return BadRequest(new { ErrorMessage = "Could Not Add" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "Could not add", ExceptionMessage = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProductType([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "ProductTypes", "UpdateProductType");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var ProductType = await _productTypesService.SelectProductTypeByIdAsync(key);

                JsonConvert.PopulateObject(values, ProductType);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updateProductTypeResult = await _productTypesService.UpdateProductTypeAsync(ProductType);

                if (updateProductTypeResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "PUT", "ProductTypes", key, "Update ProductType : " + ProductType.ProductTypeName, 0);
                    return Ok(new { SuccessMessage = "Updated Successfully" });
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Could Not Update" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the ProductType.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProductType([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "ProductTypes", "DeleteProductType");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var producttype = await _productTypesService.SelectProductTypeByIdAsync(key);
                int deleteProductTypeResult = await _productTypesService.DeleteProductTypeAsync(key);
                if (deleteProductTypeResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "DELETE", "ProductTypes", key, "Delete ProductType : " + producttype.ProductTypeName, 0);
                    return Ok(new { SuccessMessage = "Deleted Successfully" });
                }
                else
                    return BadRequest(new { ErrorMessage = "Could Not Delete" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

    }
}
