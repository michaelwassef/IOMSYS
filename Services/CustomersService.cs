using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class CustomersService : ICustomersService
    {
        private readonly DapperContext _dapperContext;

        public CustomersService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<CustomersModel>> GetAllCustomersAsync()
        {
            var sql = @"SELECT * FROM Customers";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<CustomersModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<CustomersModel?> SelectCustomerByIdAsync(int customerId)
        {
            var sql = @"SELECT * FROM Customers WHERE CustomerId = @CustomerId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<CustomersModel>(sql, new { CustomerId = customerId }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<SalesInvoicesModel>> SelectCustomerInvoicesByIdAsync(int customerId)
        {
            var sql = @"
                SELECT si.SalesInvoiceId, si.CustomerId, c.CustomerName, si.TotalAmount, si.PaidUp, si.Remainder, si.BranchId,
                      b.BranchName, pm.PaymentMethodName, u.UserName,si.PaymentMethodId, si.UserId
                      , si.SaleDate, si.TotalDiscount, si.IsReturn, si.ReturnDate, si.PaidUpDate, si.IsFullPaidUp, si.Notes
                FROM SalesInvoices si
                LEFT JOIN Customers c ON si.CustomerId = c.CustomerId
                LEFT JOIN Branches b ON si.BranchId = b.BranchId
                LEFT JOIN PaymentMethods pm ON si.PaymentMethodId = pm.PaymentMethodId
                LEFT JOIN Users u ON si.UserId = u.UserId
                WHERE si.CustomerId = @CustomerId AND si.IsReturn = 0 ORDER BY si.SalesInvoiceId DESC";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<SalesInvoicesModel>(sql, new { CustomerId = customerId }).ConfigureAwait(false);
            }
        }

        public async Task<dynamic> SelectCustomerSumsByIdAsync(int customerId)
        {
            var sql = @"
            SELECT 
                COALESCE(SUM(si.TotalAmount), 0) AS TotalAmountSum, 
                COALESCE(SUM(si.PaidUp), 0) AS PaidUpSum, 
                COALESCE(SUM(si.Remainder), 0) AS RemainderSum
            FROM SalesInvoices si
            WHERE si.CustomerId = @CustomerId;
            ";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.QuerySingleOrDefaultAsync(sql, new { CustomerId = customerId }).ConfigureAwait(false);
                return result;
            }
        }

        public async Task<int> InsertCustomerAsync(CustomersModel customer)
        {
            var sql = @"INSERT INTO Customers (CustomerName, PhoneNumber, Address) 
                        VALUES (@CustomerName, @PhoneNumber, @Address);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { customer.CustomerName, customer.PhoneNumber, customer.Address }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateCustomerAsync(CustomersModel customer)
        {
            var sql = @"UPDATE Customers SET CustomerName = @CustomerName, PhoneNumber = @PhoneNumber, Address = @Address WHERE CustomerId = @CustomerId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { customer.CustomerName, customer.PhoneNumber, customer.Address, customer.CustomerId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteCustomerAsync(int customerId)
        {
            var sql = @"DELETE FROM Customers WHERE CustomerId = @CustomerId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { CustomerId = customerId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
