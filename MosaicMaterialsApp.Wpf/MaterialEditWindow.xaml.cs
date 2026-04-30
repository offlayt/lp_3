using System.Windows;
using MaterialType = MosaicMaterialsApp.Wpf.src.AppDataContext.MaterialType;
using Material = MosaicMaterialsApp.Wpf.src.AppDataContext.Material;
using MosaicMaterialsApp.Wpf.src;

namespace MosaicMaterialsApp.Wpf;

public partial class MaterialEditWindow : Window
{
    private readonly int? _materialId;
    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

    public MaterialEditWindow(int? materialId)
    {
        _materialId = materialId;

        InitializeComponent();

        ApplyModeText();
        LoadMaterialTypes();
        LoadMaterialIfEditMode();
    }

    private void ShowMessage(string message, string title, MessageBoxImage image)
    {
        MessageBox.Show(this, message, title, MessageBoxButton.OK, image);
    }

    private void ApplyModeText()
    {
        var editMode = _materialId.HasValue;
        Title = editMode ? "Редактирование материала" : "Добавление материала";
        TitleTextBlock.Text = editMode ? "Редактирование данных материала" : "Добавление материала";
    }

    private void LoadMaterialTypes()
    {
        try
        {
            var materialTypes = AppData.GetMaterialTypes();
            MaterialTypeComboBox.ItemsSource = materialTypes;
            MaterialTypeComboBox.SelectedIndex = materialTypes.Count > 0 ? 0 : -1;
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось загрузить типы материалов.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private void LoadMaterialIfEditMode()
    {
        if (!_materialId.HasValue)
        {
            return;
        }

        try
        {
            var material = AppData.GetMaterialById(_materialId.Value);

            if (material is null)
            {
                ShowMessage("Материал для редактирования не найден.", "Ошибка", MessageBoxImage.Error);
                Close();
                return;
            }

            MaterialTypeComboBox.SelectedValue = material.MaterialTypeId;
            NameTextBox.Text = material.Name;
            StockQuantityTextBox.Text = material.StockQuantity.ToString("N2", RuCulture);
            UnitTextBox.Text = material.Unit;
            PackQuantityTextBox.Text = material.PackQuantity.ToString("N2", RuCulture);
            MinQuantityTextBox.Text = material.MinQuantity.ToString("N2", RuCulture);
            UnitPriceTextBox.Text = material.UnitPrice.ToString("N2", RuCulture);
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось загрузить материал.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
            Close();
        }
    }

    private static bool HasMoreThanTwoDecimalPlaces(decimal value)
    {
        return decimal.Round(value, 2, MidpointRounding.AwayFromZero) != value;
    }

    private bool TryParseDecimal(string source, out decimal value)
    {
        if (decimal.TryParse(source.Trim(), NumberStyles.Any, RuCulture, out value))
        {
            return true;
        }

        return decimal.TryParse(source.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    private bool ValidateInput(
        out MaterialType materialType,
        out decimal stockQuantity,
        out decimal packQuantity,
        out decimal minQuantity,
        out decimal unitPrice)
    {
        materialType = null!;
        stockQuantity = 0;
        packQuantity = 0;
        minQuantity = 0;
        unitPrice = 0;

        if (MaterialTypeComboBox.SelectedItem is not MaterialType selectedMaterialType)
        {
            ShowMessage("Выберите тип материала.", "Предупреждение", MessageBoxImage.Warning);
            return false;
        }

        materialType = selectedMaterialType;

        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            ShowMessage("Укажите наименование материала.", "Предупреждение", MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(UnitTextBox.Text))
        {
            ShowMessage("Укажите единицу измерения.", "Предупреждение", MessageBoxImage.Warning);
            return false;
        }

        if (!TryParseDecimal(StockQuantityTextBox.Text, out stockQuantity) || stockQuantity < 0)
        {
            ShowMessage("Количество на складе должно быть числом не меньше 0.", "Предупреждение", MessageBoxImage.Warning);
            return false;
        }

        if (!TryParseDecimal(PackQuantityTextBox.Text, out packQuantity) || packQuantity <= 0)
        {
            ShowMessage("Количество в упаковке должно быть числом больше 0.", "Предупреждение", MessageBoxImage.Warning);
            return false;
        }

        if (!TryParseDecimal(MinQuantityTextBox.Text, out minQuantity) || minQuantity < 0)
        {
            ShowMessage("Минимальное количество должно быть числом не меньше 0.", "Предупреждение", MessageBoxImage.Warning);
            return false;
        }

        if (!TryParseDecimal(UnitPriceTextBox.Text, out unitPrice) || unitPrice < 0)
        {
            ShowMessage("Цена должна быть числом не меньше 0.", "Предупреждение", MessageBoxImage.Warning);
            return false;
        }

        if (HasMoreThanTwoDecimalPlaces(unitPrice))
        {
            ShowMessage("Цена материала может содержать не более двух знаков после запятой.", "Предупреждение", MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void SaveMaterial()
    {
        if (!ValidateInput(out var materialType, out var stockQuantity, out var packQuantity, out var minQuantity, out var unitPrice))
        {
            return;
        }

        var material = new Material
        {
            Id = _materialId ?? 0,
            Name = NameTextBox.Text.Trim(),
            MaterialTypeId = materialType.Id,
            StockQuantity = stockQuantity,
            Unit = UnitTextBox.Text.Trim(),
            PackQuantity = packQuantity,
            MinQuantity = minQuantity,
            UnitPrice = decimal.Round(unitPrice, 2, MidpointRounding.AwayFromZero)
        };

        try
        {
            AppData.SaveMaterial(material);
            ShowMessage("Данные материала успешно сохранены.", "Информация", MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось сохранить материал.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveMaterial();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
