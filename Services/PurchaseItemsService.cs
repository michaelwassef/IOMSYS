﻿using Dapper;
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
                SELECT pi.PurchaseItemId, p.ProductName, s.SizeName, c.ColorName, pi.Quantity, pi.BuyPrice
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
                SELECT pi.PurchaseItemId, p.ProductName, s.SizeName, c.ColorName, pi.Quantity, pi.BuyPrice
                FROM PurchaseItems pi
                LEFT JOIN Products p ON pi.ProductId = p.ProductId
                LEFT JOIN Sizes s ON pi.SizeId = s.SizeId
                LEFT JOIN Colors c ON pi.ColorId = c.ColorId
                WHERE pi.PurchaseItemId = @PurchaseItemId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<PurchaseItemsModel>(sql, new { PurchaseItemId = purchaseItemId }).ConfigureAwait(false);
            }
        }
        public async Task<int> InsertPurchaseItemAsync(PurchaseItemsModel purchaseItem)
        {
            var sql = @"INSERT INTO PurchaseItems (ProductId, SizeId, ColorId, Quantity, BuyPrice) 
                VALUES (@ProductId, @SizeId, @ColorId, @Quantity, @BuyPrice);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteScalarAsync<int>(sql, purchaseItem).ConfigureAwait(false);
            }
        }

        public async Task<int> UpdatePurchaseItemAsync(PurchaseItemsModel purchaseItem)
        {
            var sql = @"UPDATE PurchaseItems 
                SET ProductId = @ProductId, SizeId = @SizeId, ColorId = @ColorId, Quantity = @Quantity, BuyPrice = @BuyPrice 
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