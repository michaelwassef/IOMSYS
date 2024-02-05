using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class CategoriesService : ICategoriesService
    {
        private readonly DapperContext _dapperContext;

        public CategoriesService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<CategoriesModel>> GetAllCategoriesAsync()
        {
            var sql = @"SELECT * FROM Categories";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<CategoriesModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<CategoriesModel?> SelectCategoryByIdAsync(int categoryId)
        {
            var sql = @"SELECT * FROM Categories WHERE CategoryId = @CategoryId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<CategoriesModel>(sql, new { CategoryId = categoryId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertCategoryAsync(CategoriesModel category)
        {
            var sql = @"INSERT INTO Categories (CategoryName) VALUES (@CategoryName);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { category.CategoryName }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateCategoryAsync(CategoriesModel category)
        {
            var sql = @"UPDATE Categories SET CategoryName = @CategoryName WHERE CategoryId = @CategoryId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { category.CategoryName, category.CategoryId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteCategoryAsync(int categoryId)
        {
            var sql = @"DELETE FROM Categories WHERE CategoryId = @CategoryId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { CategoryId = categoryId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
