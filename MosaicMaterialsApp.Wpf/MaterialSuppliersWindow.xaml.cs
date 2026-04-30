using System.Windows;
using MosaicMaterialsApp.Wpf.src;

namespace MosaicMaterialsApp.Wpf;

public partial class MaterialSuppliersWindow : Window
{
    private readonly int _materialId;

    public MaterialSuppliersWindow(int materialId, string materialName)
    {
        _materialId = materialId;

        InitializeComponent();
        MaterialNameTextBlock.Text = $"Материал: {materialName}";
        LoadSuppliers();
    }

    private void ShowMessage(string message, string title, MessageBoxImage image)
    {
        MessageBox.Show(this, message, title, MessageBoxButton.OK, image);
    }

    private void LoadSuppliers()
    {
        try
        {
            SuppliersDataGrid.ItemsSource = AppData.GetSuppliersByMaterial(_materialId);
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось загрузить список поставщиков.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
