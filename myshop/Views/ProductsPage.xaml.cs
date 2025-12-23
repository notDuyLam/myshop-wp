using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using myshop.ViewModels;

namespace myshop.Views;

public sealed partial class ProductsPage : Page
{
    public ProductsViewModel ViewModel { get; }

    // ✅ Add cancellation token
    private CancellationTokenSource? _cts;
    private bool _isDisposed = false;

    public ProductsPage()
    {
        InitializeComponent();

        _cts = new CancellationTokenSource();

        // Get ViewModel from DI
        ViewModel = App.ServiceProvider?.GetService<ProductsViewModel>()
            ?? throw new InvalidOperationException("ProductsViewModel not found in DI container");

        DataContext = ViewModel;

        // Wire up event handlers
        WireUpEventHandlers();

        // ✅ Cleanup khi page bị unload
        Unloaded += ProductsPage_Unloaded;
        Loaded += ProductsPage_Loaded;
    }

    private void WireUpEventHandlers()
    {
        SearchBox.TextChanged += SearchBox_TextChanged;
        CategoryComboBox.SelectionChanged += CategoryComboBox_SelectionChanged;
        SortComboBox.SelectionChanged += SortComboBox_SelectionChanged;
        MinPriceBox.ValueChanged += PriceBox_ValueChanged;
        MaxPriceBox.ValueChanged += PriceBox_ValueChanged;

        FirstPageButton.Click += FirstPageButton_Click;
        PreviousPageButton.Click += PreviousPageButton_Click;
        NextPageButton.Click += NextPageButton_Click;
        LastPageButton.Click += LastPageButton_Click;
        PageNumberBox.ValueChanged += PageNumberBox_ValueChanged;
        PageSizeComboBox.SelectionChanged += PageSizeComboBox_SelectionChanged;

        AddButton.Click += AddButton_Click;
        EditButton.Click += EditButton_Click;
        DeleteButton.Click += DeleteButton_Click;
    }

    // ✅ QUAN TRỌNG: Cleanup khi page bị unload
    private void ProductsPage_Unloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _isDisposed = true;

            // Cancel tất cả async operations
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            // Unsubscribe tất cả events
            UnwireEventHandlers();

            Loaded -= ProductsPage_Loaded;
            Unloaded -= ProductsPage_Unloaded;

            // Dispose ViewModel nếu implement IDisposable
            if (ViewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }

            // Clear DataContext
            DataContext = null;
            ProductsDataGrid.ItemsSource = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ProductsPage_Unloaded: {ex.Message}");
        }
    }

    private void UnwireEventHandlers()
    {
        try
        {
            SearchBox.TextChanged -= SearchBox_TextChanged;
            CategoryComboBox.SelectionChanged -= CategoryComboBox_SelectionChanged;
            SortComboBox.SelectionChanged -= SortComboBox_SelectionChanged;
            MinPriceBox.ValueChanged -= PriceBox_ValueChanged;
            MaxPriceBox.ValueChanged -= PriceBox_ValueChanged;

            FirstPageButton.Click -= FirstPageButton_Click;
            PreviousPageButton.Click -= PreviousPageButton_Click;
            NextPageButton.Click -= NextPageButton_Click;
            LastPageButton.Click -= LastPageButton_Click;
            PageNumberBox.ValueChanged -= PageNumberBox_ValueChanged;
            PageSizeComboBox.SelectionChanged -= PageSizeComboBox_SelectionChanged;

            AddButton.Click -= AddButton_Click;
            EditButton.Click -= EditButton_Click;
            DeleteButton.Click -= DeleteButton_Click;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error unwiring events: {ex.Message}");
        }
    }

    private void ProductsPage_Loaded(object sender, RoutedEventArgs e)
    {
        ProductsDataGrid.ItemsSource = ViewModel.Products;
        UpdatePaginationUI();
    }

    // ✅ Helper để check disposed
    private bool CanExecute()
    {
        return !_isDisposed && _cts != null && !_cts.Token.IsCancellationRequested;
    }

    // =========================
    // SEARCH & FILTER (with cancellation)
    // =========================

    private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!CanExecute()) return;

        try
        {
            ViewModel.SearchKeyword = SearchBox.Text;
            ViewModel.CurrentPage = 1;
            await ViewModel.LoadProductsCommand.ExecuteAsync(_cts!.Token); // ✅ Pass token
            UpdatePaginationUI();
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            Debug.WriteLine("Search cancelled or disposed");
        }
    }

    private async void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!CanExecute()) return;

        try
        {
            if (CategoryComboBox.SelectedIndex == 0 || CategoryComboBox.SelectedIndex == -1)
            {
                ViewModel.SelectedCategory = null;
            }

            ViewModel.CurrentPage = 1;
            await ViewModel.LoadProductsCommand.ExecuteAsync(_cts!.Token);
            UpdatePaginationUI();
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            Debug.WriteLine("Category filter cancelled or disposed");
        }
    }

    private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!CanExecute()) return;

        try
        {
            ViewModel.SelectedSort = SortComboBox.SelectedIndex switch
            {
                0 => "Name",
                1 => "Price",
                2 => "Stock",
                _ => "Name"
            };

            await ViewModel.LoadProductsCommand.ExecuteAsync(_cts!.Token);
            UpdatePaginationUI();
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            Debug.WriteLine("Sort cancelled or disposed");
        }
    }

    private async void PriceBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
    {
        if (!CanExecute()) return;

        try
        {
            await ViewModel.LoadProductsCommand.ExecuteAsync(_cts!.Token);
            UpdatePaginationUI();
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            Debug.WriteLine("Price filter cancelled or disposed");
        }
    }

    // =========================
    // PAGINATION (replace lambdas with named methods)
    // =========================

    private void FirstPageButton_Click(object sender, RoutedEventArgs e)
        => _ = SafeNavigateToPageAsync(1);

    private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        => _ = SafeNavigateToPageAsync(ViewModel.CurrentPage - 1);

    private void NextPageButton_Click(object sender, RoutedEventArgs e)
        => _ = SafeNavigateToPageAsync(ViewModel.CurrentPage + 1);

    private void LastPageButton_Click(object sender, RoutedEventArgs e)
        => _ = SafeNavigateToPageAsync(ViewModel.TotalPages);

    private async Task SafeNavigateToPageAsync(int page)
    {
        if (!CanExecute()) return;

        try
        {
            if (page < 1 || page > ViewModel.TotalPages) return;

            ViewModel.CurrentPage = page;
            await ViewModel.LoadProductsCommand.ExecuteAsync(_cts!.Token);
            UpdatePaginationUI();
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            Debug.WriteLine("Navigation cancelled or disposed");
        }
    }

    private void PageNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
    {
        if (!CanExecute() || double.IsNaN(e.NewValue)) return;

        int page = (int)e.NewValue;
        if (page >= 1 && page <= ViewModel.TotalPages && page != ViewModel.CurrentPage)
        {
            _ = SafeNavigateToPageAsync(page);
        }
    }

    private async void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!CanExecute()) return;

        try
        {
            if (PageSizeComboBox.SelectedItem is ComboBoxItem item &&
                int.TryParse(item.Content?.ToString(), out int pageSize))
            {
                ViewModel.PageSize = pageSize;
                ViewModel.CurrentPage = 1;
                await ViewModel.LoadProductsCommand.ExecuteAsync(_cts!.Token);
                UpdatePaginationUI();
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            Debug.WriteLine("Page size change cancelled or disposed");
        }
    }

    private void UpdatePaginationUI()
    {
        if (_isDisposed) return;

        try
        {
            PageInfoText.Text = $"Trang {ViewModel.CurrentPage} / {Math.Max(1, ViewModel.TotalPages)}";
            PageNumberBox.Value = ViewModel.CurrentPage;
            PageNumberBox.Maximum = Math.Max(1, ViewModel.TotalPages);

            FirstPageButton.IsEnabled = ViewModel.CurrentPage > 1;
            PreviousPageButton.IsEnabled = ViewModel.CurrentPage > 1;
            NextPageButton.IsEnabled = ViewModel.CurrentPage < ViewModel.TotalPages;
            LastPageButton.IsEnabled = ViewModel.CurrentPage < ViewModel.TotalPages;

            TotalProductsText.Text = $"Tổng: {ViewModel.Products.Count} sản phẩm";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in UpdatePaginationUI: {ex.Message}");
        }
    }

    // =========================
    // ACTION BUTTONS (giữ nguyên logic, thêm check)
    // =========================

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!CanExecute()) return;

        try
        {
            var dialog = new ProductDetailDialog { XamlRoot = this.XamlRoot };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.LoadProductsCommand.ExecuteAsync(_cts!.Token);
                UpdatePaginationUI();
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            Debug.WriteLine("Add cancelled or disposed");
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (!CanExecute()) return;

        try
        {
            if (ProductsDataGrid.SelectedItem == null)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Chưa chọn sản phẩm",
                    Content = "Vui lòng chọn một sản phẩm để sửa.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await noSelectionDialog.ShowAsync();
                return;
            }

            ViewModel.SelectedProduct = ProductsDataGrid.SelectedItem as myshop_data.Models.Product;

            var dialog = new ProductDetailDialog { XamlRoot = this.XamlRoot };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.LoadProductsCommand.ExecuteAsync(_cts!.Token);
                UpdatePaginationUI();
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            Debug.WriteLine("Edit cancelled or disposed");
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!CanExecute()) return;

        try
        {
            if (ProductsDataGrid.SelectedItem == null)
            {
                var noSelectionDialog = new ContentDialog
                {
                    Title = "Chưa chọn sản phẩm",
                    Content = "Vui lòng chọn một sản phẩm để xóa.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await noSelectionDialog.ShowAsync();
                return;
            }

            ViewModel.SelectedProduct = ProductsDataGrid.SelectedItem as myshop_data.Models.Product;

            var confirmDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = $"Bạn có chắc chắn muốn xóa sản phẩm '{ViewModel.SelectedProduct?.Name}'?",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteProductCommand.ExecuteAsync(_cts!.Token);

                if (!string.IsNullOrEmpty(ViewModel.ErrorMessage))
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = ViewModel.ErrorMessage,
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
                else
                {
                    await ViewModel.LoadProductsCommand.ExecuteAsync(_cts!.Token);
                    UpdatePaginationUI();
                }
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            Debug.WriteLine("Delete cancelled or disposed");
        }
    }
}