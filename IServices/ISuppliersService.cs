using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface ISuppliersService
    {
        Task<IEnumerable<SuppliersModel>> GetAllSuppliersAsync();
        Task<SuppliersModel?> SelectSupplierByIdAsync(int supplierId);
        Task<IEnumerable<PurchaseInvoicesModel>> SelectSupplierInvoicesByIdAsync(int supplierId);
        Task<dynamic> SelectSupplierSumsByIdAsync(int supplierId);
        Task<int> InsertSupplierAsync(SuppliersModel supplier);
        Task<int> UpdateSupplierAsync(SuppliersModel supplier); 
        Task<int> DeleteSupplierAsync(int supplierId);
    }
}
