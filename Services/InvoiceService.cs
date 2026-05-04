using Latidos.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QColors = QuestPDF.Helpers.Colors;
namespace Latidos.Services;

public class InvoiceService : IInvoiceService
{
    public Task<string> GenerateOrderTicketPdfAsync(Order order)
    {
        var folder = Path.Combine(FileSystem.AppDataDirectory, "facturas");
        Directory.CreateDirectory(folder);

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            // QuestPDF no soporta runtime Android. En móvil generamos ticket HTML.
            var htmlPath = Path.Combine(folder, $"{order.OrderNumber}.html");

            var rows = string.Join("", order.Items.Select(item => $"""
                <tr>
                    <td>{Escape(item.EventName)}</td>
                    <td>{Escape(item.CompetitorName)}</td>
                    <td>{Escape(item.CompetitorDocument)}</td>
                    <td>{Escape(item.CompetitorNumber)}</td>
                    <td>{Escape(item.CompetitorAgeCategory)}</td>
                    <td class="num">{CurrencyFormatter.FormatCop(item.TotalPrice)}</td>
                </tr>
                """));
            if (string.IsNullOrWhiteSpace(rows))
            {
                rows = """
                    <tr>
                        <td colspan="6">Sin competidores registrados en esta orden.</td>
                    </tr>
                    """;
            }

            var html = $$"""
                <!doctype html>
                <html lang="es">
                <head>
                    <meta charset="utf-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                    <title>Factura {{Escape(order.OrderNumber)}}</title>
                    <style>
                        body { font-family: Arial, sans-serif; margin: 20px; color: #1f1f1f; }
                        .card { border: 1px solid #e5e5e5; border-radius: 10px; padding: 16px; }
                        h1 { margin: 0; color: #c9a227; font-size: 24px; }
                        .sub { margin: 4px 0 12px; color: #666; font-size: 13px; }
                        .meta { font-size: 13px; line-height: 1.55; margin-bottom: 12px; }
                        table { width: 100%; border-collapse: collapse; font-size: 12px; }
                        th, td { border-bottom: 1px solid #eee; padding: 8px 6px; text-align: left; }
                        th { background: #fafafa; }
                        .num { text-align: right; white-space: nowrap; }
                        .total { margin-top: 14px; font-size: 16px; font-weight: 700; color: #111; }
                    </style>
                </head>
                <body>
                    <div class="card">
                        <h1>LATIDOS</h1>
                        <div class="sub">Factura de inscripción</div>

                        <div class="meta">
                            <div><strong>Orden:</strong> {{Escape(order.OrderNumber)}}</div>
                            <div><strong>Fecha:</strong> {{order.OrderDate:dd/MM/yyyy HH:mm}}</div>
                            <div><strong>Cliente:</strong> {{Escape(order.CustomerName)}}</div>
                            <div><strong>Correo:</strong> {{Escape(order.CustomerEmail)}}</div>
                            <div><strong>Transacción:</strong> {{Escape(order.TransactionId)}}</div>
                        </div>

                        <table>
                            <thead>
                                <tr>
                                    <th>Evento</th>
                                    <th>Competidor</th>
                                    <th>Documento</th>
                                    <th>Número</th>
                                    <th>Categoría</th>
                                    <th class="num">Valor</th>
                                </tr>
                            </thead>
                            <tbody>
                                {{rows}}
                            </tbody>
                        </table>

                        <div class="total">Total pagado: {{CurrencyFormatter.FormatCop(order.TotalAmount)}}</div>
                    </div>
                </body>
                </html>
                """;

            File.WriteAllText(htmlPath, html);
            return Task.FromResult(htmlPath);
        }

        QuestPDF.Settings.License = LicenseType.Community;

        var filePath = Path.Combine(folder, $"{order.OrderNumber}.pdf");

        Document.Create(container =>
        {
            container.Page(page =>
            {
                // 80mm roll continuous size
                page.ContinuousSize(80, Unit.Millimetre);
                page.Margin(10);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Spacing(5);
                    
                    // Header
                    col.Item().AlignCenter().Text("LATIDOS").Bold().FontSize(16);
                    col.Item().AlignCenter().Text("TICKET DE INSCRIPCION").Bold();
                    col.Item().LineHorizontal(1).LineColor(QColors.Grey.Medium);
                    
                    // Order Info
                    col.Item().Text($"Orden: {order.OrderNumber}").Bold();
                    col.Item().Text($"Fecha: {order.OrderDate:dd/MM/yyyy HH:mm}");
                    col.Item().Text($"Cliente: {order.CustomerName}");
                    col.Item().Text($"Correo: {order.CustomerEmail}");
                    
                    col.Item().LineHorizontal(1).LineColor(QColors.Grey.Medium);
                    col.Item().AlignCenter().Text("COMPETIDORES").Bold().FontSize(10);
                    col.Item().LineHorizontal(1).LineColor(QColors.Grey.Medium);

                    // Items / Competitors
                    foreach (var item in order.Items)
                    {
                        col.Item().Column(row =>
                        {
                            row.Spacing(2);
                            row.Item().Text(item.EventName).Bold();
                            row.Item().Text($"Nombre: {item.CompetitorName}");
                            row.Item().Text($"Doc: {item.CompetitorDocument}");
                            row.Item().Text($"Num: {item.CompetitorNumber}");
                            row.Item().Text($"Cat: {item.CompetitorAgeCategory}");
                            row.Item().AlignRight().Text($"Valor: {CurrencyFormatter.FormatCop(item.TotalPrice)}");
                            row.Item().LineHorizontal(0.5f).LineColor(QColors.Grey.Lighten2);
                        });
                    }

                    // Totals
                    col.Item().PaddingTop(5).AlignRight().Text($"TOTAL: {CurrencyFormatter.FormatCop(order.TotalAmount)}").Bold().FontSize(11);
                    col.Item().Text($"Transaccion: {order.TransactionId}");
                    
                    // Footer
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(QColors.Grey.Medium);
                    col.Item().AlignCenter().Text("Gracias por tu compra").Italic();
                    col.Item().AlignCenter().Text("www.latidos.com");
                });
            });
        }).GeneratePdf(filePath);

        return Task.FromResult(filePath);
    }

    private static string Escape(string? value)
    {
        return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
