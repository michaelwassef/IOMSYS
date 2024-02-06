using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager")]
    public class DiscountsController : Controller
    {
        private readonly IDiscountsService _discountsService;

        public DiscountsController(IDiscountsService discountsService)
        {
            _discountsService = discountsService;
        }

        public IActionResult DiscountsPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadDiscounts()
        {
            var Discounts = await _discountsService.GetAllDiscountsAsync();
            return Json(Discounts);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewDiscount([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var newDiscount = new DiscountsModel();
                JsonConvert.PopulateObject(values, newDiscount);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addDiscountResult = await _discountsService.InsertDiscountAsync(newDiscount);

                if (addDiscountResult > 0)
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
        public async Task<IActionResult> UpdateDiscount([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var Discount = await _discountsService.SelectDiscountByIdAsync(key);

                JsonConvert.PopulateObject(values, Discount);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updateDiscountResult = await _discountsService.UpdateDiscountAsync(Discount);

                if (updateDiscountResult > 0)
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
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Discount.", ExceptionMessage = ex.Message });
            }
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteDiscount([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deleteDiscountResult = await _discountsService.DeleteDiscountAsync(key);
                if (deleteDiscountResult > 0)
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
