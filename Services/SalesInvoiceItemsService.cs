using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class SalesInvoiceItemsService : ISalesInvoiceItemsService
    {
        private readonly DapperContext _dapperContext;

        public SalesInvoiceItemsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }
        public async Task<int> SelectInvoiceConnectToItemAsync(int SalesItemId)
        {
            var sql = @"SELECT SalesInvoiceId FROM SalesInvoiceItems WHERE SalesItemId = @SalesItemId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryFirstOrDefaultAsync<int>(sql, new { SalesItemId }).ConfigureAwait(false);
            }
        }

        public async Task<int> AddSalesItemToInvoiceAsync(SalesInvoiceItemsModel salesInvoiceItem)
        {
            var sql = @"INSERT INTO SalesInvoiceItems (SalesInvoiceId, SalesItemId) 
                        VALUES (@SalesInvoiceId, @SalesItemId)";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, salesInvoiceItem).ConfigureAwait(false);
            }
        }

        public async Task<int> RemoveSalesItemFromInvoiceAsync(SalesInvoiceItemsModel salesInvoiceItem)
        {
            var sql = @"DELETE FROM SalesInvoiceItems 
                        WHERE SalesInvoiceId = @SalesInvoiceId AND SalesItemId = @SalesItemId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { SalesInvoiceId = salesInvoiceItem.SalesInvoiceId, SalesItemId = salesInvoiceItem.SalesItemId }).ConfigureAwait(false);
            }
        }
    }
}
