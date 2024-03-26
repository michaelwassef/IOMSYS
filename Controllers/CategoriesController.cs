using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ICategoriesService _categoriesService;
        private readonly IPermissionsService _permissionsService;

        public CategoriesController(ICategoriesService categoriesService, IPermissionsService permissionsService)
        {
            _categoriesService = categoriesService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> CategoriesPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Categories", "CategoriesPage");
            if (!hasPermission)
            {
                return RedirectToAction("AccessDenied", "Access");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadCategories()
        {
            var Categories = await _categoriesService.GetAllCategoriesAsync();
            return Json(Categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewCategory([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Categories", "AddNewCategory");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var values = formData["values"];
                var newCategory = new CategoriesModel();
                JsonConvert.PopulateObject(values, newCategory);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addCategoryResult = await _categoriesService.InsertCategoryAsync(newCategory);
                if (addCategoryResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "POST", "Categories", addCategoryResult, "Insert New Category : " + newCategory.CategoryName, 0);
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
        public async Task<IActionResult> UpdateCategory([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Categories", "UpdateCategory");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
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
                    await _permissionsService.LogActionAsync(userId, "PUT", "Categories", key, "Update Category : " + Category.CategoryName, 0);
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
        public async Task<IActionResult> DeleteCategory([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Categories", "DeleteCategory");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var category = await _categoriesService.SelectCategoryByIdAsync(key);
                int deleteCategoryResult = await _categoriesService.DeleteCategoryAsync(key);
                if (deleteCategoryResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "DELETE", "Categories", key, "Delete Category :" + category.CategoryName, 0);
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

