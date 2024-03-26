using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    public class UnitsController : Controller
    {
        private readonly IUnitsService _unitsService;
        private readonly IPermissionsService _permissionsService;

        public UnitsController(IUnitsService unitsService, IPermissionsService permissionsService)
        {
            _unitsService = unitsService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> UnitsPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Units", "UnitsPage");
            if (!hasPermission)
            {
                return RedirectToAction("AccessDenied", "Access");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadUnits()
        {
            var Units = await _unitsService.GetAllUnitsAsync();
            return Json(Units);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewUnit([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Units", "AddNewUnit");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var values = formData["values"];
                var newUnit = new UnitsModel();
                JsonConvert.PopulateObject(values, newUnit);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addUnitResult = await _unitsService.InsertUnitAsync(newUnit);

                if (addUnitResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "POST", "Units", addUnitResult, "Insert New Unit : " + newUnit.UnitName, 0);
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
        public async Task<IActionResult> UpdateUnit([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Units", "UpdateUnit");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var Unit = await _unitsService.SelectUnitByIdAsync(key);

                JsonConvert.PopulateObject(values, Unit);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updateUnitResult = await _unitsService.UpdateUnitAsync(Unit);

                if (updateUnitResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "PUT", "Units", key, "Update Unit : " + Unit.UnitName, 0);
                    return Ok(new { SuccessMessage = "Updated Successfully" });
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Could Not Update" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Unit.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUnit([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Units", "DeleteUnit");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var unit = await _unitsService.SelectUnitByIdAsync(key);
                int deleteUnitResult = await _unitsService.DeleteUnitAsync(key);
                if (deleteUnitResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "DELETE", "Units", key, "Delete Unit : " + unit.UnitName, 0);
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
