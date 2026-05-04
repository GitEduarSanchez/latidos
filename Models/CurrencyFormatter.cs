using System.Globalization;

namespace Latidos.Models;

public static class CurrencyFormatter
{
    private static readonly CultureInfo ColombianCulture = CultureInfo.GetCultureInfo("es-CO");

    public static string FormatCop(decimal value)
    {
        return $"COP ${value.ToString("N0", ColombianCulture)}";
    }
}
