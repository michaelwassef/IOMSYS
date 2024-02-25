using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IInventoryMovementService
    {
        Task<InventoryMovementModel?> SelectInventoryMovementByToBranchIdAsync(int ToBranchId);
        Task<IEnumerable<InventoryMovementModel>> SelectHangingWarehouseByToBranchIdAsync(int? ToBranchId);
        Task<int> MoveInventoryAsync(InventoryMovementModel movement);
        Task<bool> ApproveOrRejectInventoryMovementAsync(int movementId, bool isApproved);
        Task<InventoryMovementModel?> SelectInventoryMovementByIdAsync(int movementId);
        Task<int> DeleteMovementAsync(int movementId);
    }
}
