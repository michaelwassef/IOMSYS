using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomersService _customersService;
        private readonly IPermissionsService _permissionsService;


        public CustomersController(ICustomersService customersService, IPermissionsService permissionsService)
        {
            _customersService = customersService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> CustomersPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Customers", "CustomersPage");
            if (!hasPermission)
            {
                return RedirectToAction("AccessDenied", "Access");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadCustomers()
        {
            var Customers = await _customersService.GetAllCustomersAsync();
            return Json(Customers);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewCustomer([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Customers", "AddNewCustomer");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var values = formData["values"];
                var newCustomer = new CustomersModel();
                JsonConvert.PopulateObject(values, newCustomer);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addCustomerResult = await _customersService.InsertCustomerAsync(newCustomer);

                if (addCustomerResult > 0)
                    return Ok(new { SuccessMessage = "Successfully Added" });
                else
                    return BadRequest(new { ErrorMessage = "Could Not Add" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "Could not add", ExceptionMessage = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddFastNewCustomer([FromBody] CustomersModel newCustomer)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Customers", "AddNewCustomer");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addCustomerResult = await _customersService.InsertCustomerAsync(newCustomer);

                if (addCustomerResult > 0)
                    return Ok(new { SuccessMessage = "Successfully Added", newCustomer.CustomerName, newCustomer.PhoneNumber});
                else
                    return BadRequest(new { ErrorMessage = "Could Not Add" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "Could not add", ExceptionMessage = ex.Message });
            }
        }


        [HttpPut]
        public async Task<IActionResult> UpdateCustomer([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Customers", "UpdateCustomer");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var Customer = await _customersService.SelectCustomerByIdAsync(key);

                JsonConvert.PopulateObject(values, Customer);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updateCustomerResult = await _customersService.UpdateCustomerAsync(Customer);

                if (updateCustomerResult > 0)
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
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Customer.", ExceptionMessage = ex.Message });
            }
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteCustomer([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Customers", "DeleteCustomer");
            if (!hasPermission)
            {
                return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" });
            }
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deleteCustomerResult = await _customersService.DeleteCustomerAsync(key);
                if (deleteCustomerResult > 0)
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
