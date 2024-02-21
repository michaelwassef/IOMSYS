using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IOMSYS.Controllers
{
    [Authorize(Roles = "GenralManager,BranchManager,Employee")]
    public class ExpensesController : Controller
    {
        private readonly IExpensesService _expensesService;
        private readonly IPaymentTransactionService _paymentTransactionService;

        public ExpensesController(IExpensesService expensesService, IPaymentTransactionService paymentTransactionService)
        {
            _expensesService = expensesService;
            _paymentTransactionService = paymentTransactionService;
        }

        [Authorize(Roles = "GenralManager,BranchManager")]
        public IActionResult ExpensesPage()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "GenralManager,BranchManager,Employee")]
        public async Task<IActionResult> LoadExpenses()
        {
            var expenses = await _expensesService.GetAllExpensesAsync();
            return Json(expenses);
        }

        [HttpPost]
        [Authorize(Roles = "GenralManager,BranchManager")]
        public async Task<IActionResult> AddNewExpense([FromForm] IFormCollection formData)
        {
            try
            {
                var values = formData["values"];
                var newExpense = new ExpenseModel();
                JsonConvert.PopulateObject(values, newExpense);

                newExpense.PurchaseDate = DateTime.Now;
                newExpense.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

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

                    return Ok(new { SuccessMessage = "Expense Successfully Added" });
                }

                else
                    return BadRequest(new { ErrorMessage = "Could Not Add Expense" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "Could not add the expense", ExceptionMessage = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = "GenralManager,BranchManager")]
        public async Task<IActionResult> UpdateExpense([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var values = formData["values"];
                var expense = await _expensesService.SelectExpenseByIdAsync(key);

                if (expense == null)
                    return NotFound(new { ErrorMessage = "Expense Not Found" });

                JsonConvert.PopulateObject(values, expense);

                expense.PurchaseDate = DateTime.Now;
                expense.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int updateExpenseResult = await _expensesService.UpdateExpenseAsync(expense);

                if (updateExpenseResult > 0)
                {
                    var paymentTransaction = await _paymentTransactionService.GetPaymentTransactionByInvoiceIdAsync(expense.ExpensesId);
                    if (paymentTransaction != null)
                    {
                        paymentTransaction.Amount = expense.ExpensesAmount;
                        paymentTransaction.BranchId = expense.BranchId;
                        paymentTransaction.PaymentMethodId = expense.PaymentMethodId;

                        var updateTransactionResult = await _paymentTransactionService.UpdatePaymentTransactionAsync(paymentTransaction);
                        if (updateTransactionResult <= 0)
                        {
                            return BadRequest(new { ErrorMessage = "Could not update the related payment transaction." });
                        }
                    }
                    else
                    {
                        return BadRequest(new { ErrorMessage = "No related payment transaction found for update." });
                    }
                }
                else
                {
                    return BadRequest(new { ErrorMessage = "Could Not Update Expense" });
                }

                return Ok(new { SuccessMessage = "Payment transaction updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred while updating the expense.", ExceptionMessage = ex.Message });
            }
        }

        [HttpDelete]
        [Authorize(Roles = "GenralManager")]
        public async Task<IActionResult> DeleteExpense([FromForm] IFormCollection formData)
        {
            try
            {
                var key = Convert.ToInt32(formData["key"]);
                var expense = await _expensesService.SelectExpenseByIdAsync(key);
                int deleteExpenseResult = await _expensesService.DeleteExpenseAsync(key);
                if (deleteExpenseResult > 0)
                {
                    var paymentTransaction = await _paymentTransactionService.GetPaymentTransactionByInvoiceIdAsync(expense.ExpensesId);
                    if (paymentTransaction != null)
                    {
                        // Delete the payment transaction
                        var deleteTransactionResult = await _paymentTransactionService.DeletePaymentTransactionAsync((int)paymentTransaction.TransactionId);
                        if (deleteTransactionResult <= 0)
                        {
                            return BadRequest(new { ErrorMessage = "Failed to delete the related payment transaction." });
                        }
                    }
                    return Ok(new { SuccessMessage = "Expense Deleted Successfully" });
                }
                else
                    return BadRequest(new { ErrorMessage = "Could Not Delete Expense" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            }
        }
    }
}
