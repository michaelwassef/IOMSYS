﻿namespace IOMSYS.Models
{
    public class PurchaseItemsModel
    {
        public int PurchaseItemId { get; set; }
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public int ColorId { get; set; }
        public int Quantity { get; set; }
        public decimal BuyPrice { get; set; }
        public string ProductName { get; set; }
        public string SizeName { get; set; }
        public string ColorName { get; set; }
    }
}