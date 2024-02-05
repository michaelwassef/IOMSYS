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
