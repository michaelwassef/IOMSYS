namespace IOMSYS.Models
{
    public class OfferModel
    {
        public int OfferId { get; set; }
        public string OfferName { get; set; }
        public string OfferType { get; set; } // Examples: 'BOGO', 'Bundle', 'Discount', etc.
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Details { get; set; } // Assuming storage of JSON or other structured format
        public bool IsActive { get; set; }
    }
}
