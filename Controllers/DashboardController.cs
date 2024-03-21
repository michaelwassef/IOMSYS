using IOMSYS.IServices;
using Microsoft.AspNetCore.Mvc;

namespace IOMSYS.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFinancialDashboard(DateTime fromDate, DateTime toDate, int BranchId)
        {
            var dashboard = await _dashboardService.GetFinancialDashboardAsync(fromDate, toDate, BranchId);
            return Json(dashboard);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetTotalAmountInPurchaseInvoices(DateTime fromDate, DateTime toDate, int BranchId)
        //{
        //    var total = await _dashboardService.GetTotalAmountInPurchaseInvoicesAsync(fromDate, toDate, BranchId);
        //    return Json(new { total = total });
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetPaidUpInPurchaseInvoices(DateTime fromDate, DateTime toDate, int BranchId)
        //{
        //    var total = await _dashboardService.GetPaidUpInPurchaseInvoicesAsync(fromDate, toDate, BranchId);
        //    return Json(new { total = total });
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetRemainderInPurchaseInvoices(DateTime fromDate, DateTime toDate, int BranchId)
        //{
        //    var total = await _dashboardService.GetRemainderInPurchaseInvoicesAsync(fromDate, toDate, BranchId);
        //    return Json(new { total = total });
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetTotalAmountInSalesInvoices(DateTime fromDate, DateTime toDate, int BranchId)
        //{
        //    var total = await _dashboardService.GetTotalAmountInSalesInvoicesAsync(fromDate, toDate, BranchId);
        //    return Json(new { total = total });
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetPaidUpInSalesInvoices(DateTime fromDate, DateTime toDate, int BranchId)
        //{
        //    var total = await _dashboardService.GetPaidUpInSalesInvoicesAsync(fromDate, toDate, BranchId);
        //    return Json(new { total = total });
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetRemainderInSalesInvoices(DateTime fromDate, DateTime toDate, int BranchId)
        //{
        //    var total = await _dashboardService.GetRemainderInSalesInvoicesAsync(fromDate, toDate, BranchId);
        //    return Json(new { total = total });
        //}

        //[HttpGet]
        //public async Task<IActionResult> CalculateProfit(DateTime fromDate, DateTime toDate, int BranchId)
        //{
        //    var total = await _dashboardService.CalculateProfitAsync(fromDate, toDate, BranchId);
        //    return Json(new { total = total });
        //}

        [HttpGet]
        public async Task<IActionResult> GetExpensesAmount(DateTime fromDate, DateTime toDate, int BranchId)
        {
            var total = await _dashboardService.GetExpensesAmountInExpensesAsync(fromDate, toDate, BranchId);
            return Json(new { total = total });
        }

        [HttpGet]
        public async Task<IActionResult> GetDailySalesAmount(DateTime fromDate, DateTime toDate, int BranchId)
        {
            var data = await _dashboardService.GetDailySalesAmountAsync(fromDate, toDate, BranchId);
            return Json(data);
        }


        [HttpGet]
        public async Task<IActionResult> GetBestSalesAmount(DateTime fromDate, DateTime toDate, int BranchId)
        {
            var data = await _dashboardService.GetBestSaleAsync(fromDate, toDate, BranchId);
            return Json(data);
        }
    }
}
