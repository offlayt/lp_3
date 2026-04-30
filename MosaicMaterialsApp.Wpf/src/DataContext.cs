using Microsoft.EntityFrameworkCore;

namespace MosaicMaterialsApp.Wpf.src;

public class AppDataContext : DbContext
{
    private static readonly string ArtifactsFolder = Path.Combine(AppContext.BaseDirectory, "artifacts");

    public DbSet<MaterialType> MaterialTypes => Set<MaterialType>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<MaterialSupplier> MaterialSuppliers => Set<MaterialSupplier>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaterialSupplier>()
            .HasKey(item => new { item.MaterialId, item.SupplierId });

        modelBuilder.Entity<Material>()
            .Property(item => item.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Material>()
            .Property(item => item.StockQuantity)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Material>()
            .Property(item => item.MinQuantity)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Material>()
            .Property(item => item.PackQuantity)
            .HasPrecision(18, 2);
    }

    public void EnsureCreatedAndSeed()
    {
        Directory.CreateDirectory(ArtifactsFolder);

        try
        {
            Database.EnsureCreated();

            var hasAnyData = Materials.Any() || Suppliers.Any() || MaterialTypes.Any() || ProductTypes.Any() || MaterialSuppliers.Any();
            var hasFullData = Materials.Any() && Suppliers.Any() && MaterialTypes.Any() && ProductTypes.Any() && MaterialSuppliers.Any();

            if (hasFullData)
            {
                GenerateScript();
                return;
            }

            if (hasAnyData)
            {
                Database.EnsureDeleted();
                Database.EnsureCreated();
            }

            Seed();
            GenerateScript();
        }
        catch
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
            Seed();
            GenerateScript();
        }
    }

    private void Seed()
    {
        if (Materials.Any() || Suppliers.Any() || MaterialTypes.Any() || ProductTypes.Any())
        {
            return;
        }

        var materialTypeImports = LoadXlsx.LoadExcel<LoadXlsx.MaterialTypeImport>("Material_type_import.xlsx");
        var materialImports = LoadXlsx.LoadExcel<LoadXlsx.MaterialImport>("Materials_import.xlsx");
        var supplierImports = LoadXlsx.LoadExcel<LoadXlsx.SupplierImport>("Suppliers_import.xlsx");
        var materialSupplierImports = LoadXlsx.LoadExcel<LoadXlsx.MaterialSupplierImport>("Material_suppliers_import.xlsx");
        var productTypeImports = LoadXlsx.LoadExcel<LoadXlsx.ProductTypeImport>("Product_type_import.xlsx");

        var materialTypes = materialTypeImports
            .GroupBy(item => item.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Select(item => new MaterialType
            {
                Name = item.Name.Trim(),
                LossPercent = LoadXlsx.ParsePercent(item.LossPercent)
            })
            .ToList();

        var productTypes = productTypeImports
            .GroupBy(item => item.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Select(item => new ProductType
            {
                Name = item.Name.Trim(),
                Coefficient = LoadXlsx.ParseDouble(item.Coefficient)
            })
            .ToList();

        var materialTypeByName = materialTypes.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);

        var materials = materialImports
            .Select(item => new Material
            {
                Name = item.Name.Trim(),
                MaterialType = materialTypeByName[item.MaterialTypeName.Trim()],
                UnitPrice = LoadXlsx.ParseDecimal(item.UnitPrice),
                StockQuantity = LoadXlsx.ParseDecimal(item.StockQuantity),
                MinQuantity = LoadXlsx.ParseDecimal(item.MinQuantity),
                PackQuantity = LoadXlsx.ParseDecimal(item.PackQuantity),
                Unit = item.Unit.Trim()
            })
            .ToList();

        var suppliers = supplierImports
            .Select(item => new Supplier
            {
                Name = item.Name.Trim(),
                SupplierType = item.SupplierType.Trim(),
                Inn = item.Inn.Trim(),
                Rating = (int)LoadXlsx.ParseDecimal(item.Rating),
                StartDate = LoadXlsx.ParseDate(item.StartDate)
            })
            .ToList();

        var materialByName = materials.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);
        var supplierByName = suppliers.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);

        var materialSuppliers = materialSupplierImports
            .DistinctBy(item => $"{item.MaterialName.Trim()}|{item.SupplierName.Trim()}", StringComparer.OrdinalIgnoreCase)
            .Select(item => new MaterialSupplier
            {
                Material = materialByName[item.MaterialName.Trim()],
                Supplier = supplierByName[item.SupplierName.Trim()]
            })
            .ToList();

        MaterialTypes.AddRange(materialTypes);
        ProductTypes.AddRange(productTypes);
        Materials.AddRange(materials);
        Suppliers.AddRange(suppliers);
        MaterialSuppliers.AddRange(materialSuppliers);

        SaveChanges();
    }

    private void GenerateScript()
    {
        var scriptPath = Path.Combine(ArtifactsFolder, "database_script.sql");
        var script = Database.GenerateCreateScript();
        File.WriteAllText(scriptPath, script);
    }

    public class MaterialType
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public double LossPercent { get; set; }
        public List<Material> Materials { get; set; } = new();
    }

    public class Material
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int MaterialTypeId { get; set; }
        public MaterialType? MaterialType { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal StockQuantity { get; set; }
        public decimal MinQuantity { get; set; }
        public decimal PackQuantity { get; set; }
        public required string Unit { get; set; }
        public List<MaterialSupplier> MaterialSuppliers { get; set; } = new();
    }

    public class Supplier
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string SupplierType { get; set; }
        public required string Inn { get; set; }
        public int Rating { get; set; }
        public DateTime StartDate { get; set; }
        public List<MaterialSupplier> MaterialSuppliers { get; set; } = new();
    }

    public class MaterialSupplier
    {
        public int MaterialId { get; set; }
        public Material? Material { get; set; }
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
    }

    public class ProductType
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public double Coefficient { get; set; }
    }
}
