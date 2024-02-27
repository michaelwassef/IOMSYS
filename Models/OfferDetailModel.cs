namespace IOMSYS.Models
{
    public class OfferDetailModel
    {
        public int OfferDetailId { get; set; }
        public int OfferId { get; set; }
        public int ProductId { get; set; }
        public int? RequiredQuantity { get; set; } // Nullable for flexibility
        public int? FreeQuantity { get; set; } // Nullable for flexibility
        public decimal? DiscountedPrice { get; set; } // Nullable for flexibility
    }
}
