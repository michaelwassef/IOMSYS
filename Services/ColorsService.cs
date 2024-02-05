using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class ColorsService : IColorsService
    {
        private readonly DapperContext _dapperContext;

        public ColorsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<ColorsModel>> GetAllColorsAsync()
        {
            var sql = @"SELECT * FROM Colors";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ColorsModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<ColorsModel?> SelectColorByIdAsync(int colorId)
        {
            var sql = @"SELECT * FROM Colors WHERE ColorId = @ColorId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<ColorsModel>(sql, new { ColorId = colorId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertColorAsync(ColorsModel color)
        {
            var sql = @"INSERT INTO Colors (ColorName) VALUES (@ColorName);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { color.ColorName }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateColorAsync(ColorsModel color)
        {
            var sql = @"UPDATE Colors SET ColorName = @ColorName WHERE ColorId = @ColorId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { color.ColorName, color.ColorId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteColorAsync(int colorId)
        {
            var sql = @"DELETE FROM Colors WHERE ColorId = @ColorId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { ColorId = colorId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
