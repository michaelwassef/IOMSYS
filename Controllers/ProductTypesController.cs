using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{

    [Authorize(Roles = "GenralManager,BranchManager")]
    public class ProductTypesController : Controller
    {
        private readonly IProductTypesService _productTypesService;

        public ProductTypesController(IProductTypesService productTypesService)
        {
            _productTypesService = productTypesService;
        }

        public IActionResult ProductTypesPage()
        {
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
            try
            {
                var values = formData["values"];
                var newProductType = new ProductTypesModel();
                JsonConvert.PopulateObject(values, newProductType);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addProductTypeResult = await _productTypesService.InsertProductTypeAsync(newProductType);

                if (addProductTypeResult > 0)
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
        public async Task<IActionResult> UpdateProductType([FromForm] IFormCollection formData)
        {
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
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deleteProductTypeResult = await _productTypesService.DeleteProductTypeAsync(key);
                if (deleteProductTypeResult > 0)
                    return Ok(new { SuccessMessage = "Deleted Successfully" });
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
