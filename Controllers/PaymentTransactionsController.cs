﻿using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class PaymentTransactionsController : Controller
    {
        private readonly IPaymentTransactionService _paymentTransactionService;

        public PaymentTransactionsController(IPaymentTransactionService paymentTransactionService)
        {
            _paymentTransactionService = paymentTransactionService;
        }

        [Authorize(Roles = "GenralManager,BranchManager")]
        public IActionResult PaymentTransactionsPage()
        {
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
        public async Task<IActionResult> GetBranchAccountBalance(int BranchId)
        {
            var paymentTransactions = await _paymentTransactionService.GetBranchAccountBalanceAsync(BranchId);
            return Json(paymentTransactions);
        }

        [HttpPost]
        [Authorize(Roles = "GenralManager,BranchManager")]
        public async Task<IActionResult> AddNewPaymentTransaction([FromBody] PaymentTransactionModel transaction)
        {
            try
            {
                transaction.ModifiedUser = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                transaction.ModifiedDate = DateTime.Now;

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int result = await _paymentTransactionService.InsertPaymentTransactionAsync(transaction);

                if (result > 0)
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
        public async Task<IActionResult> UpdatePaymentTransaction([FromBody] PaymentTransactionModel transaction)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int result = await _paymentTransactionService.UpdatePaymentTransactionAsync(transaction);

                if (result > 0)
                    return Ok(new { SuccessMessage = "Updated Successfully" });
                else
                    return BadRequest(new { ErrorMessage = "Could Not Update" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the Payment Transaction.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeletePaymentTransaction(int transactionId)
        {
            try
            {
                int result = await _paymentTransactionService.DeletePaymentTransactionAsync(transactionId);

                if (result > 0)
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