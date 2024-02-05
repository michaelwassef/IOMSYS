using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class SizesService : ISizesService
    {
        private readonly DapperContext _dapperContext;

        public SizesService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<SizesModel>> GetAllSizesAsync()
        {
            var sql = @"SELECT * FROM Sizes";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<SizesModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<SizesModel?> SelectSizeByIdAsync(int sizeId)
        {
            var sql = @"SELECT * FROM Sizes WHERE SizeId = @SizeId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<SizesModel>(sql, new { SizeId = sizeId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertSizeAsync(SizesModel size)
        {
            var sql = @"INSERT INTO Sizes (SizeName) VALUES (@SizeName);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { size.SizeName }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateSizeAsync(SizesModel size)
        {
            var sql = @"UPDATE Sizes SET SizeName = @SizeName WHERE SizeId = @SizeId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { size.SizeName, size.SizeId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteSizeAsync(int sizeId)
        {
            var sql = @"DELETE FROM Sizes WHERE SizeId = @SizeId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { SizeId = sizeId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
