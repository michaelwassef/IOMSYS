using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class PaymentMethodsService : IPaymentMethodsService
    {
        private readonly DapperContext _dapperContext;

        public PaymentMethodsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<PaymentMethodsModel>> GetAllPaymentMethodsAsync()
        {
            var sql = @"SELECT * FROM PaymentMethods";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<PaymentMethodsModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<PaymentMethodsModel?> SelectPaymentMethodByIdAsync(int paymentMethodId)
        {
            var sql = @"SELECT * FROM PaymentMethods WHERE PaymentMethodId = @PaymentMethodId";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<PaymentMethodsModel>(sql, new { PaymentMethodId = paymentMethodId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertPaymentMethodAsync(PaymentMethodsModel paymentMethod)
        {
            var sql = @"INSERT INTO PaymentMethods (PaymentMethodName) VALUES (@PaymentMethodName);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteScalarAsync<int>(sql, new { paymentMethod.PaymentMethodName }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdatePaymentMethodAsync(PaymentMethodsModel paymentMethod)
        {
            var sql = @"UPDATE PaymentMethods SET PaymentMethodName = @PaymentMethodName WHERE PaymentMethodId = @PaymentMethodId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { paymentMethod.PaymentMethodName, paymentMethod.PaymentMethodId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeletePaymentMethodAsync(int paymentMethodId)
        {
            var sql = @"DELETE FROM PaymentMethods WHERE PaymentMethodId = @PaymentMethodId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { PaymentMethodId = paymentMethodId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
