﻿using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class SalesInvoicesService : ISalesInvoicesService
    {
        private readonly DapperContext _dapperContext;

        public SalesInvoicesService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<SalesInvoicesModel>> GetAllSalesInvoicesAsync()
        {
            var sql = @"
                SELECT si.SalesInvoiceId, si.TotalAmount, si.PaidUp, si.Remainder, si.SaleDate, 
                       c.CustomerName, b.BranchName, pm.PaymentMethodName, u.UserName
                FROM SalesInvoices si
                LEFT JOIN Customers c ON si.CustomerId = c.CustomerId
                LEFT JOIN Branches b ON si.BranchId = b.BranchId
                LEFT JOIN PaymentMethods pm ON si.PaymentMethodId = pm.PaymentMethodId
                LEFT JOIN Users u ON si.UserId = u.UserId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<SalesInvoicesModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<SalesInvoicesModel> GetSalesInvoiceByIdAsync(int salesInvoiceId)
        {
            var sql = @"
                SELECT si.SalesInvoiceId, si.TotalAmount, si.PaidUp, si.Remainder, si.SaleDate, 
                       c.CustomerName, b.BranchName, pm.PaymentMethodName, u.UserName
                FROM SalesInvoices si
                LEFT JOIN Customers c ON si.CustomerId = c.CustomerId
                LEFT JOIN Branches b ON si.BranchId = b.BranchId
                LEFT JOIN PaymentMethods pm ON si.PaymentMethodId = pm.PaymentMethodId
                LEFT JOIN Users u ON si.UserId = u.UserId
                WHERE si.SalesInvoiceId = @SalesInvoiceId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<SalesInvoicesModel>(sql, new { SalesInvoiceId = salesInvoiceId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertSalesInvoiceAsync(SalesInvoicesModel salesInvoice)
        {
            var sql = @"
                INSERT INTO SalesInvoices (CustomerId, TotalAmount, PaidUp, Remainder, BranchId, PaymentMethodId, UserId, SaleDate) 
                VALUES (@CustomerId, @TotalAmount, @PaidUp, @Remainder, @BranchId, @PaymentMethodId, @UserId, @SaleDate);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteScalarAsync<int>(sql, salesInvoice).ConfigureAwait(false);
            }
        }

        public async Task<int> UpdateSalesInvoiceAsync(SalesInvoicesModel salesInvoice)
        {
            var sql = @"
                UPDATE SalesInvoices 
                SET CustomerId = @CustomerId, TotalAmount = @TotalAmount, PaidUp = @PaidUp, Remainder = @Remainder, 
                    BranchId = @BranchId, PaymentMethodId = @PaymentMethodId, UserId = @UserId, SaleDate = @SaleDate
                WHERE SalesInvoiceId = @SalesInvoiceId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, salesInvoice).ConfigureAwait(false);
            }
        }
        public async Task<int> DeleteSalesInvoiceAsync(int salesInvoiceId)
        {
            var sql = @"DELETE FROM SalesInvoices WHERE SalesInvoiceId = @SalesInvoiceId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { SalesInvoiceId = salesInvoiceId }).ConfigureAwait(false);
            }
        }
    }
}