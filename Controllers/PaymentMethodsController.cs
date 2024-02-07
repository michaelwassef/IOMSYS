using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class PaymentMethodsController : Controller
    {
        private readonly IPaymentMethodsService _paymentMethodsService;

        public PaymentMethodsController(IPaymentMethodsService paymentMethodsService)
        {
            _paymentMethodsService = paymentMethodsService;
        }

        [Authorize(Roles = "GenralManager,BranchManager")]
        public IActionResult PaymentMethodsPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadPaymentMethods()
        {
            var PaymentMethods = await _paymentMethodsService.GetAllPaymentMethodsAsync();
            return Json(PaymentMethods);
        }

        [HttpPost]
        [Authorize(Roles = "GenralManager,BranchManager")]
        public async Task<IActionResult> AddNewPaymentMethod([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var newPaymentMethod = new PaymentMethodsModel();
                JsonConvert.PopulateObject(values, newPaymentMethod);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addPaymentMethodResult = await _paymentMethodsService.InsertPaymentMethodAsync(newPaymentMethod);

                if (addPaymentMethodResult > 0)
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
        public async Task<IActionResult> UpdatePaymentMethod([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var PaymentMethod = await _paymentMethodsService.SelectPaymentMethodByIdAsync(key);

                JsonConvert.PopulateObject(values, PaymentMethod);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updatePaymentMethodResult = await _paymentMethodsService.UpdatePaymentMethodAsync(PaymentMethod);

                if (updatePaymentMethodResult > 0)
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
                return BadRequest(new { ErrorMessage = "An error occurred while updating the PaymentMethod.", ExceptionMessage = ex.Message });
            }
        }


        [HttpDelete]
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeletePaymentMethod([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deletePaymentMethodResult = await _paymentMethodsService.DeletePaymentMethodAsync(key);
                if (deletePaymentMethodResult > 0)
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
