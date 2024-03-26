using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly DapperContext _dapperContext;
        private readonly ISalesInvoicesService _salesInvoicesService;

        public PaymentTransactionService(DapperContext dapperContext, ISalesInvoicesService salesInvoicesService)
        {
            _dapperContext = dapperContext;
            _salesInvoicesService = salesInvoicesService;
        }

        public async Task<IEnumerable<PaymentTransactionModel>> GetAllPaymentTransactionsAsync()
        {
            var sql = @"SELECT * FROM PaymentTransactions";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PaymentTransactionModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<PaymentTransactionModel>> LoadPaymentTransactionsByBranchAsync(int branchId)
        {
            var sql = @"SELECT * FROM PaymentTransactions WHERE BranchId = @BranchId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PaymentTransactionModel>(sql, new { BranchId = branchId }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<TransactionDetailModel>> LoadDetailsPaymentTransactionsByBranchAsync(int branchId, DateTime fromdate, DateTime todate)
        {
            var sql = @"SELECT 
                P.TransactionId,
                P.BranchId,
                P.PaymentMethodId,
                P.TransactionType,
                P.TransactionDate as TransactionDate,
                P.Amount,
                P.Details,
                P.ModifiedDate,
                P.ModifiedUser,
                P.InvoiceId,
                b.BranchName,
                u.UserName,
                pm.PaymentMethodName,
                CASE 
                    WHEN P.TransactionType = 'اضافة' AND EXISTS (SELECT 1 FROM SalesInvoices WHERE SalesInvoiceId = P.InvoiceId) THEN 'فاتورة مبيعات'
                    WHEN EXISTS (SELECT 1 FROM PurchaseInvoices WHERE PurchaseInvoiceId = P.InvoiceId) THEN 'فاتورة مشتريات'
                    WHEN P.TransactionType = 'خصم' AND EXISTS (SELECT 1 FROM SalesInvoices WHERE SalesInvoiceId = P.InvoiceId) THEN 'مرتجع فاتورة مبيعات'
                    WHEN EXISTS (SELECT 1 FROM Expenses WHERE ExpensesId = P.InvoiceId) THEN 'مصروفات'
                    ELSE P.TransactionType 
                END AS InvoiceType,
               CASE 
                    WHEN EXISTS (SELECT 1 FROM SalesInvoices WHERE SalesInvoiceId = P.InvoiceId) THEN c.CustomerName
                    WHEN EXISTS (SELECT 1 FROM PurchaseInvoices WHERE PurchaseInvoiceId = P.InvoiceId) THEN s.SupplierName
                    ELSE NULL 
                END AS EntityName
            FROM 
                PaymentTransactions P
            INNER JOIN 
                Branches b ON P.BranchId = b.BranchId
            INNER JOIN 
                Users u ON P.ModifiedUser = u.UserId
            INNER JOIN 
                PaymentMethods pm ON P.PaymentMethodId = pm.PaymentMethodId
            LEFT JOIN 
                SalesInvoices si ON si.SalesInvoiceId = P.InvoiceId
            LEFT JOIN 
                PurchaseInvoices pi ON pi.PurchaseInvoiceId = P.InvoiceId
            LEFT JOIN 
                Customers c ON c.CustomerId = si.CustomerId
            LEFT JOIN 
                Suppliers s ON s.SupplierId = pi.SupplierId
            WHERE
                P.BranchId = @BranchId AND P.ModifiedDate >= @fromdate AND P.ModifiedDate <= @todate
                ORDER BY P.ModifiedDate DESC;";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<TransactionDetailModel>(sql, new { BranchId = branchId, fromdate, todate }).ConfigureAwait(false);
            }
        }

        public async Task<PaymentTransactionModel> GetPaymentTransactionByIdAsync(int transactionId)
        {
            var sql = @"SELECT * FROM PaymentTransactions WHERE TransactionId = @TransactionId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryFirstOrDefaultAsync<PaymentTransactionModel>(sql, new { TransactionId = transactionId }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<PaymentTransactionModel>> GetPaymentTransactionsByInvoiceIdAsync(int InvoiceId)
        {
            var sql = @"SELECT * FROM PaymentTransactions WHERE InvoiceId = @InvoiceId";
            using (var db = _dapperContext.CreateConnection())
            {
                // Using QueryAsync to fetch multiple rows
                return await db.QueryAsync<PaymentTransactionModel>(sql, new { InvoiceId }).ConfigureAwait(false);
            }
        }

        public async Task<decimal> GetBranchAccountBalanceAsync(int branchId)
        {
            var sql = @"
                SELECT COALESCE(SUM(Amount), 0) AS Balance
                FROM PaymentTransactions 
                WHERE BranchId = @BranchId";

            using (var db = _dapperContext.CreateConnection())
            {
                var balance = await db.QueryFirstOrDefaultAsync<decimal?>(sql, new { BranchId = branchId }).ConfigureAwait(false);
                if (balance.HasValue)
                {
                    return balance.Value;
                }
                else
                {
                    throw new Exception("Balance is null for the given branch.");
                }
            }
        }

        public async Task<decimal> GetBranchAccountBalanceByPaymentAsync(int BranchId, int PaymentMethodId)
        {
            var sql = @"
                SELECT COALESCE(SUM(Amount), 0) AS Balance
                FROM PaymentTransactions 
                WHERE BranchId = @BranchId AND PaymentMethodId = @PaymentMethodId";

            using (var db = _dapperContext.CreateConnection())
            {
                var balance = await db.QueryFirstOrDefaultAsync<decimal?>(sql, new { BranchId, PaymentMethodId }).ConfigureAwait(false);
                if (balance.HasValue)
                {
                    return balance.Value;
                }
                else
                {
                    throw new Exception("Balance is null for the given branch.");
                }
            }
        }

        public async Task<int> InsertPaymentTransactionAsync(PaymentTransactionModel transaction)
        {
            var sql = @"INSERT INTO PaymentTransactions (BranchId, PaymentMethodId, TransactionType, TransactionDate, Amount, Details, ModifiedDate, ModifiedUser, InvoiceId) 
                        VALUES (@BranchId, @PaymentMethodId, @TransactionType, @TransactionDate, @Amount, @Details, @ModifiedDate, @ModifiedUser, @InvoiceId);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, transaction).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdatePaymentTransactionAsync(PaymentTransactionModel transaction)
        {
            var sql = @"UPDATE PaymentTransactions 
                        SET BranchId = @BranchId, PaymentMethodId = @PaymentMethodId, TransactionType = @TransactionType, 
                            TransactionDate = @TransactionDate, Amount = @Amount, Details = @Details, 
                            ModifiedDate = @ModifiedDate, ModifiedUser = @ModifiedUser , InvoiceId = @InvoiceId
                        WHERE TransactionId = @TransactionId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, transaction).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeletePaymentTransactionAsync(int transactionId)
        {
            var sql = @"DELETE FROM PaymentTransactions WHERE TransactionId = @TransactionId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { TransactionId = transactionId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> InsertPaymentTransactionAsync(TransactionDetailModel transaction)
        {
            var sql = @"INSERT INTO PaymentTransactions (BranchId, PaymentMethodId, TransactionType, TransactionDate, Amount, Details, ModifiedDate, ModifiedUser, InvoiceId) 
                        VALUES (@BranchId, @PaymentMethodId, @TransactionType, @TransactionDate, @Amount, @Details, @ModifiedDate, @ModifiedUser, @InvoiceId);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, transaction).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdatePaymentTransactionAsync(TransactionDetailModel transaction)
        {
            var sql = @"UPDATE PaymentTransactions 
                        SET BranchId = @BranchId, PaymentMethodId = @PaymentMethodId, TransactionType = @TransactionType, 
                            TransactionDate = @TransactionDate, Amount = @Amount, Details = @Details, 
                            ModifiedDate = @ModifiedDate, ModifiedUser = @ModifiedUser , InvoiceId = @InvoiceId
                        WHERE TransactionId = @TransactionId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, transaction).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<decimal> CalculateAmountOwedByBranchAsync(int SupplierId, int BranchId)
        {
            var sql = @"SELECT COALESCE(SUM(pi.Remainder), 0) AS TotalOwed
                    FROM PurchaseInvoices pi
                    WHERE pi.SupplierId = @SupplierId AND pi.BranchId = @BranchId AND pi.IsFullPaidUp = 0";
            using (var db = _dapperContext.CreateConnection())
            {
                var totalOwed = await db.QueryAsync<decimal>(sql, new { SupplierId, BranchId })
                                        .ConfigureAwait(false);
                return totalOwed.FirstOrDefault();
            }
        }
        public async Task<IEnumerable<int>> GetNotFullyPaidInvoiceIdsAsync(int branchId, int SupplierId)
        {
            var sql = @"
                SELECT PurchaseInvoiceId
                FROM PurchaseInvoices
                WHERE BranchId = @BranchId AND SupplierId = @SupplierId AND IsFullPaidUp = 0";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<int>(sql, new { BranchId = branchId, SupplierId = SupplierId }).ConfigureAwait(false);
            }
        }
        public async Task<PurchaseInvoicesModel> GetPurchaseInvoiceByIdAsync(int purchaseInvoiceId)
        {
            var sql = @"
                SELECT pi.PurchaseInvoiceId, pi.TotalAmount, pi.PaidUp,pi.SupplierId,pi.BranchId,pi.PaymentMethodId, pi.Remainder, s.SupplierName, 
                b.BranchName, pm.PaymentMethodName, u.UserName, pi.PurchaseDate, pi.UserId, pi.PaidUpDate, pi.IsFullPaidUp, pi.Notes, pi.SalesInvoiceId
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
        public async Task<int> UpdatePurchaseInvoiceAsync(int PurchaseInvoiceId, decimal PaidUp, decimal Remainder, bool IsFullPaidUp)
        {
            var sql = @"UPDATE PurchaseInvoices SET PaidUp = @PaidUp, Remainder = @Remainder, IsFullPaidUp = @IsFullPaidUp
                        WHERE PurchaseInvoiceId = @PurchaseInvoiceId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { PurchaseInvoiceId, PaidUp, Remainder, IsFullPaidUp }).ConfigureAwait(false);
            }
        }

        public async Task<decimal> ProcessInvoicesAndUpdateBalances(int SupplierId, int toBranchId, decimal amountToSpend)
        {
            var amountOwed = await CalculateAmountOwedByBranchAsync(SupplierId, toBranchId);
            var spendingAmount = Math.Min(amountOwed, amountToSpend);
            var initialSpendingAmount = spendingAmount;
            var invoiceIds = await GetNotFullyPaidInvoiceIdsAsync(toBranchId, SupplierId);

            foreach (var invoiceId in invoiceIds)
            {
                if (spendingAmount <= 0)
                    break;

                var invoice = await GetPurchaseInvoiceByIdAsync(invoiceId);
                var amountToPay = Math.Min(invoice.Remainder, spendingAmount);
                var newPaidUpAmount = invoice.PaidUp + amountToPay;
                var newRemainder = invoice.TotalAmount - newPaidUpAmount;
                bool isFullPaidUp = newRemainder == 0;

                await UpdatePurchaseInvoiceAsync(invoiceId, newPaidUpAmount, newRemainder, isFullPaidUp);
                var newinvoice = await GetPurchaseInvoiceByIdAsync(invoiceId);
                if (amountToPay > 0)
                {
                    newinvoice.PaidUp = amountToPay;
                    newinvoice.Notes = "دفعة من فاتورة المشتريات #" + newinvoice.PurchaseInvoiceId;
                    await RecordPaymentTransaction(newinvoice, invoiceId);
                }
                spendingAmount -= amountToPay;
            }
            var amountUtilized = initialSpendingAmount - spendingAmount;
            return amountUtilized;
        }
        public async Task<decimal> ProcessInvoicesAndUpdateBalancesBRANSHES(int SupplierId, int toBranchId, decimal amountToSpend)
        {
            var amountOwed = await CalculateAmountOwedByBranchAsync(SupplierId, toBranchId);
            var spendingAmount = Math.Min(amountOwed, amountToSpend);
            var initialSpendingAmount = spendingAmount;
            var invoiceIds = await GetNotFullyPaidInvoiceIdsAsync(toBranchId, SupplierId);

            foreach (var invoiceId in invoiceIds)
            {
                if (spendingAmount <= 0)
                    break;

                var invoice = await GetPurchaseInvoiceByIdAsync(invoiceId);

                var amountToPay = Math.Min(invoice.Remainder, spendingAmount);
                var newPaidUpAmount = invoice.PaidUp + amountToPay;
                var newRemainder = invoice.TotalAmount - newPaidUpAmount;
                bool isFullPaidUp = newRemainder == 0;

                await UpdatePurchaseInvoiceAsync(invoiceId, newPaidUpAmount, newRemainder, isFullPaidUp);

                spendingAmount -= amountToPay;

                var saleInvoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(invoice.SalesInvoiceId);
                if (saleInvoice != null)
                {
                    saleInvoice.PaidUp = newPaidUpAmount;
                    saleInvoice.Remainder = saleInvoice.TotalAmount - newPaidUpAmount;
                    saleInvoice.IsFullPaidUp = saleInvoice.PaidUp == saleInvoice.TotalAmount;

                    int updateSales = await _salesInvoicesService.UpdateSalesInvoiceAsync(saleInvoice);
                    //var newinvoice = await _salesInvoicesService.GetSalesInvoiceByIdAsync(saleInvoice.SalesInvoiceId);
                    //if (newPaidUpAmount > 0)
                    //{
                    //    newinvoice.Notes = "دفعة من فاتورة المبيعات #" + saleInvoice.SalesInvoiceId;
                    //    await RecordPaymentTransaction(newinvoice, saleInvoice.SalesInvoiceId);
                    //}
                }
            }
            var amountUtilized = initialSpendingAmount - spendingAmount;
            return amountUtilized;
        }
        public async Task<decimal> ProcessInvoicesAndUpdateBalancesS(int customerId, int toBranchId, decimal amountToSpend, int PaymentMethodId)
        {
            var amountOwed = await CalculateAmountOwedByBranchAsyncS(customerId, toBranchId);
            var spendingAmount = Math.Min(amountOwed, amountToSpend);
            var initialSpendingAmount = spendingAmount;
            var invoiceIds = await GetNotFullyPaidInvoiceIdsAsyncS(toBranchId, customerId);

            foreach (var invoiceId in invoiceIds)
            {
                if (spendingAmount <= 0)
                    break;

                var invoice = await GetSalesInvoiceByIdAsyncS(invoiceId);

                var amountToPay = Math.Min(invoice.Remainder, spendingAmount);
                var newPaidUpAmount = invoice.PaidUp + amountToPay;
                var newRemainder = invoice.TotalAmount - newPaidUpAmount;
                bool isFullPaidUp = newRemainder == 0;

                await UpdateSalesInvoiceAsyncS(invoiceId, newPaidUpAmount, newRemainder, isFullPaidUp);
                var newinvoice = await GetSalesInvoiceByIdAsyncS(invoiceId);
                if (amountToPay > 0)
                {
                    newinvoice.PaidUp = amountToPay;
                    newinvoice.PaymentMethodId = PaymentMethodId;
                    newinvoice.Notes = "دفعة من فاتورة المبيعات #" + newinvoice.SalesInvoiceId;
                    await RecordPaymentTransaction(newinvoice, invoiceId);
                }
                spendingAmount -= amountToPay;
            }
            var amountUtilized = initialSpendingAmount - spendingAmount;
            return amountUtilized;
        }

        public async Task<decimal> CalculateAmountOwedByBranchAsyncS(int CustomerId, int BranchId)
        {
            var sql = @"SELECT COALESCE(SUM(pi.Remainder), 0) AS TotalOwed
                    FROM SalesInvoices pi
                    WHERE pi.CustomerId = @CustomerId AND pi.BranchId = @BranchId AND pi.IsFullPaidUp = 0";
            using (var db = _dapperContext.CreateConnection())
            {
                var totalOwed = await db.QueryAsync<decimal>(sql, new { CustomerId, BranchId })
                                        .ConfigureAwait(false);
                return totalOwed.FirstOrDefault();
            }
        }
        public async Task<IEnumerable<int>> GetNotFullyPaidInvoiceIdsAsyncS(int branchId, int customerId)
        {
            var sql = @"
                SELECT SalesInvoiceId
                FROM SalesInvoices
                WHERE BranchId = @BranchId AND CustomerId = @CustomerId AND IsFullPaidUp = 0";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<int>(sql, new { BranchId = branchId, CustomerId = customerId }).ConfigureAwait(false);
            }
        }
        public async Task<SalesInvoicesModel> GetSalesInvoiceByIdAsyncS(int SalesInvoiceId)
        {
            var sql = @"
                SELECT si.SalesInvoiceId, si.TotalAmount, si.PaidUp, si.Remainder, si.SaleDate, si.TotalDiscount,
                       si.CustomerId, c.CustomerName, si.BranchId, b.BranchName,si.PaymentMethodId, pm.PaymentMethodName, si.UserId, u.UserName, si.PaidUpDate, si.IsFullPaidUp, si.Notes
                FROM SalesInvoices si
                LEFT JOIN Customers c ON si.CustomerId = c.CustomerId
                LEFT JOIN Branches b ON si.BranchId = b.BranchId
                LEFT JOIN PaymentMethods pm ON si.PaymentMethodId = pm.PaymentMethodId
                LEFT JOIN Users u ON si.UserId = u.UserId
                WHERE si.SalesInvoiceId = @SalesInvoiceId AND si.IsReturn = 0 ";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<SalesInvoicesModel>(sql, new { SalesInvoiceId }).ConfigureAwait(false);
            }
        }
        public async Task<int> UpdateSalesInvoiceAsyncS(int SalesInvoiceId, decimal PaidUp, decimal Remainder, bool IsFullPaidUp)
        {
            var sql = @"UPDATE SalesInvoices SET PaidUp = @PaidUp, Remainder = @Remainder, IsFullPaidUp = @IsFullPaidUp
                        WHERE SalesInvoiceId = @SalesInvoiceId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { SalesInvoiceId, PaidUp, Remainder, IsFullPaidUp }).ConfigureAwait(false);
            }
        }
        public async Task RecordPaymentTransaction(PurchaseInvoicesModel model, int invoiceId)
        {
            var paymentTransaction = new PaymentTransactionModel
            {
                BranchId = model.BranchId,
                PaymentMethodId = model.PaymentMethodId,
                Amount = -model.PaidUp,
                TransactionType = "خصم",
                TransactionDate = model.PurchaseDate,
                ModifiedUser = model.UserId,
                ModifiedDate = DateTime.Now,
                InvoiceId = invoiceId,
                Details = model.Notes,
            };
            await InsertPaymentTransactionAsync(paymentTransaction);
        }
        public async Task RecordPaymentTransaction(SalesInvoicesModel model, int invoiceId)
        {
            var paymentTransaction = new PaymentTransactionModel
            {
                BranchId = model.BranchId,
                PaymentMethodId = model.PaymentMethodId,
                Amount = model.PaidUp,
                TransactionType = "اضافة",
                TransactionDate = model.SaleDate,
                ModifiedUser = model.UserId,
                ModifiedDate = DateTime.Now,
                InvoiceId = invoiceId,
                Details = model.Notes,
            };
            await InsertPaymentTransactionAsync(paymentTransaction);
        }
    }
}
