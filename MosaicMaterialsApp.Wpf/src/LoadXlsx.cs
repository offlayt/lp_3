using CsvHelper;
using CsvHelper.Configuration.Attributes;
using CsvHelper.Excel;

namespace MosaicMaterialsApp.Wpf.src;

public static class LoadXlsx
{
    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    private static string ResolveResourcePath(string relativePath)
    {
        return Path.Combine(AppContext.BaseDirectory, "res", relativePath);
    }

    public static List<T> LoadExcel<T>(string fileName)
    {
        var filePath = ResolveResourcePath(fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Файл импорта не найден: {fileName}", filePath);
        }

        using var csv = new CsvReader(new ExcelParser(filePath));
        return csv.GetRecords<T>().ToList();
    }

    public static bool TryParseDecimal(string value, out decimal result)
    {
        var text = value.Trim();
        return decimal.TryParse(text, NumberStyles.Any, RuCulture, out result)
            || decimal.TryParse(text, NumberStyles.Any, InvariantCulture, out result);
    }

    public static decimal ParseDecimal(string value)
    {
        if (TryParseDecimal(value, out var result))
        {
            return result;
        }

        throw new FormatException($"Не удалось преобразовать значение '{value}' в число.");
    }

    public static bool TryParseDouble(string value, out double result)
    {
        var text = value.Trim();
        return double.TryParse(text, NumberStyles.Any, RuCulture, out result)
            || double.TryParse(text, NumberStyles.Any, InvariantCulture, out result);
    }

    public static double ParseDouble(string value)
    {
        if (TryParseDouble(value, out var result))
        {
            return result;
        }

        throw new FormatException($"Не удалось преобразовать значение '{value}' в число.");
    }

    public static double ParsePercent(string value)
    {
        var text = value.Trim();

        if (text.EndsWith("%", StringComparison.Ordinal))
        {
            text = text[..^1].Trim();
            return ParseDouble(text) / 100d;
        }

        return ParseDouble(text);
    }

    public static DateTime ParseDate(string value)
    {
        var text = value.Trim();

        if (DateTime.TryParse(text, RuCulture, DateTimeStyles.None, out var result)
            || DateTime.TryParse(text, InvariantCulture, DateTimeStyles.None, out result))
        {
            return result;
        }

        return DateTime.FromOADate(ParseDouble(value));
    }

    public class MaterialTypeImport
    {
        [Index(0)]
        public required string Name { get; set; }

        [Index(1)]
        public required string LossPercent { get; set; }
    }

    public class MaterialImport
    {
        [Index(0)]
        public required string Name { get; set; }

        [Index(1)]
        public required string MaterialTypeName { get; set; }

        [Index(2)]
        public required string UnitPrice { get; set; }

        [Index(3)]
        public required string StockQuantity { get; set; }

        [Index(4)]
        public required string MinQuantity { get; set; }

        [Index(5)]
        public required string PackQuantity { get; set; }

        [Index(6)]
        public required string Unit { get; set; }
    }

    public class SupplierImport
    {
        [Index(0)]
        public required string Name { get; set; }

        [Index(1)]
        public required string SupplierType { get; set; }

        [Index(2)]
        public required string Inn { get; set; }

        [Index(3)]
        public required string Rating { get; set; }

        [Index(4)]
        public required string StartDate { get; set; }
    }

    public class MaterialSupplierImport
    {
        [Index(0)]
        public required string MaterialName { get; set; }

        [Index(1)]
        public required string SupplierName { get; set; }
    }

    public class ProductTypeImport
    {
        [Index(0)]
        public required string Name { get; set; }

        [Index(1)]
        public required string Coefficient { get; set; }
    }
}
