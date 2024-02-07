using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class CategoriesController : Controller
    {
        private readonly ICategoriesService _categoriesService;

        public CategoriesController(ICategoriesService categoriesService)
        {
            _categoriesService = categoriesService;
        }

        [Authorize(Roles = "GenralManager,BranchManager")]
        public IActionResult CategoriesPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadCategories()
        {
            var Categories = await _categoriesService.GetAllCategoriesAsync();
            return Json(Categories);
        }

        [HttpPost]
        [Authorize(Roles = "GenralManager,BranchManager")]
        public async Task<IActionResult> AddNewCategory([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var newCategory = new CategoriesModel();
                JsonConvert.PopulateObject(values, newCategory);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addCategoryResult = await _categoriesService.InsertCategoryAsync(newCategory);

                if (addCategoryResult > 0)
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
        public async Task<IActionResult> UpdateCategory([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var Category = await _categoriesService.SelectCategoryByIdAsync(key);

                JsonConvert.PopulateObject(values, Category);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updateCategoryResult = await _categoriesService.UpdateCategoryAsync(Category);

                if (updateCategoryResult > 0)
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
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Category.", ExceptionMessage = ex.Message });
            }
        }


        [HttpDelete]
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeleteCategory([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deleteCategoryResult = await _categoriesService.DeleteCategoryAsync(key);
                if (deleteCategoryResult > 0)
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

