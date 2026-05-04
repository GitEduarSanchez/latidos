using Latidos.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QColors = QuestPDF.Helpers.Colors;

namespace Latidos.Services;

public class InvoiceService : IInvoiceService
{
    public Task<string> GenerateOrderTicketPdfAsync(Order order)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var folder = Path.Combine(FileSystem.AppDataDirectory, "facturas");
        Directory.CreateDirectory(folder);

        var filePath = Path.Combine(folder, $"{order.OrderNumber}.pdf");

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A5);
                page.Margin(18);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(col =>
                {
                    col.Spacing(6);
                    col.Item().Text("LATIDOS").Bold().FontSize(18);
                    col.Item().Text("Factura de inscripcion").Bold();
                    col.Item().Text($"Orden: {order.OrderNumber}");
                    col.Item().Text($"Fecha: {order.OrderDate:dd/MM/yyyy HH:mm}");
                    col.Item().Text($"Cliente: {order.CustomerName}");
                    col.Item().Text($"Correo: {order.CustomerEmail}");
                    col.Item().LineHorizontal(1).LineColor(QColors.Grey.Lighten1);

                    foreach (var item in order.Items)
                    {
                        col.Item().Column(row =>
                        {
                            row.Spacing(2);
                            row.Item().Text(item.EventName).Bold();
                            row.Item().Text($"Competidor: {item.CompetitorName}");
                            row.Item().Text($"Documento: {item.CompetitorDocument}");
                            row.Item().Text($"Numero: {item.CompetitorNumber}");
                            row.Item().Text($"Categoria: {item.CompetitorAgeCategory}");
                            row.Item().Text($"Valor: {CurrencyFormatter.FormatCop(item.TotalPrice)}");
                            row.Item().LineHorizontal(0.5f).LineColor(QColors.Grey.Lighten2);
                        });
                    }

                    col.Item().Text($"Total pagado: {CurrencyFormatter.FormatCop(order.TotalAmount)}").Bold().FontSize(12);
                    col.Item().Text($"Transaccion: {order.TransactionId}");
                    col.Item().Text("Gracias por tu compra.");
                });
            });
        }).GeneratePdf(filePath);

        return Task.FromResult(filePath);
    }
}
