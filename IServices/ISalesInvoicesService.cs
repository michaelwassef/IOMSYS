using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface ISalesInvoicesService
    {
        Task<IEnumerable<SalesInvoicesModel>> GetAllSalesInvoicesAsync();
        Task<SalesInvoicesModel> GetSalesInvoiceByIdAsync(int salesInvoiceId);
        Task<int> InsertSalesInvoiceAsync(SalesInvoicesModel salesInvoice);
        Task<int> UpdateSalesInvoiceAsync(SalesInvoicesModel salesInvoice);
        Task<int> DeleteSalesInvoiceAsync(int salesInvoiceId);
    }
}
