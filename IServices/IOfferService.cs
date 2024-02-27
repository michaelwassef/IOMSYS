using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IOfferService
    {
        Task<IEnumerable<OfferModel>> GetAllOffersAsync();
        Task<OfferModel> GetOfferByIdAsync(int offerId);
        Task<int> CreateOfferAsync(OfferModel offer);
        Task<bool> UpdateOfferAsync(OfferModel offer);
        Task<bool> DeleteOfferAsync(int offerId);
        Task<IEnumerable<OfferDetailModel>> GetAllOfferDetailsAsync();
        Task<OfferDetailModel> GetOfferDetailByIdAsync(int offerDetailId);
        Task<int> CreateOfferDetailAsync(OfferDetailModel offerDetail);
        Task<bool> UpdateOfferDetailAsync(OfferDetailModel offerDetail);
        Task<bool> DeleteOfferDetailAsync(int offerDetailId);
        Task<IEnumerable<ProductOfferModel>> GetAllProductOffersAsync();
        Task<bool> CreateProductOfferAsync(ProductOfferModel productOffer);
        Task<bool> DeleteProductOfferAsync(int productId, int offerId);
    }
}
