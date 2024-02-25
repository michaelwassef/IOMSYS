using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class InventoryMovementService : IInventoryMovementService
    {
        private readonly DapperContext _dapperContext;
        private readonly IBranchInventoryService _branchInventoryService;

        public InventoryMovementService(DapperContext dapperContext, IBranchInventoryService branchInventoryService)
        {
            _dapperContext = dapperContext;
            _branchInventoryService = branchInventoryService;
        }

        public async Task<InventoryMovementModel?> SelectInventoryMovementByToBranchIdAsync(int ToBranchId)
        {
            var sql = @"SELECT * FROM InventoryMovements WHERE ToBranchId = @ToBranchId AND IsApproved = 'false' ";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<InventoryMovementModel>(sql, new { ToBranchId }).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<InventoryMovementModel>> SelectHangingWarehouseByToBranchIdAsync(int? ToBranchId)
        {
            var sql = @"SELECT I.MovementId, p.ProductName, s.SizeName, c.ColorName, I.Quantity,
                b1.BranchName AS FromBranchName, b2.BranchName AS ToBranchName, I.IsApproved,
                I.MovementDate, I.Notes 
                FROM InventoryMovements I
                LEFT JOIN Products p ON I.ProductId = p.ProductId 
                LEFT JOIN Sizes s ON I.SizeId = s.SizeId
                LEFT JOIN Colors c ON I.ColorId = c.ColorId
                LEFT JOIN Branches b1 ON I.FromBranchId = b1.BranchId
                LEFT JOIN Branches b2 ON I.ToBranchId = b2.BranchId
                WHERE I.ToBranchId = @ToBranchId AND IsApproved = 'false'";
            using (var db = _dapperContext.CreateConnection())
            {
                var results = await db.QueryAsync<InventoryMovementModel>(sql, new { ToBranchId }).ConfigureAwait(false);
                return results;
            }
        }


        public async Task<int> MoveInventoryAsync(InventoryMovementModel movement)
        {
            using (var db = _dapperContext.CreateConnection())
            {
                try
                {
                    var movementId = await db.ExecuteScalarAsync<int>(
                        @"INSERT INTO InventoryMovements (ProductId, SizeId, ColorId, Quantity, FromBranchId, ToBranchId, Notes, IsApproved, MovementDate) 
                        VALUES (@ProductId, @SizeId, @ColorId, @Quantity, @FromBranchId, @ToBranchId, @Notes, @IsApproved, @MovementDate);
                        SELECT CAST(SCOPE_IDENTITY() as int);",
                        movement).ConfigureAwait(false);

                    await _branchInventoryService.AdjustInventoryQuantityAsync(
                        movement.ProductId, movement.SizeId, movement.ColorId, movement.FromBranchId, -movement.Quantity);

                    return movementId;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during inventory movement: {ex.Message}");
                    return -1;
                }
            }
        }
        public async Task<int> DeleteMovementAsync(int movementId)
        {
            var sql = @"DELETE FROM InventoryMovements WHERE MovementId = @movementId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { movementId = movementId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
        public async Task<bool> ApproveOrRejectInventoryMovementAsync(int movementId, bool isApproved)
        {
            var sql = @"UPDATE InventoryMovements SET IsApproved = @IsApproved WHERE MovementId = @MovementId";

            using (var db = _dapperContext.CreateConnection())
            {
                var result = await db.ExecuteAsync(sql, new { MovementId = movementId, IsApproved = isApproved }).ConfigureAwait(false);
                if (result > 0)
                {
                    if (isApproved)
                    {
                        var movement = await SelectInventoryMovementByIdAsync(movementId);
                        if (movement != null)
                        {
                            await _branchInventoryService.AdjustInventoryQuantityAsync(
                                movement.ProductId, movement.SizeId, movement.ColorId, movement.ToBranchId, movement.Quantity);
                        }
                    }
                    else
                    {
                        var movement = await SelectInventoryMovementByIdAsync(movementId);
                        await _branchInventoryService.AdjustInventoryQuantityAsync(
                                movement.ProductId, movement.SizeId, movement.ColorId, movement.FromBranchId, movement.Quantity);
                        await DeleteMovementAsync(movementId);
                    }
                    return true;
                }
                return false;
            }
        }

        public async Task<InventoryMovementModel?> SelectInventoryMovementByIdAsync(int movementId)
        {
            var sql = @"SELECT * FROM InventoryMovements WHERE MovementId = @MovementId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<InventoryMovementModel>(sql, new { MovementId = movementId }).ConfigureAwait(false);
            }
        }

    }
}
