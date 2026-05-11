using System.Globalization;
using System.Windows;
using System.Windows.Input;
using MosaicMaterialsApp.Wpf.src;
using ProductType = MosaicMaterialsApp.Wpf.src.AppDataContext.ProductType;
using MaterialType = MosaicMaterialsApp.Wpf.src.AppDataContext.MaterialType;

namespace MosaicMaterialsApp.Wpf;

public partial class MainWindow : Window
{
    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

    public MainWindow()
    {
        InitializeComponent();
        LoadMaterials();
        LoadDictionaries();
    }

    private void ShowMessage(string message, string title, MessageBoxImage image)
    {
        MessageBox.Show(this, message, title, MessageBoxButton.OK, image);
    }

    private void LoadMaterials()
    {
        try
        {
            var materials = AppData.GetMaterials();
            MaterialsListBox.ItemsSource = materials;

            if (materials.Count > 0)
            {
                MaterialsListBox.SelectedIndex = 0;
            }
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось загрузить материалы.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private void LoadDictionaries()
    {
        try
        {
            var productTypes = AppData.GetProductTypes();
            var materialTypes = AppData.GetMaterialTypes();

            ProductTypeComboBox.ItemsSource = productTypes;
            MaterialTypeComboBox.ItemsSource = materialTypes;

            ProductTypeComboBox.SelectedIndex = productTypes.Count > 0 ? 0 : -1;
            MaterialTypeComboBox.SelectedIndex = materialTypes.Count > 0 ? 0 : -1;
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось загрузить справочники.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private MaterialListItem? GetSelectedMaterial(string? warningMessage = null)
    {
        var material = MaterialsListBox.SelectedItem as MaterialListItem;

        if (material is null && !string.IsNullOrWhiteSpace(warningMessage))
        {
            ShowMessage(warningMessage, "Предупреждение", MessageBoxImage.Warning);
        }

        return material;
    }

    private void OpenMaterialEditor(int? materialId)
    {
        var window = new MaterialEditWindow(materialId) { Owner = this };
        window.ShowDialog();
        LoadMaterials();
    }

    private bool TryParsePositiveInt(string value, string warningMessage, out int result)
    {
        if (int.TryParse(value.Trim(), NumberStyles.Integer, RuCulture, out result) && result > 0)
        {
            return true;
        }

        ShowMessage(warningMessage, "Предупреждение", MessageBoxImage.Warning);
        return false;
    }

    private bool TryParsePositiveDouble(string value, string warningMessage, out double result)
    {
        if (LoadXlsx.TryParseDouble(value, out result) && result > 0)
        {
            return true;
        }

        ShowMessage(warningMessage, "Предупреждение", MessageBoxImage.Warning);
        return false;
    }

    private void CalculateProductQuantity()
    {
        if (ProductTypeComboBox.SelectedItem is not ProductType productType)
        {
            ShowMessage("Выберите тип продукции.", "Предупреждение", MessageBoxImage.Warning);
            return;
        }

        if (MaterialTypeComboBox.SelectedItem is not MaterialType materialType)
        {
            ShowMessage("Выберите тип материала.", "Предупреждение", MessageBoxImage.Warning);
            return;
        }

        if (!TryParsePositiveInt(RawAmountTextBox.Text, "Укажите корректное количество сырья (больше 0).", out var rawAmount))
        {
            return;
        }

        if (!TryParsePositiveDouble(Parameter1TextBox.Text, "Укажите корректный параметр 1 (больше 0).", out var parameter1))
        {
            return;
        }

        if (!TryParsePositiveDouble(Parameter2TextBox.Text, "Укажите корректный параметр 2 (больше 0).", out var parameter2))
        {
            return;
        }

        try
        {
            var result = MaterialCalculator.CalculateProductQuantity(
                productType.Id,
                materialType.Id,
                rawAmount,
                parameter1,
                parameter2);

            if (result < 0)
            {
                ShowMessage("Не удалось выполнить расчет. Проверьте входные данные.", "Предупреждение", MessageBoxImage.Warning);
                CalculationResultTextBlock.Text = "Результат: -1";
                return;
            }

            CalculationResultTextBlock.Text = $"Результат: {result}";
        }
        catch (Exception exception)
        {
            ShowMessage($"Ошибка при расчете количества продукции.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private void AddMaterialButton_Click(object sender, RoutedEventArgs e)
    {
        OpenMaterialEditor(materialId: null);
    }

    private void SuppliersButton_Click(object sender, RoutedEventArgs e)
    {
        var material = GetSelectedMaterial("Выберите материал, чтобы открыть список поставщиков.");

        if (material is null)
        {
            return;
        }

        var window = new MaterialSuppliersWindow(material.Id, material.Name) { Owner = this };
        window.ShowDialog();
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        CalculateProductQuantity();
    }

    private void MaterialsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var material = MaterialsListBox.SelectedItem as MaterialListItem;

        if (material is null)
        {
            return;
        }

        OpenMaterialEditor(material.Id);
    }
}
