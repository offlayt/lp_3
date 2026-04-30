using Microsoft.EntityFrameworkCore;

namespace MosaicMaterialsApp.Wpf.src;

public static class MaterialCalculator
{
    public static decimal CalculateMinimalPurchaseCost(
        decimal stockQuantity,
        decimal minQuantity,
        decimal packQuantity,
        decimal unitPrice)
    {
        if (stockQuantity < 0 || minQuantity < 0 || packQuantity <= 0 || unitPrice < 0)
        {
            return 0m;
        }

        if (stockQuantity >= minQuantity)
        {
            return 0m;
        }

        var deficit = minQuantity - stockQuantity;
        var packsCount = decimal.Ceiling(deficit / packQuantity);
        var purchaseQuantity = packsCount * packQuantity;
        var cost = purchaseQuantity * unitPrice;

        return decimal.Round(cost < 0 ? 0 : cost, 2, MidpointRounding.AwayFromZero);
    }

    public static int CalculateProductQuantity(
        int productTypeId,
        int materialTypeId,
        int rawAmount,
        double parameter1,
        double parameter2)
    {
        if (productTypeId <= 0 || materialTypeId <= 0 || rawAmount <= 0)
        {
            return -1;
        }

        if (parameter1 <= 0 || parameter2 <= 0)
        {
            return -1;
        }

        using var db = new AppDataContext();

        var productType = db.ProductTypes.AsNoTracking().FirstOrDefault(item => item.Id == productTypeId);
        var materialType = db.MaterialTypes.AsNoTracking().FirstOrDefault(item => item.Id == materialTypeId);

        if (productType is null || materialType is null)
        {
            return -1;
        }

        var requiredPerUnit = parameter1 * parameter2 * productType.Coefficient;

        if (requiredPerUnit <= 0)
        {
            return -1;
        }

        var requiredWithLoss = requiredPerUnit * (1 + materialType.LossPercent);

        if (requiredWithLoss <= 0)
        {
            return -1;
        }

        var result = (int)Math.Floor(rawAmount / requiredWithLoss);
        return result < 0 ? -1 : result;
    }
}
