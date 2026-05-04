using Latidos.Models;

namespace Latidos.Services;

/// <summary>
/// Mock Payment Service para testing y desarrollo.
/// Simula respuestas de Stripe sin hacer llamadas reales.
/// </summary>
public class MockPaymentService : IPaymentService
{
    private static readonly Random _random = new();
    private static int _transactionCounter = 1000;

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        await Task.Delay(_random.Next(500, 2000));

        var cardNumber = request.TokenId.Replace(" ", "");

        var response = new PaymentResponse
        {
            Amount = request.Amount,
            ProcessedAt = DateTime.UtcNow
        };

        if (cardNumber == "4242424242424242")
        {
            response.Success = true;
            response.TransactionId = $"ch_mock_{_transactionCounter++}_{Guid.NewGuid().ToString()[..8]}";
            response.Message = "Pago procesado correctamente (MOCK)";
        }
        else if (cardNumber == "4000000000000002")
        {
            response.Success = false;
            response.TransactionId = string.Empty;
            response.Message = "La tarjeta fue rechazada (MOCK - tarjeta de prueba)";
        }
        else if (cardNumber == "4000000000000069")
        {
            response.Success = false;
            response.TransactionId = string.Empty;
            response.Message = "La tarjeta esta vencida (MOCK - tarjeta de prueba)";
        }
        else if (cardNumber == "4000002500003155")
        {
            response.Success = false;
            response.TransactionId = string.Empty;
            response.Message = "Esta tarjeta requiere autenticacion (MOCK - tarjeta de prueba)";
        }
        else if (cardNumber.Length >= 13 && cardNumber.All(char.IsDigit))
        {
            response.Success = true;
            response.TransactionId = $"ch_mock_{_transactionCounter++}_{Guid.NewGuid().ToString()[..8]}";
            response.Message = "Pago procesado correctamente (MOCK)";
        }
        else
        {
            response.Success = false;
            response.TransactionId = string.Empty;
            response.Message = "Numero de tarjeta invalido (MOCK)";
        }

        return response;
    }

    public async Task<bool> RefundAsync(string transactionId, decimal amount)
    {
        await Task.Delay(_random.Next(500, 1500));
        return true;
    }
}
