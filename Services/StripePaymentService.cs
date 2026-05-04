using Latidos.Models;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Latidos.Services;

public class StripePaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly string _stripeApiKey;
    private readonly string _stripeApiUrl = "https://api.stripe.com/v1";

    public StripePaymentService(HttpClient httpClient, IConfiguration? configuration = null)
    {
        _httpClient = httpClient;
        _stripeApiKey = configuration?["Stripe:SecretKey"] ?? Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? "sk_test_placeholder";
        SetupHttpClient();
    }

    private void SetupHttpClient()
    {
        var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_stripeApiKey}:"));
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("amount", ((int)(request.Amount * 100)).ToString()),
                new KeyValuePair<string, string>("currency", request.Currency.ToLower()),
                new KeyValuePair<string, string>("source", request.TokenId),
                new KeyValuePair<string, string>("description", request.Description),
                new KeyValuePair<string, string>("receipt_email", request.CustomerEmail),
                new KeyValuePair<string, string>("metadata[customer_name]", request.CustomerName),
            });

            var response = await _httpClient.PostAsync($"{_stripeApiUrl}/charges", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var charge = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                return new PaymentResponse
                {
                    Success = true,
                    TransactionId = charge.GetProperty("id").GetString() ?? string.Empty,
                    Message = "Payment processed successfully",
                    Amount = request.Amount,
                    ProcessedAt = DateTime.UtcNow
                };
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                return new PaymentResponse
                {
                    Success = false,
                    Message = $"Payment failed: {errorResponse}",
                    Amount = request.Amount,
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            return new PaymentResponse
            {
                Success = false,
                Message = $"Error processing payment: {ex.Message}",
                Amount = request.Amount,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<bool> RefundAsync(string transactionId, decimal amount)
    {
        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("charge", transactionId),
                new KeyValuePair<string, string>("amount", ((int)(amount * 100)).ToString()),
            });

            var response = await _httpClient.PostAsync($"{_stripeApiUrl}/refunds", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
