﻿namespace IOMSYS.Models
{
    public class InventoryMovementBatchModel
    {
        public List<InventoryMovementModel> Items { get; set; } = new List<InventoryMovementModel>();
        public bool makeInvoice { get; set; }
    }
}
