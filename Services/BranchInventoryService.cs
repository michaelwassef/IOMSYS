using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class BranchInventoryService : IBranchInventoryService
    {
        private readonly DapperContext _dapperContext;

        public BranchInventoryService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<BranchInventoryModel>> GetAllBranchInventoriesAsync()
        {
            var sql = @"SELECT * FROM BranchInventory";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<BranchInventoryModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<BranchInventoryModel?> GetInventoryByProductAndBranchAsync(int productId, int sizeId, int colorId, int branchId)
        {
            var sql = @"SELECT * FROM BranchInventory WHERE ProductId = @ProductId AND SizeId = @SizeId AND ColorId = @ColorId AND BranchId = @BranchId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<BranchInventoryModel>(sql, new { ProductId = productId, SizeId = sizeId, ColorId = colorId, BranchId = branchId }).ConfigureAwait(false);
            }
        }

        public async Task<BranchInventoryModel?> GetInventoryByBranchAsync(int branchId)
        {
            var sql = @"SELECT * FROM BranchInventory WHERE BranchId = @BranchId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<BranchInventoryModel>(sql, new { BranchId = branchId }).ConfigureAwait(false);
            }
        }

        public async Task<int> UpdateInventoryQuantityAsync(BranchInventoryModel inventory)
        {
            var sql = @"UPDATE BranchInventory SET AvailableQty = @AvailableQty WHERE InventoryId = @InventoryId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { inventory.AvailableQty, inventory.InventoryId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> AddOrUpdateInventoryAsync(BranchInventoryModel inventory)
        {
            var existingInventory = await GetInventoryByProductAndBranchAsync(inventory.ProductId, inventory.SizeId, inventory.ColorId, inventory.BranchId);
            if (existingInventory == null)
            {
                var insertSql = @"INSERT INTO BranchInventory (ProductId, SizeId, ColorId, BranchId, AvailableQty) VALUES (@ProductId, @SizeId, @ColorId, @BranchId, @AvailableQty);
                                  SELECT CAST(SCOPE_IDENTITY() as int);";
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(insertSql, inventory).ConfigureAwait(false);
                }
            }
            else
            {
                inventory.InventoryId = existingInventory.InventoryId;
                return await UpdateInventoryQuantityAsync(inventory);
            }
        }

        public async Task<int> AdjustInventoryQuantityAsync(int productId, int sizeId, int colorId, int branchId, decimal quantityAdjustment)
        {
            var inventory = await GetInventoryByProductAndBranchAsync(productId, sizeId, colorId, branchId);
            if (inventory != null)
            {
                inventory.AvailableQty += quantityAdjustment;
                if (inventory.AvailableQty < 0)
                {
                    return 0;
                }
                return await UpdateInventoryQuantityAsync(inventory);
            }
            else
            {
                // If no record exists, assume adding new inventory with the adjustment as the initial quantity
                var newInventory = new BranchInventoryModel
                {
                    ProductId = productId,
                    SizeId = sizeId,
                    ColorId = colorId,
                    BranchId = branchId,
                    AvailableQty = quantityAdjustment
                };
                return await AddOrUpdateInventoryAsync(newInventory);
            }
        }
    }
}
