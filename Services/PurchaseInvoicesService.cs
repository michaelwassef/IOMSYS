﻿using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class PurchaseInvoicesService : IPurchaseInvoicesService
    {
        private readonly DapperContext _dapperContext;

        public PurchaseInvoicesService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<PurchaseInvoicesModel>> GetAllPurchaseInvoicesAsync()
        {
            var sql = @"
                SELECT pi.PurchaseInvoiceId, pi.TotalAmount, pi.PaidUp, pi.Remainder, s.SupplierName, b.BranchName, pm.PaymentMethodName, u.UserName, pi.PurchaseDate
                FROM PurchaseInvoices pi
                LEFT JOIN Suppliers s ON pi.SupplierId = s.SupplierId
                LEFT JOIN Branches b ON pi.BranchId = b.BranchId
                LEFT JOIN PaymentMethods pm ON pi.PaymentMethodId = pm.PaymentMethodId
                LEFT JOIN Users u ON pi.UserId = u.UserId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PurchaseInvoicesModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<PurchaseInvoicesModel> GetPurchaseInvoiceByIdAsync(int purchaseInvoiceId)
        {
            var sql = @"
                SELECT pi.PurchaseInvoiceId, pi.TotalAmount, pi.PaidUp, pi.Remainder, s.SupplierName, b.BranchName, pm.PaymentMethodName, u.UserName, pi.PurchaseDate
                FROM PurchaseInvoices pi
                LEFT JOIN Suppliers s ON pi.SupplierId = s.SupplierId
                LEFT JOIN Branches b ON pi.BranchId = b.BranchId
                LEFT JOIN PaymentMethods pm ON pi.PaymentMethodId = pm.PaymentMethodId
                LEFT JOIN Users u ON pi.UserId = u.UserId
                WHERE pi.PurchaseInvoiceId = @PurchaseInvoiceId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<PurchaseInvoicesModel>(sql, new { PurchaseInvoiceId = purchaseInvoiceId }).ConfigureAwait(false);
            }
        }


        public async Task<int> InsertPurchaseInvoiceAsync(PurchaseInvoicesModel purchaseInvoice)
        {
            var sql = @"INSERT INTO PurchaseInvoices (TotalAmount, PaidUp, Remainder, SupplierId, BranchId, PaymentMethodId, UserId, PurchaseDate) 
                        VALUES (@TotalAmount, @PaidUp, @Remainder, @SupplierId, @BranchId, @PaymentMethodId, @UserId, @PurchaseDate);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteScalarAsync<int>(sql, purchaseInvoice).ConfigureAwait(false);
            }
        }

        public async Task<int> UpdatePurchaseInvoiceAsync(PurchaseInvoicesModel purchaseInvoice)
        {
            var sql = @"UPDATE PurchaseInvoices SET TotalAmount = @TotalAmount, PaidUp = @PaidUp, Remainder = @Remainder, SupplierId = @SupplierId, BranchId = @BranchId, PaymentMethodId = @PaymentMethodId, UserId = @UserId, PurchaseDate = @PurchaseDate
                        WHERE PurchaseInvoiceId = @PurchaseInvoiceId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, purchaseInvoice).ConfigureAwait(false);
            }
        }

        public async Task<int> DeletePurchaseInvoiceAsync(int purchaseInvoiceId)
        {
            var sql = @"DELETE FROM PurchaseInvoices WHERE PurchaseInvoiceId = @PurchaseInvoiceId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { PurchaseInvoiceId = purchaseInvoiceId }).ConfigureAwait(false);
            }
        }
    }
}