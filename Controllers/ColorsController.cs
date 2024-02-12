using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class ColorsController : Controller
    {
        private readonly IColorsService _colorsService;

        public ColorsController(IColorsService colorsService)
        {
            _colorsService = colorsService;
        }

        [Authorize(Roles = "GenralManager,BranchManager")]
        public IActionResult ColorsPage()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "GenralManager,BranchManager,Employee")]
        public async Task<IActionResult> LoadColors()
        {
            var Colors = await _colorsService.GetAllColorsAsync();
            return Json(Colors);
        }

        [HttpPost]
        [Authorize(Roles = "GenralManager,BranchManager")]
        public async Task<IActionResult> AddNewColor([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var newColor = new ColorsModel();
                JsonConvert.PopulateObject(values, newColor);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addColorResult = await _colorsService.InsertColorAsync(newColor);

                if (addColorResult > 0)
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
        public async Task<IActionResult> UpdateColor([FromForm] IFormCollection formData)
        {
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
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeleteColor([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deleteColorResult = await _colorsService.DeleteColorAsync(key);
                if (deleteColorResult > 0)
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
