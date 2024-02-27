using Dapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class SalesItemsService : ISalesItemsService
    {
        private readonly DapperContext _dapperContext;
        public SalesItemsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }
        public async Task<IEnumerable<SalesItemsModel>> GetAllSalesItemsAsync()
        {
            var sql = @"
                SELECT si.SalesItemId, p.ProductName, s.SizeName, c.ColorName, si.Quantity, si.BranchId,si.SellPrice, si.ItemDiscount, si.DiscountId
                FROM SalesItems si
                LEFT JOIN Products p ON si.ProductId = p.ProductId
                LEFT JOIN Sizes s ON si.SizeId = s.SizeId
                LEFT JOIN Colors c ON si.ColorId = c.ColorId
                where si.IsReturn=0";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<SalesItemsModel>(sql).ConfigureAwait(false);
            }
        }
        public async Task<IEnumerable<SalesItemsModel>> GetAllReturnSalesItemsAsync(int BranchId)
        {
            var sql = @"
                SELECT si.SalesItemId, p.ProductName, s.SizeName, c.ColorName, si.Quantity, si.BranchId, si.SellPrice, si.ItemDiscount, si.DiscountId, si.ReturnDate
                FROM SalesItems si
                LEFT JOIN Products p ON si.ProductId = p.ProductId
                LEFT JOIN Sizes s ON si.SizeId = s.SizeId
                LEFT JOIN Colors c ON si.ColorId = c.ColorId
                where si.IsReturn=1 AND BranchId = @BranchId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<SalesItemsModel>(sql, new { BranchId }).ConfigureAwait(false);
            }
        }
        public async Task<SalesItemsModel> GetSalesItemByIdAsync(int salesItemId)
        {
            var sql = @"
                SELECT sii.SalesInvoiceId, si.SalesItemId, p.ProductId, p.ProductName, s.SizeId ,s.SizeName, c.ColorId, c.ColorName, si.Quantity, si.BranchId,si.SellPrice, si.ItemDiscount, si.DiscountId
                FROM SalesItems si
                LEFT JOIN Products p ON si.ProductId = p.ProductId
                LEFT JOIN Sizes s ON si.SizeId = s.SizeId
                LEFT JOIN Colors c ON si.ColorId = c.ColorId
                INNER JOIN SalesInvoiceItems sii ON si.SalesItemId = sii.SalesItemId
                WHERE si.SalesItemId = @SalesItemId AND si.IsReturn=0";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<SalesItemsModel>(sql, new { SalesItemId = salesItemId }).ConfigureAwait(false);
            }
        }
        public async Task<IEnumerable<SalesItemsModel>> GetSaleItemsByInvoiceIdAsync(int SalesInvoiceId)
        {
            var sql = @"
                SELECT sii.SalesInvoiceId,si.SalesItemId, p.ProductId, p.ProductName, s.SizeId, s.SizeName, c.ColorId, c.ColorName, si.Quantity, si.BranchId, si.SellPrice, si.ItemDiscount, si.DiscountId
                FROM SalesInvoiceItems sii
                INNER JOIN SalesItems si ON sii.SalesItemId = si.SalesItemId
                LEFT JOIN Products p ON si.ProductId = p.ProductId
                LEFT JOIN Sizes s ON si.SizeId = s.SizeId
                LEFT JOIN Colors c ON si.ColorId = c.ColorId
                WHERE sii.SalesInvoiceId = @salesInvoiceId AND si.IsReturn=0";

            using (var db = _dapperContext.CreateConnection())
            {
                var items = await db.QueryAsync<SalesItemsModel>(sql, new { salesInvoiceId = SalesInvoiceId }).ConfigureAwait(false);
                return items;
            }
        }
        public async Task<int> InsertSalesItemAsync(SalesItemsModel salesItem)
        {
            var sql = @"
                INSERT INTO SalesItems (ProductId, SizeId, ColorId, Quantity, SellPrice, ItemDiscount, DiscountId, BranchId, IsReturn) 
                VALUES (@ProductId, @SizeId, @ColorId, @Quantity, @SellPrice, @ItemDiscount, @DiscountId, @BranchId, @IsReturn);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteScalarAsync<int>(sql, salesItem).ConfigureAwait(false);
            }
        }
        public async Task<int> UpdateSalesItemAsync(SalesItemsModel salesItem)
        {
            var sql = @"
                UPDATE SalesItems 
                SET ProductId = @ProductId, SizeId = @SizeId, ColorId = @ColorId, Quantity = @Quantity, SellPrice = @SellPrice ,
                ItemDiscount = @ItemDiscount, DiscountId = @DiscountId, BranchId = @BranchId, IsReturn = @IsReturn
                WHERE SalesItemId = @SalesItemId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, salesItem).ConfigureAwait(false);
            }
        }
        public async Task<int> ReturnSalesItemAsync(int salesItemId)
        {
            var sql = @"
                UPDATE SalesItems SET IsReturn = 1, ReturnDate = @ReturnDate WHERE SalesItemId = @salesItemId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql,new { salesItemId, ReturnDate=DateTime.Now }).ConfigureAwait(false);
            }
        }
        public async Task<int> DeleteSalesItemAsync(int salesItemId)
        {
            var sql = @"DELETE FROM SalesItems WHERE SalesItemId = @SalesItemId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { SalesItemId = salesItemId }).ConfigureAwait(false);
            }
        }

    }
}
