﻿using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;
using System.Transactions;

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
        public async Task<IActionResult> LoadDetailsPaymentTransactionsByBranch(int branchId, DateTime fromdate, DateTime todate)
        {
            var paymentTransactions = await _paymentTransactionService.LoadDetailsPaymentTransactionsByBranchAsync(branchId, fromdate, todate);
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

                    var paymentTransactions = await _paymentTransactionService.GetBranchAccountBalanceByPaymentAsync(model.FromBranchId, model.FromPaymentMethodId);
                    var frombranchname = await _branchesService.SelectBranchByIdAsync(model.FromBranchId);
                    var tobranchname = await _branchesService.SelectBranchByIdAsync(model.ToBranchId);
                    if (model.Amount > paymentTransactions)
                    {
                        return Json(new { success = false, message = $"لا يوجد رصيد لتحويل من {frombranchname.BranchName} الي {tobranchname.BranchName} مبلغ {model.Amount}" });
                    }

                    if (!ModelState.IsValid)
                        return BadRequest(ModelState);

                    var frompaymentTransaction = new PaymentTransactionModel
                    {
                        BranchId = model.FromBranchId,
                        PaymentMethodId = model.FromPaymentMethodId,
                        Amount = -model.Amount,
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
                        Details = "محولة من " + frombranchname.BranchName + " - " + model.Notes,
                    };
                    await _paymentTransactionService.InsertPaymentTransactionAsync(topaymentTransaction);

                    var frombranshCus = await _branchesService.SelectBranchByIdAsync(model.FromBranchId);
                    var tobranshCus = await _branchesService.SelectBranchByIdAsync(model.ToBranchId);

                    decimal amountUtilized = await _paymentTransactionService.ProcessInvoicesAndUpdateBalancesBRANSHES(tobranshCus.SupplierId, model.FromBranchId, model.Amount);
                    await _permissionsService.LogActionAsync(userId, "POST", "PaymentTransactions", 0, "Move Amount : "+amountUtilized+" From : " +frombranshCus.BranchName+" To : "+tobranshCus.BranchName , model.FromBranchId);
                }
                return Json(new { success = true, message = "تم نقل المبالغ بنجاح." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Could not add", ExceptionMessage = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNewPaymentTransaction([FromBody] TransactionDetailModel transaction)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PaymentTransactions", "AddNewPaymentTransaction");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                transaction.ModifiedUser = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                transaction.ModifiedDate = DateTime.Now;
                transaction.TransactionType = "اضافة";

                if (transaction.Amount < 0)
                {
                    return BadRequest(new { ErrorMessage = "يجب ادخال قيمة اكبر من 0" });
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                transaction.TransactionDate = transaction.TransactionDate.Add(DateTime.Now.TimeOfDay);
                

                int result = await _paymentTransactionService.InsertPaymentTransactionAsync(transaction);

                if (result > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "POST", "PaymentTransactions", result, "Insert Amount : " + transaction.Amount + " To : " + transaction.BranchId + " - PaymentMethod : " + transaction.PaymentMethodId, transaction.BranchId);
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
        public async Task<IActionResult> UpdatePaymentTransaction(int id, [FromBody] JsonElement data)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PaymentTransactions", "UpdatePaymentTransaction");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var existing = await _paymentTransactionService.GetPaymentTransactionByIdAsync(id);

                var values = data.ToString();
                JsonConvert.PopulateObject(values, existing);


                int result = await _paymentTransactionService.UpdatePaymentTransactionAsync(existing);

                if (result > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "PUT", "PaymentTransactions", (int)existing.TransactionId, "Update Amount : " + existing.Amount + " To : " + existing.BranchId + " - PaymentMethod : " + existing.PaymentMethodId, existing.BranchId);
                    return Ok(new { SuccessMessage = "تم التعديل بنجاح" });
                }
                else
                    return BadRequest(new { ErrorMessage = "حدث خطأ اثناء التعديل" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Payment Transaction.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePaymentTransaction(int transactionId)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "PaymentTransactions", "DeletePaymentTransaction");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }
            try
            {
                var transaction = await _paymentTransactionService.GetPaymentTransactionByIdAsync(transactionId);
                int result = await _paymentTransactionService.DeletePaymentTransactionAsync(transactionId);

                if (result > 0)
                {
                    await _permissionsService.LogActionAsync(userId, "DELETE", "PaymentTransactions", (int)transaction.TransactionId, "Delete Amount : " + transaction.Amount + " To : " + transaction.BranchId + " - PaymentMethod : " + transaction.PaymentMethodId, transaction.BranchId);
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
