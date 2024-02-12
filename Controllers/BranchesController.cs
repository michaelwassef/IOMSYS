using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class BranchesController : Controller
    {
        private readonly IBranchesService _branchesService;

        public BranchesController(IBranchesService branchesService)
        {
            _branchesService = branchesService;
        }

        [Authorize(Roles = "GenralManager")]
        public IActionResult BranchesPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadBranches()
        {
            var branches = await _branchesService.GetAllBranchesAsync();
            return Json(branches);
        }

        [HttpPost]
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> AddNewBranch([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var newbranch = new BranchesModel();
                JsonConvert.PopulateObject(values, newbranch);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int addBranchResult = await _branchesService.InsertBranchAsync(newbranch);

                if (addBranchResult > 0)
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
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> UpdateBranch([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var branch = await _branchesService.SelectBranchByIdAsync(key);

                JsonConvert.PopulateObject(values, branch);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updateBranchResult = await _branchesService.UpdateBranchAsync(branch);

                if (updateBranchResult > 0)
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
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Branch.", ExceptionMessage = ex.Message });
            }
        }


        [HttpDelete]
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeleteBranch([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                int deleteBranchResult = await _branchesService.DeleteBranchAsync(key);
                if (deleteBranchResult > 0)
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
