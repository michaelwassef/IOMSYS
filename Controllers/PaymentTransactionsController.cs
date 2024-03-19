using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Reports;
using IOMSYS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class PaymentTransactionsController : Controller
    {
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IPermissionsService _permissionsService;
        private readonly IBranchesService _branchesService;

        public PaymentTransactionsController(IPaymentTransactionService paymentTransactionService, IPermissionsService permissionsService, IBranchesService branchesService)
        {
            _paymentTransactionService = paymentTransactionService;
            _permissionsService = permissionsService;
            _branchesService = branchesService;
        }

        public async Task<IActionResult> PaymentTransactionsPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PaymentTransactions", "PaymentTransactionsPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadPaymentTransactions()
        {
            var paymentTransactions = await _paymentTransactionService.GetAllPaymentTransactionsAsync();
            return Json(paymentTransactions);
        }

        [HttpGet]
        public async Task<IActionResult> LoadPaymentTransactionsByBranch(int branchId)
        {
            var paymentTransactions = await _paymentTransactionService.LoadPaymentTransactionsByBranchAsync(branchId);
            return Json(paymentTransactions);
        }

        [HttpGet]
        public async Task<IActionResult> LoadDeitalsPaymentTransactionsByBranch(int branchId)
        {
            var paymentTransactions = await _paymentTransactionService.LoadDetailsPaymentTransactionsByBranchAsync(branchId);
            return Json(paymentTransactions);
        }

        [HttpGet]
        public async Task<IActionResult> GetBranchAccountBalance(int BranchId)
        {
            var paymentTransactions = await _paymentTransactionService.GetBranchAccountBalanceAsync(BranchId);
            return Json(paymentTransactions);
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountBalanceByPaymentMethodIdAndBranchId(int BranchId, int PaymentMethodId)
        {
            var paymentTransactions = await _paymentTransactionService.GetBranchAccountBalanceByPaymentAsync(BranchId, PaymentMethodId);
            return Json(paymentTransactions);
        }

        [HttpPost]
        public async Task<IActionResult> MoveBalance([FromBody] MovePaymentBatchModel batchModel)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PaymentTransactions", "MoveBalance");
            if (!hasPermission) { return BadRequest(new { message = "ليس لديك صلاحية" }); }

            try
            {
                foreach (var model in batchModel.Items)
                {
                    int? BranchId = await _branchesService.SelectBranchIdByManagerIdAsync(userId);
                    if (model.FromBranchId != BranchId)
                    {
                        return Json(new { success = false, message = "ليس لديك صلاحية للتحويل من هذا الفرع" });
                    }

                    if (!ModelState.IsValid)
                        return BadRequest(ModelState);

                    var frombranchname = await _branchesService.SelectBranchByIdAsync(model.FromBranchId);
                    var tobranchname = await _branchesService.SelectBranchByIdAsync(model.ToBranchId);
                    var frompaymentTransaction = new PaymentTransactionModel
                    {
                        BranchId = model.FromBranchId,
                        PaymentMethodId = model.FromPaymentMethodId,
                        Amount = model.Amount,
                        TransactionType = "خصم",
                        TransactionDate = DateTime.Now,
                        ModifiedUser = userId,
                        ModifiedDate = DateTime.Now,
                        Details = "محولة الي " + tobranchname.BranchName + " - " + model.Notes,
                    };
                    await _paymentTransactionService.InsertPaymentTransactionAsync(frompaymentTransaction);

                    var topaymentTransaction = new PaymentTransactionModel
                    {
                        BranchId = model.ToBranchId,
                        PaymentMethodId = model.ToPaymentMethodId,
                        Amount = model.Amount,
                        TransactionType = "اضافة",
                        TransactionDate = DateTime.Now,
                        ModifiedUser = userId,
                        ModifiedDate = DateTime.Now,
                        Details = "محولة من "+ frombranchname.BranchName + " - "+model.Notes,
                    };
                    await _paymentTransactionService.InsertPaymentTransactionAsync(topaymentTransaction);

                    var frombranshCus = await _branchesService.SelectBranchByIdAsync(model.FromBranchId);
                    var tobranshCus = await _branchesService.SelectBranchByIdAsync(model.ToBranchId);
                    decimal amountUtilized = await _paymentTransactionService.ProcessInvoicesAndUpdateBalancesBRANSHES(model.FromBranchId, tobranshCus.SupplierId, model.Amount);
                }
                return Json(new { success = true, message = "تم نقل المبالغ بنجاح." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Could not add", ExceptionMessage = ex.Message });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> AddNewPaymentTransaction([FromBody] PaymentTransactionModel transaction)
        //{
        //    int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        //    var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PaymentTransactions", "AddNewPaymentTransaction");
        //    if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
        //    try
        //    {
        //        transaction.ModifiedUser = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
        //        transaction.ModifiedDate = DateTime.Now;

        //        if (!ModelState.IsValid)
        //            return BadRequest(ModelState);

        //        int result = await _paymentTransactionService.InsertPaymentTransactionAsync(transaction);

        //        if (result > 0)
        //            return Ok(new { SuccessMessage = "Successfully Added" });
        //        else
        //            return BadRequest(new { ErrorMessage = "Could Not Add" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { ErrorMessage = "Could not add", ExceptionMessage = ex.Message });
        //    }
        //}

        //[HttpPut]
        //public async Task<IActionResult> UpdatePaymentTransaction([FromBody] PaymentTransactionModel transaction)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            return BadRequest(ModelState);

        //        int result = await _paymentTransactionService.UpdatePaymentTransactionAsync(transaction);

        //        if (result > 0)
        //            return Ok(new { SuccessMessage = "Updated Successfully" });
        //        else
        //            return BadRequest(new { ErrorMessage = "Could Not Update" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { ErrorMessage = "An error occurred while updating the Payment Transaction.", ExceptionMessage = ex.Message });
        //    }
        //}

        //[HttpDelete]
        //public async Task<IActionResult> DeletePaymentTransaction(int transactionId)
        //{
        //    try
        //    {
        //        int result = await _paymentTransactionService.DeletePaymentTransactionAsync(transactionId);

        //        if (result > 0)
        //            return Ok(new { SuccessMessage = "Deleted Successfully" });
        //        else
        //            return BadRequest(new { ErrorMessage = "Could Not Delete" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
        //    }
        //}
    }
}
