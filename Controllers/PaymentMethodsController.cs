using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class PaymentMethodsController : Controller
    {
        private readonly IPaymentMethodsService _paymentMethodsService;
        private readonly IPermissionsService _permissionsService;

        public PaymentMethodsController(IPaymentMethodsService paymentMethodsService, IPermissionsService permissionsService)
        {
            _paymentMethodsService = paymentMethodsService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> PaymentMethodsPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PaymentMethods", "PaymentMethodsPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadPaymentMethods()
        {
            var PaymentMethods = await _paymentMethodsService.GetAllPaymentMethodsAsync();
            return Json(PaymentMethods);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewPaymentMethod([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PaymentMethods", "AddNewPaymentMethod");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var values = formData["values"];
                var newPaymentMethod = new PaymentMethodsModel();
                JsonConvert.PopulateObject(values, newPaymentMethod);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addPaymentMethodResult = await _paymentMethodsService.InsertPaymentMethodAsync(newPaymentMethod);

                if (addPaymentMethodResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "POST", "PaymentMethods", addPaymentMethodResult, "Insert New Payment Method : " + newPaymentMethod.PaymentMethodName, 0);
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
        public async Task<IActionResult> UpdatePaymentMethod([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PaymentMethods", "UpdatePaymentMethod");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
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
                    await _permissionsService.LogActionAsync(userId, "PUT", "PaymentMethods", key, "Update Payment Method : " + PaymentMethod.PaymentMethodName, 0);
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
        public async Task<IActionResult> DeletePaymentMethod([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PaymentMethods", "DeletePaymentMethod");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var PaymentMethod = await _paymentMethodsService.SelectPaymentMethodByIdAsync(key);
                int deletePaymentMethodResult = await _paymentMethodsService.DeletePaymentMethodAsync(key);
                if (deletePaymentMethodResult > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "DELETE", "PaymentMethods", key, "Delete Payment Method : " + PaymentMethod.PaymentMethodName, 0);
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
