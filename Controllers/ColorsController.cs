using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class ColorsController : Controller
    {
        private readonly IColorsService _colorsService;
        private readonly IPermissionsService _permissionsService;

        public ColorsController(IColorsService colorsService, IPermissionsService permissionsService)
        {
            _colorsService = colorsService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> ColorsPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Colors", "ColorsPage");
            if (!hasPermission)
            {
                return RedirectToAction("AccessDenied", "Access");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadColors()
        {
            var Colors = await _colorsService.GetAllColorsAsync();
            return Json(Colors);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewColor([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Colors", "AddNewColor");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var values = formData["values"];
                var newColor = new ColorsModel();
                JsonConvert.PopulateObject(values, newColor);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addColorResult = await _colorsService.InsertColorAsync(newColor);

                if (addColorResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "POST", "Colors", addColorResult, "Insert New Color : " + newColor.ColorName, 0);
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
        public async Task<IActionResult> UpdateColor([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Colors", "UpdateColor");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var Color = await _colorsService.SelectColorByIdAsync(key);

                JsonConvert.PopulateObject(values, Color);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updateColorResult = await _colorsService.UpdateColorAsync(Color);

                if (updateColorResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "PUT", "Colors", key, "Update Color : " + Color.ColorName, 0);
                    return Ok(new { SuccessMessage = "Updated Successfully" });
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Could Not Update" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Color.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteColor([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Colors", "DeleteColor");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var color = await _colorsService.SelectColorByIdAsync(key);
                int deleteColorResult = await _colorsService.DeleteColorAsync(key);
                if (deleteColorResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "DELETE", "Colors", key, "DELETE Color : " + color.ColorName, 0);
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
