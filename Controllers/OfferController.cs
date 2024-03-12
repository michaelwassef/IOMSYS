using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class OfferController : Controller
    {
        private readonly IOfferService _offerService;
        private readonly IPermissionsService _permissionsService;

        public OfferController(IOfferService offerService, IPermissionsService permissionsService)
        {
            _offerService = offerService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> OffersPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Offer", "OffersPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOffers()
        {
            try
            {
                var offers = await _offerService.GetAllOffersAsync();
                return Json(offers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOffersWithoutDetails()
        {
            try
            {
                var offers = await _offerService.GetOffersWithoutDetailsAsync();
                return Json(offers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActiveOffers()
        {
            var offers = await _offerService.GetAllOffersAsync();
            return Json(offers);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetilasOffersByOfferId(int OfferId)
        {
            var offers = await _offerService.GetOfferDetailByOfferIdAsync(OfferId);
            return Json(offers);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetilasOffersListByOfferId(int OfferId)
        {
            var offers = await _offerService.GetOfferDetailByOfferIdlistAsync(OfferId);
            return Json(offers);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOffer([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Offer", "CreateOffer");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }

            try
            {
                var values = formData["values"];
                var offer = new OfferModel();
                JsonConvert.PopulateObject(values, offer);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var offerId = await _offerService.CreateOfferAsync(offer);
                if (offerId > 0)
                    return Ok(new { SuccessMessage = "Offer created successfully", OfferId = offerId });
                else
                    return BadRequest(new { ErrorMessage = "Could not create offer" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "Could not create offer", ExceptionMessage = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOffer([FromBody] OfferModel offer)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Offer", "UpdateOffer");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var success = await _offerService.UpdateOfferAsync(offer);
                if (success)
                    return Ok(new { SuccessMessage = "Offer updated successfully" });
                else
                    return BadRequest(new { ErrorMessage = "Could not update offer" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the offer.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOffer(int offerId)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Offer", "DeleteOffer");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }

            try
            {
                var success = await _offerService.DeleteOfferAsync(offerId);
                if (success)
                    return Ok(new { SuccessMessage = "Offer deleted successfully" });
                else
                    return BadRequest(new { ErrorMessage = "Could not delete offer" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOfferDetail([FromForm] OfferDetailModel offerDetail)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Offer", "CreateOffer");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }



                var offerDetailId = await _offerService.CreateOfferDetailAsync(offerDetail);
                if (offerDetailId > 0)
                    return Ok(new { SuccessMessage = "Offer detail created successfully", OfferDetailId = offerDetailId });
                else
                    return BadRequest(new { ErrorMessage = "Could not create offer detail" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "Could not create offer detail", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOfferDetail(int offerDetailId)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Offer", "DeleteOffer");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }

            try
            {
                var success = await _offerService.DeleteOfferDetailAsync(offerDetailId);
                if (success)
                    return Ok(new { SuccessMessage = "Offer detail deleted successfully" });
                else
                    return BadRequest(new { ErrorMessage = "Could not delete offer detail" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }
    }
}
