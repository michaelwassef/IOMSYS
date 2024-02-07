using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class ProductsController : Controller
    {
        private readonly IProductsService _ProductsService;
        private readonly ICategoriesService _categoriesService;
        private readonly IProductTypesService _producttypesService;

        public ProductsController(IProductsService productsService, ICategoriesService categoriesService, IProductTypesService producttypesService)
        {
            _ProductsService = productsService;
            _categoriesService = categoriesService;
            _producttypesService = producttypesService;
        }

        public IActionResult ProductsPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadProducts()
        {
            var Products = await _ProductsService.GetAllProductsAsync();
            return Json(Products);
        }

        [HttpPost]
        [Authorize(Roles = "GenralManager,BranchManager")]
        public async Task<IActionResult> AddNewProduct([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var newProduct = new ProductsModel();
                JsonConvert.PopulateObject(values, newProduct);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

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
        [Authorize(Roles = "GenralManager,BranchManager")]
        public async Task<IActionResult> UpdateProduct([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var Product = await _ProductsService.SelectProductByIdAsync(key);

                JsonConvert.PopulateObject(values, Product);

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
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeleteProduct([FromForm] IFormCollection formData)
        {
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
    }
}
