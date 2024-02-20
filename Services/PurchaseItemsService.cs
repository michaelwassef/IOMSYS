using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class PurchaseItemsService : IPurchaseItemsService
    {
        private readonly DapperContext _dapperContext;

        public PurchaseItemsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<PurchaseItemsModel>> GetAllPurchaseItemsAsync()
        {
            var sql = @"
                SELECT pi.PurchaseItemId, p.ProductName, s.SizeName, c.ColorName, pi.Quantity, pi.BuyPrice,pi.Notes,pi.BranchId,pi.Statues
                FROM PurchaseItems pi
                LEFT JOIN Products p ON pi.ProductId = p.ProductId
                LEFT JOIN Sizes s ON pi.SizeId = s.SizeId
                LEFT JOIN Colors c ON pi.ColorId = c.ColorId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PurchaseItemsModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<PurchaseItemsModel> GetPurchaseItemByIdAsync(int purchaseItemId)
        {
            var sql = @"
                SELECT 
                    pi.PurchaseItemId, 
                    p.ProductId,
                    p.ProductName,
                    s.SizeId,
                    s.SizeName,
                    c.ColorId,
                    c.ColorName, 
                    pi.Quantity, 
                    pi.BuyPrice,
                    pii.PurchaseInvoiceId,
                    pi.Notes,
                    pi.BranchId,
                    pi.Statues
                FROM 
                    PurchaseItems pi
                    LEFT JOIN Products p ON pi.ProductId = p.ProductId
                    LEFT JOIN Sizes s ON pi.SizeId = s.SizeId
                    LEFT JOIN Colors c ON pi.ColorId = c.ColorId
                    INNER JOIN PurchaseInvoiceItems pii ON pi.PurchaseItemId = pii.PurchaseItemId
                WHERE 
                    pi.PurchaseItemId = @PurchaseItemId;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<PurchaseItemsModel>(sql, new { PurchaseItemId = purchaseItemId }).ConfigureAwait(false);
            }
        }
        public async Task<IEnumerable<PurchaseItemsModel>> GetPurchaseItemsByInvoiceIdAsync(int InvoiceId)
        {
            var sql = @"
                SELECT pi.PurchaseItemId, pi.ProductId, pi.SizeId, pi.ColorId, pi.Quantity, pi.BuyPrice, 
                       p.ProductName, s.SizeName, c.ColorName, pi.Notes, pi.BranchId, pi.Statues
                FROM PurchaseInvoiceItems pii
                INNER JOIN PurchaseItems pi ON pii.PurchaseItemId = pi.PurchaseItemId
                LEFT JOIN Products p ON pi.ProductId = p.ProductId
                LEFT JOIN Sizes s ON pi.SizeId = s.SizeId
                LEFT JOIN Colors c ON pi.ColorId = c.ColorId
                WHERE pii.PurchaseInvoiceId = @PurchaseInvoiceId;
            ";

            using (var db = _dapperContext.CreateConnection())
            {
                var items = await db.QueryAsync<PurchaseItemsModel>(sql, new { PurchaseInvoiceId = InvoiceId }).ConfigureAwait(false);
                return items;
            }
        }

        public async Task<int> InsertPurchaseItemAsync(PurchaseItemsModel purchaseItem)
        {
            var sql = @"INSERT INTO PurchaseItems (ProductId, SizeId, ColorId, Quantity, BuyPrice, Notes, BranchId, Statues) 
                VALUES (@ProductId, @SizeId, @ColorId, @Quantity, @BuyPrice, @Notes, @BranchId, @Statues);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteScalarAsync<int>(sql, purchaseItem).ConfigureAwait(false);
            }
        }

        public async Task<int> UpdatePurchaseItemAsync(PurchaseItemsModel purchaseItem)
        {
            var sql = @"UPDATE PurchaseItems 
                SET ProductId = @ProductId, SizeId = @SizeId, ColorId = @ColorId, Quantity = @Quantity, BuyPrice = @BuyPrice ,
               Notes = @Notes , BranchId = @BranchId, Statues = @Statues
                WHERE PurchaseItemId = @PurchaseItemId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, purchaseItem).ConfigureAwait(false);
            }
        }

        public async Task<int> DeletePurchaseItemAsync(int purchaseItemId)
        {
            var sql = @"DELETE FROM PurchaseItems WHERE PurchaseItemId = @PurchaseItemId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { PurchaseItemId = purchaseItemId }).ConfigureAwait(false);
            }
        }
    }
}
