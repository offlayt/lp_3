using Microsoft.EntityFrameworkCore;

namespace MosaicMaterialsApp.Wpf.src;

public static class AppData
{
    public static void EnsureCreatedAndSeed()
    {
        using var db = new AppDataContext();
        db.EnsureCreatedAndSeed();
    }

    public static List<MaterialListItem> GetMaterials()
    {
        using var db = new AppDataContext();

        return db.Materials
            .AsNoTracking()
            .Select(item => new MaterialListItem
            {
                Id = item.Id,
                TypeName = item.MaterialType!.Name,
                Name = item.Name,
                UnitPrice = item.UnitPrice,
                StockQuantity = item.StockQuantity,
                MinQuantity = item.MinQuantity,
                PackQuantity = item.PackQuantity,
                Unit = item.Unit
            })
            .OrderBy(item => item.Name)
            .ToList()
            .Select(item =>
            {
                item.MinimalPurchaseCost = MaterialCalculator.CalculateMinimalPurchaseCost(
                    item.StockQuantity,
                    item.MinQuantity,
                    item.PackQuantity,
                    item.UnitPrice);
                return item;
            })
            .ToList();
    }

    public static List<AppDataContext.MaterialType> GetMaterialTypes()
    {
        using var db = new AppDataContext();
        return db.MaterialTypes.AsNoTracking().OrderBy(item => item.Name).ToList();
    }

    public static List<AppDataContext.ProductType> GetProductTypes()
    {
        using var db = new AppDataContext();
        return db.ProductTypes.AsNoTracking().OrderBy(item => item.Name).ToList();
    }

    public static AppDataContext.Material? GetMaterialById(int materialId)
    {
        using var db = new AppDataContext();
        return db.Materials.AsNoTracking().FirstOrDefault(item => item.Id == materialId);
    }

    public static void SaveMaterial(AppDataContext.Material material)
    {
        using var db = new AppDataContext();

        if (material.Id <= 0)
        {
            db.Materials.Add(material);
            db.SaveChanges();
            return;
        }

        var existing = db.Materials.FirstOrDefault(item => item.Id == material.Id)
            ?? throw new InvalidOperationException("Редактируемый материал не найден.");

        existing.Name = material.Name;
        existing.MaterialTypeId = material.MaterialTypeId;
        existing.UnitPrice = material.UnitPrice;
        existing.StockQuantity = material.StockQuantity;
        existing.MinQuantity = material.MinQuantity;
        existing.PackQuantity = material.PackQuantity;
        existing.Unit = material.Unit;

        db.SaveChanges();
    }

    public static List<SupplierListItem> GetSuppliersByMaterial(int materialId)
    {
        using var db = new AppDataContext();

        return db.MaterialSuppliers
            .AsNoTracking()
            .Where(item => item.MaterialId == materialId)
            .Select(item => new SupplierListItem
            {
                Name = item.Supplier!.Name,
                Rating = item.Supplier.Rating,
                StartDate = item.Supplier.StartDate
            })
            .OrderBy(item => item.Name)
            .ToList();
    }
}

public class MaterialListItem
{
    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

    public int Id { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal MinQuantity { get; set; }
    public decimal PackQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal MinimalPurchaseCost { get; set; }

    public string Title => $"{TypeName} | {Name}";
    public string UnitPriceText => UnitPrice.ToString("N2", RuCulture);
    public string StockQuantityText => StockQuantity.ToString("N2", RuCulture);
    public string MinQuantityText => MinQuantity.ToString("N2", RuCulture);
    public string PackQuantityText => PackQuantity.ToString("N2", RuCulture);
    public string MinimalPurchaseCostText => MinimalPurchaseCost.ToString("N2", RuCulture);
    public string StockInfo => $"{StockQuantityText} {Unit}";
    public string MinInfo => $"{MinQuantityText} {Unit}";
    public string PackInfo => $"{PackQuantityText} {Unit}";
}

public class SupplierListItem
{
    public string Name { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime StartDate { get; set; }
    public string StartDateText => StartDate.ToString("dd.MM.yyyy");
}
