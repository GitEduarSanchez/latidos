using Latidos.Models;

namespace Latidos.Services;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
    Task<bool> RefundAsync(string transactionId, decimal amount);
}
