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
