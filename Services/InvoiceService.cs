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
            var htmlPath = Path.Combine(folder, $"{order.OrderNumber}.html");
            File.WriteAllText(htmlPath, BuildMobileInvoiceHtml(order));
            return Task.FromResult(htmlPath);
        }

        QuestPDF.Settings.License = LicenseType.Community;

        var filePath = Path.Combine(folder, $"{order.OrderNumber}.pdf");

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.ContinuousSize(80, Unit.Millimetre);
                page.Margin(10);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Spacing(5);

                    col.Item().AlignCenter().Text("LATIDOS").Bold().FontSize(16);
                    col.Item().AlignCenter().Text("TICKET DE INSCRIPCION").Bold();
                    col.Item().LineHorizontal(1).LineColor(QColors.Grey.Medium);

                    col.Item().Text($"Orden: {order.OrderNumber}").Bold();
                    col.Item().Text($"Fecha: {order.OrderDate:dd/MM/yyyy HH:mm}");
                    col.Item().Text($"Cliente: {order.CustomerName}");
                    col.Item().Text($"Correo: {order.CustomerEmail}");

                    col.Item().LineHorizontal(1).LineColor(QColors.Grey.Medium);
                    col.Item().AlignCenter().Text("COMPETIDORES").Bold().FontSize(10);
                    col.Item().LineHorizontal(1).LineColor(QColors.Grey.Medium);

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

                    col.Item().PaddingTop(5).AlignRight().Text($"TOTAL: {CurrencyFormatter.FormatCop(order.TotalAmount)}").Bold().FontSize(11);
                    col.Item().Text($"Transaccion: {order.TransactionId}");

                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(QColors.Grey.Medium);
                    col.Item().AlignCenter().Text("Gracias por tu compra").Italic();
                    col.Item().AlignCenter().Text("www.latidos.com");
                });
            });
        }).GeneratePdf(filePath);

        return Task.FromResult(filePath);
    }

    private static string BuildMobileInvoiceHtml(Order order)
    {
        var rows = string.Join("", order.Items.Select(item => $"""
            <section class="item">
                <div class="item-top">
                    <div>
                        <div class="event">{Escape(item.EventName)}</div>
                        <div class="name">{Escape(item.CompetitorName)}</div>
                    </div>
                    <div class="amount">{CurrencyFormatter.FormatCop(item.TotalPrice)}</div>
                </div>
                <div class="details">
                    <div><span>Documento</span>{Escape(item.CompetitorDocument)}</div>
                    <div><span>Numero</span>{Escape(item.CompetitorNumber)}</div>
                    <div><span>Categoria</span>{Escape(item.CompetitorAgeCategory)}</div>
                </div>
            </section>
            """));

        if (string.IsNullOrWhiteSpace(rows))
        {
            rows = """<section class="empty">Sin competidores registrados en esta orden.</section>""";
        }

        return $$"""
            <!doctype html>
            <html lang="es">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <title>Factura {{Escape(order.OrderNumber)}}</title>
                <style>
                    :root {
                        --ink: #24211c;
                        --muted: #756e61;
                        --line: #ede5d6;
                        --soft: #fbf8f1;
                        --gold: #c9a227;
                    }
                    * { box-sizing: border-box; }
                    body {
                        margin: 0;
                        padding: 14px;
                        color: var(--ink);
                        background: #f8f7f2;
                        font-family: Arial, sans-serif;
                    }
                    .invoice {
                        overflow: hidden;
                        border: 1px solid var(--line);
                        border-radius: 22px;
                        background: white;
                    }
                    .hero {
                        padding: 20px;
                        color: white;
                        background: #24211c;
                    }
                    h1 {
                        margin: 0;
                        color: var(--gold);
                        font-size: 26px;
                    }
                    .sub { margin-top: 4px; color: #d7d0c1; font-size: 13px; }
                    .meta {
                        display: grid;
                        gap: 10px;
                        padding: 16px 20px;
                        font-size: 13px;
                        background: var(--soft);
                        border-bottom: 1px solid var(--line);
                    }
                    .meta div { display: grid; gap: 2px; }
                    .meta strong { color: var(--muted); font-size: 11px; text-transform: uppercase; }
                    .meta span { overflow-wrap: anywhere; }
                    .items { display: grid; gap: 10px; padding: 14px; }
                    .item {
                        padding: 14px;
                        border: 1px solid var(--line);
                        border-radius: 16px;
                        background: white;
                    }
                    .item-top {
                        display: grid;
                        grid-template-columns: 1fr auto;
                        gap: 12px;
                        align-items: start;
                    }
                    .event { font-size: 14px; font-weight: 700; }
                    .name { margin-top: 3px; color: var(--muted); font-size: 13px; }
                    .amount { white-space: nowrap; font-size: 13px; font-weight: 700; color: var(--gold); }
                    .details {
                        display: grid;
                        grid-template-columns: 1fr;
                        gap: 8px;
                        margin-top: 12px;
                    }
                    .details div {
                        padding: 8px;
                        border-radius: 12px;
                        background: var(--soft);
                        font-size: 12px;
                        overflow-wrap: anywhere;
                    }
                    .details span {
                        display: block;
                        margin-bottom: 3px;
                        color: var(--muted);
                        font-size: 10px;
                        font-weight: 700;
                        text-transform: uppercase;
                    }
                    .empty { padding: 18px; color: var(--muted); text-align: center; }
                    .total {
                        margin: 0 14px 14px;
                        padding: 16px;
                        display: flex;
                        flex-direction: column;
                        gap: 4px;
                        border-radius: 16px;
                        color: white;
                        background: #24211c;
                        font-size: 14px;
                        font-weight: 700;
                    }
                    .total span:last-child { color: var(--gold); font-size: 20px; }
                    .footer {
                        padding: 0 20px 20px;
                        color: var(--muted);
                        font-size: 12px;
                        text-align: center;
                    }
                    @media (min-width: 520px) {
                        .details { grid-template-columns: repeat(3, 1fr); }
                        .total { align-items: center; flex-direction: row; justify-content: space-between; }
                    }
                </style>
            </head>
            <body>
                <main class="invoice">
                    <header class="hero">
                        <h1>LATIDOS</h1>
                        <div class="sub">Factura de inscripcion</div>
                    </header>

                    <div class="meta">
                        <div><strong>Orden</strong><span>{{Escape(order.OrderNumber)}}</span></div>
                        <div><strong>Fecha</strong><span>{{order.OrderDate:dd/MM/yyyy HH:mm}}</span></div>
                        <div><strong>Cliente</strong><span>{{Escape(order.CustomerName)}}</span></div>
                        <div><strong>Correo</strong><span>{{Escape(order.CustomerEmail)}}</span></div>
                        <div><strong>Transaccion</strong><span>{{Escape(order.TransactionId)}}</span></div>
                    </div>

                    <div class="items">
                        {{rows}}
                    </div>

                    <div class="total">
                        <span>Total pagado</span>
                        <span>{{CurrencyFormatter.FormatCop(order.TotalAmount)}}</span>
                    </div>
                    <div class="footer">Gracias por tu compra</div>
                </main>
            </body>
            </html>
            """;
    }

    private static string Escape(string? value)
    {
        return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
