﻿using Dapper;
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
                SELECT si.SalesItemId, p.ProductName, s.SizeName, c.ColorName, si.Quantity, si.SellPrice
                FROM SalesItems si
                LEFT JOIN Products p ON si.ProductId = p.ProductId
                LEFT JOIN Sizes s ON si.SizeId = s.SizeId
                LEFT JOIN Colors c ON si.ColorId = c.ColorId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<SalesItemsModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<SalesItemsModel> GetSalesItemByIdAsync(int salesItemId)
        {
            var sql = @"
                SELECT si.SalesItemId, p.ProductName, s.SizeName, c.ColorName, si.Quantity, si.SellPrice
                FROM SalesItems si
                LEFT JOIN Products p ON si.ProductId = p.ProductId
                LEFT JOIN Sizes s ON si.SizeId = s.SizeId
                LEFT JOIN Colors c ON si.ColorId = c.ColorId
                WHERE si.SalesItemId = @SalesItemId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<SalesItemsModel>(sql, new { SalesItemId = salesItemId }).ConfigureAwait(false);
            }
        }
        public async Task<int> InsertSalesItemAsync(SalesItemsModel salesItem)
        {
            var sql = @"
                INSERT INTO SalesItems (ProductId, SizeId, ColorId, Quantity, SellPrice) 
                VALUES (@ProductId, @SizeId, @ColorId, @Quantity, @SellPrice);
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
                SET ProductId = @ProductId, SizeId = @SizeId, ColorId = @ColorId, Quantity = @Quantity, SellPrice = @SellPrice 
                WHERE SalesItemId = @SalesItemId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, salesItem).ConfigureAwait(false);
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