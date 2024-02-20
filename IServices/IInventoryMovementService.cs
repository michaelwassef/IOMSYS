using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IInventoryMovementService
    {
        Task<InventoryMovementModel?> SelectInventoryMovementByToBranchIdAsync(int ToBranchId);
        Task<int> MoveInventoryAsync(InventoryMovementModel movement);
    }
}
