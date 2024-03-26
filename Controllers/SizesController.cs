using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class SizesController : Controller
    {
        private readonly ISizesService _sizesService;
        private readonly IPermissionsService _permissionsService;

        public SizesController(ISizesService sizesService, IPermissionsService permissionsService)
        {
            _sizesService = sizesService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> SizesPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Sizes", "SizesPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadSizes()
        {
            var Sizes = await _sizesService.GetAllSizesAsync();
            return Json(Sizes);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewSize([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Sizes", "AddNewSize");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var values = formData["values"];
                var newSize = new SizesModel();
                JsonConvert.PopulateObject(values, newSize);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addSizeResult = await _sizesService.InsertSizeAsync(newSize);

                if (addSizeResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "POST", "Sizes", addSizeResult, "Insert New Size : " + newSize.SizeName, 0);
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
        public async Task<IActionResult> UpdateSize([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Sizes", "UpdateSize");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var Size = await _sizesService.SelectSizeByIdAsync(key);

                JsonConvert.PopulateObject(values, Size);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updateSizeResult = await _sizesService.UpdateSizeAsync(Size);

                if (updateSizeResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "PUT", "Sizes", key, "Update Size : " + Size.SizeName, 0);
                    return Ok(new { SuccessMessage = "Updated Successfully" });
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Could Not Update" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Size.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSize([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Sizes", "DeleteSize");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var size = await _sizesService.SelectSizeByIdAsync(key);
                int deleteSizeResult = await _sizesService.DeleteSizeAsync(key);
                if (deleteSizeResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "DELETE", "Sizes", key, "Delete Size : " + size.SizeName, 0);
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
