using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IPaymentMethodsService
    {
        Task<IEnumerable<PaymentMethodsModel>> GetAllPaymentMethodsAsync();
        Task<PaymentMethodsModel?> SelectPaymentMethodByIdAsync(int paymentMethodId);
        Task<int> InsertPaymentMethodAsync(PaymentMethodsModel paymentMethod);
        Task<int> UpdatePaymentMethodAsync(PaymentMethodsModel paymentMethod);
        Task<int> DeletePaymentMethodAsync(int paymentMethodId);
    }
}
