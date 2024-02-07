using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class SuppliersController : Controller
    {
        private readonly ISuppliersService _SuppliersService;

        public SuppliersController(ISuppliersService suppliersService)
        {
            _SuppliersService = suppliersService;
        }

        [Authorize(Roles = "GenralManager,BranchManager")]
        public IActionResult SuppliersPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadSuppliers()
        {
            var Suppliers = await _SuppliersService.GetAllSuppliersAsync();
            return Json(Suppliers);
        }

        [HttpPost]
        [Authorize(Roles = "GenralManager,BranchManager")]
        public async Task<IActionResult> AddNewSupplier([FromForm] IFormCollection formData)
        {
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
        [Authorize(Roles = "GenralManager,BranchManager")]
        public async Task<IActionResult> UpdateSupplier([FromForm] IFormCollection formData)
        {
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
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeleteSupplier([FromForm] IFormCollection formData)
        {
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
