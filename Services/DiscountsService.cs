using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class DiscountsService : IDiscountsService
    {
        private readonly DapperContext _dapperContext;

        public DiscountsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<DiscountsModel>> GetAllDiscountsAsync()
        {
            var sql = @"SELECT * FROM Discounts";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<DiscountsModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<DiscountsModel?> SelectDiscountByIdAsync(int discountId)
        {
            var sql = @"SELECT * FROM Discounts WHERE DiscountId = @DiscountId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<DiscountsModel>(sql, new { DiscountId = discountId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertDiscountAsync(DiscountsModel discount)
        {
            var sql = @"INSERT INTO Discounts (DiscountName, Percentage, FromDate, ToDate) 
                        VALUES (@DiscountName, @Percentage, @FromDate, @ToDate);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { discount.DiscountName, discount.Percentage, discount.FromDate, discount.ToDate }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateDiscountAsync(DiscountsModel discount)
        {
            var sql = @"UPDATE Discounts SET DiscountName = @DiscountName, Percentage = @Percentage, FromDate = @FromDate, ToDate = @ToDate WHERE DiscountId = @DiscountId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { discount.DiscountName, discount.Percentage, discount.FromDate, discount.ToDate, discount.DiscountId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteDiscountAsync(int discountId)
        {
            var sql = @"DELETE FROM Discounts WHERE DiscountId = @DiscountId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { DiscountId = discountId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
