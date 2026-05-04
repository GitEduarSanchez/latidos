using Latidos.Models;

namespace Latidos.Services;

public interface IInvoiceService
{
    Task<string> GenerateOrderTicketPdfAsync(Order order);
}
