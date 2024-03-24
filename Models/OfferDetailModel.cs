namespace IOMSYS.Models
{
    public class OfferDetailModel
    {
        public int OfferDetailId { get; set; }
        public int OfferId { get; set; }
        public int ProductId { get; set; }
        public decimal? RequiredQuantity { get; set; } 
        public decimal? FreeQuantity { get; set; } 
        public decimal? DiscountedPrice { get; set; }
        public string? OfferType { get; set; }
    }
}
