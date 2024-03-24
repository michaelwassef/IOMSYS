using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface ICustomersService
    {
        Task<IEnumerable<CustomersModel>> GetAllCustomersAsync();
        Task<CustomersModel?> SelectCustomerByIdAsync(int customerId);
        Task<dynamic> SelectCustomerSumsByIdAsync(int customerId);
        Task<IEnumerable<SalesInvoicesModel>> SelectCustomerInvoicesByIdAsync(int customerId);
        Task<int> InsertCustomerAsync(CustomersModel customer);
        Task<int> UpdateCustomerAsync(CustomersModel customer);
        Task<int> DeleteCustomerAsync(int customerId);
    }
}
