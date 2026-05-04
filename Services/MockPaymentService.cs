using Latidos.Models;

namespace Latidos.Services;

/// <summary>
/// Mock Payment Service para testing y desarrollo
/// Simula respuestas de Stripe sin hacer llamadas reales
/// </summary>
public class MockPaymentService : IPaymentService
{
    private static Random _random = new Random();
    private static int _transactionCounter = 1000;

    public MockPaymentService()
    {
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        // Simular latencia de red
        await Task.Delay(_random.Next(500, 2000));

        // Validar tarjeta de prueba
        var cardNumber = request.TokenId.Replace(" ", "");

        // Tarjetas de prueba de Stripe
        var response = new PaymentResponse
        {
            Amount = request.Amount,
            ProcessedAt = DateTime.UtcNow
        };

        // Simular diferentes resultados basados en tarjeta
        if (cardNumber == "4242424242424242")
        {
            // Exitosa
            response.Success = true;
            response.TransactionId = $"ch_mock_{_transactionCounter++}_{Guid.NewGuid().ToString().Substring(0, 8)}";
            response.Message = "✓ Payment processed successfully (MOCK)";
        }
        else if (cardNumber == "4000000000000002")
        {
            // Declinada
            response.Success = false;
            response.TransactionId = string.Empty;
            response.Message = "Your card was declined (MOCK - Test Card)";
        }
        else if (cardNumber == "4000000000000069")
        {
            // Expirada
            response.Success = false;
            response.TransactionId = string.Empty;
            response.Message = "Your card has expired (MOCK - Test Card)";
        }
        else if (cardNumber == "4000002500003155")
        {
            // Requiere autenticación
            response.Success = false;
            response.TransactionId = string.Empty;
            response.Message = "This card requires authentication (MOCK - Test Card)";
        }
        else if (cardNumber.Length >= 13 && cardNumber.All(char.IsDigit))
        {
            // Cualquier otra tarjeta válida se acepta en mock
            response.Success = true;
            response.TransactionId = $"ch_mock_{_transactionCounter++}_{Guid.NewGuid().ToString().Substring(0, 8)}";
            response.Message = "✓ Payment processed successfully (MOCK)";
        }
        else
        {
            // Tarjeta inválida
            response.Success = false;
            response.TransactionId = string.Empty;
            response.Message = "Invalid card number (MOCK)";
        }

        return response;
    }

    public async Task<bool> RefundAsync(string transactionId, decimal amount)
    {
        // Simular latencia
        await Task.Delay(_random.Next(500, 1500));

        // En mock, todos los reembolsos son exitosos
        return true;
    }
}
