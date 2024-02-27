using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly ISuppliersService _SuppliersService;
        private readonly IPermissionsService _permissionsService;

        public SuppliersController(ISuppliersService suppliersService, IPermissionsService permissionsService)
        {
            _SuppliersService = suppliersService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> SuppliersPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Suppliers", "SuppliersPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadSuppliers()
        {
            var Suppliers = await _SuppliersService.GetAllSuppliersAsync();
            return Json(Suppliers);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewSupplier([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Suppliers", "AddNewSupplier");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var values = formData["values"];
                var newSupplier = new SuppliersModel();
                JsonConvert.PopulateObject(values, newSupplier);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addSupplierResult = await _SuppliersService.InsertSupplierAsync(newSupplier);

                if (addSupplierResult > 0)
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
        public async Task<IActionResult> UpdateSupplier([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Suppliers", "UpdateSupplier");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var Supplier = await _SuppliersService.SelectSupplierByIdAsync(key);

                JsonConvert.PopulateObject(values, Supplier);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updateSupplierResult = await _SuppliersService.UpdateSupplierAsync(Supplier);

                if (updateSupplierResult > 0)
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
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Supplier.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSupplier([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Suppliers", "DeleteSupplier");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deleteSupplierResult = await _SuppliersService.DeleteSupplierAsync(key);
                if (deleteSupplierResult > 0)
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
