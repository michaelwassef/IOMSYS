using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class UnitsService : IUnitsService
    {
        private readonly DapperContext _dapperContext;

        public UnitsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<UnitsModel>> GetAllUnitsAsync()
        {
            var sql = @"SELECT * FROM Units";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<UnitsModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<UnitsModel?> SelectUnitByIdAsync(int UnitId)
        {
            var sql = @"SELECT * FROM Units WHERE UnitId = @UnitId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<UnitsModel>(sql, new { UnitId = UnitId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertUnitAsync(UnitsModel Unit)
        {
            var sql = @"INSERT INTO Units (UnitName) VALUES (@UnitName);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { Unit.UnitName }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateUnitAsync(UnitsModel Unit)
        {
            var sql = @"UPDATE Units SET UnitName = @UnitName WHERE UnitId = @UnitId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { Unit.UnitName, Unit.UnitId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteUnitAsync(int UnitId)
        {
            var sql = @"DELETE FROM Units WHERE UnitId = @UnitId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { UnitId = UnitId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
