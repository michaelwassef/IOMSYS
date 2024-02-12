using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class SizesController : Controller
    {
        private readonly ISizesService _sizesService;

        public SizesController(ISizesService sizesService)
        {
            _sizesService = sizesService;
        }

        [Authorize(Roles = "GenralManager,BranchManager")]
        public IActionResult SizesPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadSizes()
        {
            var Sizes = await _sizesService.GetAllSizesAsync();
            return Json(Sizes);
        }

        [HttpPost]
        [Authorize(Roles = "GenralManager,BranchManager")]
        public async Task<IActionResult> AddNewSize([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var newSize = new SizesModel();
                JsonConvert.PopulateObject(values, newSize);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addSizeResult = await _sizesService.InsertSizeAsync(newSize);

                if (addSizeResult > 0)
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
        public async Task<IActionResult> UpdateSize([FromForm] IFormCollection formData)
        {
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
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeleteSize([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deleteSizeResult = await _sizesService.DeleteSizeAsync(key);
                if (deleteSizeResult > 0)
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
