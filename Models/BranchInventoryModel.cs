namespace IOMSYS.Models
{
    public class BranchInventoryModel
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public int ColorId { get; set; }
        public int BranchId { get; set; }
        public int AvailableQty { get; set; }
    }
}
