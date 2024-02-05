using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class PurchaseInvoiceItemsService : IPurchaseInvoiceItemsService
    {
        private readonly DapperContext _dapperContext;

        public PurchaseInvoiceItemsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        // Method to add an item to a purchase invoice
        public async Task<int> AddItemToPurchaseInvoiceAsync(PurchaseInvoiceItemsModel purchaseInvoiceItem)
        {
            var sql = @"INSERT INTO PurchaseInvoiceItems (PurchaseInvoiceId, PurchaseItemId) 
                        VALUES (@PurchaseInvoiceId, @PurchaseItemId)";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, purchaseInvoiceItem).ConfigureAwait(false);
            }
        }

        // Method to remove an item from a purchase invoice
        public async Task<int> RemoveItemFromPurchaseInvoiceAsync(PurchaseInvoiceItemsModel purchaseInvoiceItem)
        {
            var sql = @"DELETE FROM PurchaseInvoiceItems 
                        WHERE PurchaseInvoiceId = @PurchaseInvoiceId AND PurchaseItemId = @PurchaseItemId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { PurchaseInvoiceId = purchaseInvoiceItem.PurchaseInvoiceId, PurchaseItemId = purchaseInvoiceItem.PurchaseItemId }).ConfigureAwait(false);
            }
        }
    }
}
