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
            var sql = @"SELECT * FROM InventoryMovement WHERE ToBranchId = @ToBranchId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<InventoryMovementModel>(sql, new { ToBranchId }).ConfigureAwait(false);
            }
        }

        public async Task<int> MoveInventoryAsync(InventoryMovementModel movement)
        {
            using (var db = _dapperContext.CreateConnection())
            {
                try
                {
                    var movementId = await db.ExecuteScalarAsync<int>(
                        @"INSERT INTO InventoryMovement (ProductId, SizeId, ColorId, Quantity, FromBranchId, ToBranchId, Notes, IsApproved, MovementDate) 
                        VALUES (@ProductId, @SizeId, @ColorId, @Quantity, @FromBranchId, @ToBranchId, @Notes, @IsApproved, @MovementDate);
                        SELECT CAST(SCOPE_IDENTITY() as int);",
                        movement).ConfigureAwait(false);

                    await _branchInventoryService.AdjustInventoryQuantityAsync(
                        movement.ProductId, movement.SizeId, movement.ColorId, movement.FromBranchId, -movement.Quantity);

                    await _branchInventoryService.AdjustInventoryQuantityAsync(
                        movement.ProductId, movement.SizeId, movement.ColorId, movement.ToBranchId, movement.Quantity);

                    return movementId;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during inventory movement: {ex.Message}");
                    return -1;
                }
            }
        }

    }
}
