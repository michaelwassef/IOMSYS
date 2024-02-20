namespace IOMSYS.Models
{
    public class InventoryMovementModel
    {
        public int MovementId { get; set; }
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public int ColorId { get; set; }
        public int Quantity { get; set; }
        public int FromBranchId { get; set; }
        public int ToBranchId { get; set; }
        public bool IsApproved { get; set; } 
        public string Notes { get; set; }
        public DateTime MovementDate { get; set; }
    }
}
