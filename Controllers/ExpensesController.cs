using IOMSYS.IServices;
using IOMSYS.Models;
using IOMSYS.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly IExpensesService _expensesService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IPermissionsService _permissionsService;

        public ExpensesController(IExpensesService expensesService, IPaymentTransactionService paymentTransactionService, IPermissionsService permissionsService)
        {
            _expensesService = expensesService;
            _paymentTransactionService = paymentTransactionService;
            _permissionsService = permissionsService;
        }

        public async Task<IActionResult> ExpensesPage()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Expenses", "ExpensesPage");
            if (!hasPermission) { return RedirectToAction("AccessDenied", "Access"); }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadExpenses()
        {
            var expenses = await _expensesService.GetAllExpensesAsync();
            return Json(expenses);
        }

        [HttpGet]
        public async Task<IActionResult> LoadExpensesByBranch(int branchId)
        {
            var Products = await _expensesService.GetAllExpensesByBranchAsync(branchId);
            return Json(Products);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewExpense([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Expenses", "AddNewExpense");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }

            try
            {
                var values = formData["values"];
                var newExpense = new ExpenseModel();
                JsonConvert.PopulateObject(values, newExpense);

                newExpense.PurchaseDate = DateTime.Now;
                newExpense.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (newExpense.ExpensesAmount < 0)
                    return BadRequest(new { ErrorMessage = "لا يمكن ادخال قيمه سالبه" });

                var branchBalance = await _paymentTransactionService.GetBranchAccountBalanceByPaymentAsync(newExpense.BranchId, newExpense.PaymentMethodId);
                if (newExpense.ExpensesAmount >= branchBalance)
                {
                    return BadRequest(new { ErrorMessage = "لا يوجد رصيد في الخزنة للخصم منه" });
                }

                int addExpenseResult = await _expensesService.InsertExpenseAsync(newExpense);

                if (addExpenseResult > 0)
                {
                    var newPaymentTransaction = new PaymentTransactionModel
                    {
                        BranchId = newExpense.BranchId,
                        PaymentMethodId = newExpense.PaymentMethodId,
                        Amount = newExpense.ExpensesAmount,
                        TransactionDate = newExpense.PurchaseDate,
                        ModifiedDate = DateTime.Now,
                        ModifiedUser = newExpense.UserId,
                        TransactionType = "خصم",
                        InvoiceId = addExpenseResult,
                    };
                    int paymentTransactionResult = await _paymentTransactionService.InsertPaymentTransactionAsync(newPaymentTransaction);

                    return Ok(new { SuccessMessage = "تم ادخال المصروف بنجاح" });
                }

                else
                    return BadRequest(new { ErrorMessage = "حدث خطأ اثناء الادخال" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "Could not add the expense", ExceptionMessage = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateExpense([FromForm] IFormCollection formData)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Expenses", "UpdateExpense");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }

            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var expense = await _expensesService.SelectExpenseByIdAsync(key);
                var oldexpense = await _expensesService.SelectExpenseByIdAsync(key);
                JsonConvert.PopulateObject(values, expense);

                decimal difference = 0;
                expense.PurchaseDate = DateTime.Now;
                expense.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (expense.ExpensesAmount < 0)
                    return BadRequest(new { ErrorMessage = "لا يمكن ادخال قيمه سالبه" });

                if (expense.ExpensesAmount > oldexpense.ExpensesAmount)
                {
                    difference = expense.ExpensesAmount - oldexpense.ExpensesAmount;
                    var branchBalance = await _paymentTransactionService.GetBranchAccountBalanceByPaymentAsync(expense.BranchId, expense.PaymentMethodId);
                    if (difference > branchBalance)
                    {
                        return BadRequest(new { ErrorMessage = "لا يوجد رصيد في الخزنة للخصم منه" });
                    }
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "لتعديل القيمة لمبلغ اقل من المدخل يرجي حذف العملية وادخالها مرة اخري صحيحة" });
                }

                int updateExpenseResult = await _expensesService.UpdateExpenseAsync(expense);
                if (updateExpenseResult > 0)
                {
                    if (difference > 0) 
                    {
                        expense.ExpensesAmount = difference;
                        await RecordPaymentTransaction(expense, expense.ExpensesId); 
                    }
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "حدث خطأ اثناء التعديل" });
                }

                return Ok(new { SuccessMessage = "تم التعديل المصروف بنجاح." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the expense.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteExpense(int key)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var hasPermission = await _permissionsService.HasPermissionAsync(userId, "Expenses", "DeleteExpense");
            if (!hasPermission) { return BadRequest(new { ErrorMessage = "ليس لديك صلاحية" }); }

            try
            {
                var expense = await _expensesService.SelectExpenseByIdAsync(key);
                int deleteExpenseResult = await _expensesService.DeleteExpenseAsync(key);
                if (deleteExpenseResult > 0)
                {
                    var paymentTransactions = await _paymentTransactionService.GetPaymentTransactionsByInvoiceIdAsync(key);

                    if (paymentTransactions != null && paymentTransactions.Any())
                    {
                        bool deleteFailed = false;

                        foreach (var paymentTransaction in paymentTransactions)
                        {
                            var deleteTransactionResult = await _paymentTransactionService.DeletePaymentTransactionAsync((int)paymentTransaction.TransactionId);

                            if (deleteTransactionResult <= 0)
                            {
                                deleteFailed = true;
                                break;
                            }
                        }

                        if (deleteFailed)
                        {
                            return BadRequest(new { ErrorMessage = "Failed to delete one or more related payment transactions." });
                        }
                    }

                    return Ok(new { SuccessMessage = "تم الحذف بنجاح" });
                }
                else
                    return BadRequest(new { ErrorMessage = "حدث خطأ اثناء الحذف" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }

        private async Task RecordPaymentTransaction(ExpenseModel model, int invoiceId)
        {
            var paymentTransaction = new PaymentTransactionModel
            {
                BranchId = model.BranchId,
                PaymentMethodId = model.PaymentMethodId,
                Amount = -model.ExpensesAmount,
                TransactionType = "خصم",
                TransactionDate = model.PurchaseDate,
                ModifiedUser = model.UserId,
                ModifiedDate = DateTime.Now,
                InvoiceId = invoiceId,
                Details = model.Notes,
            };
            await _paymentTransactionService.InsertPaymentTransactionAsync(paymentTransaction);
        }
    }
}
