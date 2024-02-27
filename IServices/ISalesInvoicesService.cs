using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface ISalesInvoicesService
    {
        Task<IEnumerable<SalesInvoicesModel>> GetAllSalesInvoicesAsync();
        Task<IEnumerable<SalesInvoicesModel>> GetAllSalesInvoicesByBranshAsync(int BranchId);
        Task<SalesInvoicesModel> GetSalesInvoiceByIdAsync(int salesInvoiceId);
        Task<int> InsertSalesInvoiceAsync(SalesInvoicesModel salesInvoice);
        Task<int> UpdateSalesInvoiceAsync(SalesInvoicesModel salesInvoice);
        Task<int> UpdateReturnSalesInvoiceAsync(int SalesInvoiceId);
        Task<int> DeleteSalesInvoiceAsync(int salesInvoiceId);
        Task<int> GetLastInvoiceIdAsync();
    }
}
