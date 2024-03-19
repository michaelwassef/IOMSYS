using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class SuppliersService : ISuppliersService
    {
        private readonly DapperContext _dapperContext;

        public SuppliersService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<SuppliersModel>> GetAllSuppliersAsync()
        {
            var sql = @"SELECT * FROM Suppliers";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<SuppliersModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<SuppliersModel?> SelectSupplierByIdAsync(int supplierId)
        {
            var sql = @"SELECT * FROM Suppliers WHERE SupplierId = @SupplierId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<SuppliersModel>(sql, new { SupplierId = supplierId }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<PurchaseInvoicesModel>> SelectSupplierInvoicesByIdAsync(int supplierId)
        {
            var sql = @"
                SELECT pi.PurchaseInvoiceId, pi.TotalAmount, pi.PaidUp, pi.SupplierId, pi.BranchId, pi.PaymentMethodId,
                pi.Remainder, s.SupplierName, b.BranchName, pm.PaymentMethodName, u.UserName, pi.PurchaseDate, pi.UserId, pi.PaidUpDate, pi.IsFullPaidUp, pi.Notes, pi.SalesInvoiceId
                FROM PurchaseInvoices pi
                LEFT JOIN Suppliers s ON pi.SupplierId = s.SupplierId
                LEFT JOIN Branches b ON pi.BranchId = b.BranchId
                LEFT JOIN PaymentMethods pm ON pi.PaymentMethodId = pm.PaymentMethodId
                LEFT JOIN Users u ON pi.UserId = u.UserId
                WHERE pi.SupplierId = @SupplierId ORDER BY pi.PurchaseInvoiceId DESC";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PurchaseInvoicesModel>(sql, new { SupplierId = supplierId }).ConfigureAwait(false);
            }
        }

        public async Task<dynamic> SelectSupplierSumsByIdAsync(int supplierId)
        {
            var sql = @"
            SELECT 
                COALESCE(SUM(pi.TotalAmount), 0) AS TotalAmountSum, 
                COALESCE(SUM(pi.PaidUp), 0) AS PaidUpSum, 
                COALESCE(SUM(pi.Remainder), 0) AS RemainderSum
            FROM PurchaseInvoices pi
            WHERE pi.SupplierId = @SupplierId;
            ";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.QuerySingleOrDefaultAsync(sql, new { SupplierId = supplierId }).ConfigureAwait(false);
                return result;
            }
        }

        public async Task<int> InsertSupplierAsync(SuppliersModel supplier)
        {
            var sql = @"INSERT INTO Suppliers (SupplierName, PhoneNumber1, PhoneNumber2, Address) 
                        VALUES (@SupplierName, @PhoneNumber1, @PhoneNumber2, @Address);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { supplier.SupplierName, supplier.PhoneNumber1, supplier.PhoneNumber2, supplier.Address }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateSupplierAsync(SuppliersModel supplier)
        {
            var sql = @"UPDATE Suppliers SET SupplierName = @SupplierName, PhoneNumber1 = @PhoneNumber1, PhoneNumber2 = @PhoneNumber2, Address = @Address WHERE SupplierId = @SupplierId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { supplier.SupplierName, supplier.PhoneNumber1, supplier.PhoneNumber2, supplier.Address, supplier.SupplierId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteSupplierAsync(int supplierId)
        {
            var sql = @"DELETE FROM Suppliers WHERE SupplierId = @SupplierId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { SupplierId = supplierId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
