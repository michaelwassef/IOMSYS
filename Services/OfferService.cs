using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;
using Dapper;

namespace IOMSYS.Services
{
    public class OfferService : IOfferService
    {
        private readonly DapperContext _dapperContext;

        public OfferService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        // CRUD Operations for Offers
        public async Task<IEnumerable<OfferModel>> GetAllOffersAsync()
        {
            const string sql = "SELECT * FROM Offers";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<OfferModel>(sql);
            }
        }

        public async Task<IEnumerable<OfferModel>> GetAllActiveOffersAsync()
        {
            const string sql = "SELECT * FROM Offers where IsActive '1'";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<OfferModel>(sql);
            }
        }

        public async Task<OfferModel> GetOfferByIdAsync(int offerId)
        {
            const string sql = "SELECT * FROM Offers WHERE OfferId = @OfferId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<OfferModel>(sql, new { OfferId = offerId });
            }
        }

        public async Task<int> CreateOfferAsync(OfferModel offer)
        {
            const string sql = @"INSERT INTO Offers (OfferName, OfferType, StartDate, EndDate, Details, IsActive) 
                                 VALUES (@OfferName, @OfferType, @StartDate, @EndDate, @Details, @IsActive);
                                 SELECT CAST(SCOPE_IDENTITY() as int);";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteScalarAsync<int>(sql, offer);
            }
        }

        public async Task<bool> UpdateOfferAsync(OfferModel offer)
        {
            const string sql = @"UPDATE Offers 
                                 SET OfferName = @OfferName, OfferType = @OfferType, StartDate = @StartDate, EndDate = @EndDate, Details = @Details, IsActive = @IsActive
                                 WHERE OfferId = @OfferId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, offer) > 0;
            }
        }

        public async Task<bool> DeleteOfferAsync(int offerId)
        {
            const string sql = "DELETE FROM Offers WHERE OfferId = @OfferId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { OfferId = offerId }) > 0;
            }
        }

        // CRUD Operations for OfferDetails
        public async Task<IEnumerable<OfferDetailModel>> GetAllOfferDetailsAsync()
        {
            const string sql = "SELECT * FROM OfferDetails";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<OfferDetailModel>(sql);
            }
        }

        public async Task<OfferDetailModel> GetOfferDetailByIdAsync(int offerDetailId)
        {
            const string sql = "SELECT * FROM OfferDetails WHERE OfferDetailId = @OfferDetailId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<OfferDetailModel>(sql, new { OfferDetailId = offerDetailId });
            }
        }

        public async Task<OfferDetailModel> GetOfferDetailByOfferIdAsync(int OfferId)
        {
            const string sql = "SELECT * FROM OfferDetails WHERE OfferId = @OfferId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<OfferDetailModel>(sql, new { OfferId });
            }
        }

        public async Task<int> CreateOfferDetailAsync(OfferDetailModel offerDetail)
        {
            const string sql = @"INSERT INTO OfferDetails (OfferId, ProductId, RequiredQuantity, FreeQuantity, DiscountedPrice) 
                                 VALUES (@OfferId, @ProductId, @RequiredQuantity, @FreeQuantity, @DiscountedPrice);
                                 SELECT CAST(SCOPE_IDENTITY() as int);";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteScalarAsync<int>(sql, offerDetail);
            }
        }

        public async Task<bool> UpdateOfferDetailAsync(OfferDetailModel offerDetail)
        {
            const string sql = @"UPDATE OfferDetails 
                                 SET OfferId = @OfferId, ProductId = @ProductId, RequiredQuantity = @RequiredQuantity, FreeQuantity = @FreeQuantity, DiscountedPrice = @DiscountedPrice
                                 WHERE OfferDetailId = @OfferDetailId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, offerDetail) > 0;
            }
        }

        public async Task<bool> DeleteOfferDetailAsync(int offerDetailId)
        {
            const string sql = "DELETE FROM OfferDetails WHERE OfferDetailId = @OfferDetailId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { OfferDetailId = offerDetailId }) > 0;
            }
        }

        // CRUD Operations for ProductOffers
        public async Task<IEnumerable<ProductOfferModel>> GetAllProductOffersAsync()
        {
            const string sql = "SELECT * FROM ProductOffers";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductOfferModel>(sql);
            }
        }

        public async Task<bool> CreateProductOfferAsync(ProductOfferModel productOffer)
        {
            const string sql = @"INSERT INTO ProductOffers (ProductId, OfferId) VALUES (@ProductId, @OfferId)";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, productOffer) > 0;
            }
        }

        public async Task<bool> DeleteProductOfferAsync(int productId, int offerId)
        {
            const string sql = "DELETE FROM ProductOffers WHERE ProductId = @ProductId AND OfferId = @OfferId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.ExecuteAsync(sql, new { ProductId = productId, OfferId = offerId }) > 0;
            }
        }
    }
}
