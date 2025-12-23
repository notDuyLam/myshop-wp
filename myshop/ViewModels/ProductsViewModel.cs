using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.EntityFrameworkCore;
using myshop.Services;
using myshop_data.Data;
using myshop_data.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace myshop.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly DbContextService _dbContextService;
    private CancellationTokenSource? _cancellationTokenSource;

    public ProductsViewModel(DbContextService dbContextService)
    {
        _dbContextService = dbContextService;
        _cancellationTokenSource = new CancellationTokenSource();

        Products = new ObservableCollection<Product>();
        Categories = new ObservableCollection<Category>();

        // Wrap trong try-catch để tránh crash khi app đóng
        _ = SafeInitializeAsync();
    }

    // =========================
    // COLLECTIONS
    // =========================

    public ObservableCollection<Product> Products { get; }
    public ObservableCollection<Category> Categories { get; }

    // =========================
    // PROPERTIES
    // =========================

    [ObservableProperty] private Product? selectedProduct;
    [ObservableProperty] private Category? selectedCategory;
    [ObservableProperty] private string? searchKeyword;
    [ObservableProperty] private string selectedSort = "Name";
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;

    // =========================
    // PAGINATION
    // =========================

    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int pageSize = 10;
    [ObservableProperty] private int totalPages;

    // =========================
    // COMMANDS
    // =========================

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        if (_cancellationTokenSource?.Token.IsCancellationRequested == true) return;

        ErrorMessage = null;
        IsLoading = true;

        try
        {
            await using var context = await _dbContextService.CreateDbContextAsync();

            IQueryable<Product> query = context.Products
                .Include(p => p.Category)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                var keyword = SearchKeyword.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(keyword) ||
                    p.Sku.ToLower().Contains(keyword));
            }

            if (SelectedCategory != null)
            {
                query = query.Where(p =>
                    p.CategoryId == SelectedCategory.CategoryId);
            }

            query = SelectedSort switch
            {
                "Price" => query.OrderBy(p => p.ImportPrice),
                "Stock" => query.OrderBy(p => p.Count),
                _ => query.OrderBy(p => p.Name)
            };

            var totalItems = await query.CountAsync(_cancellationTokenSource?.Token ?? default);
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var items = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync(_cancellationTokenSource?.Token ?? default);

            Products.Clear();
            foreach (var p in items)
                Products.Add(p);
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            // Ignore if cancelled or disposed
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
        if (SelectedProduct == null || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;

        try
        {
            await using var context = await _dbContextService.CreateDbContextAsync();
            context.Products.Add(SelectedProduct);
            await context.SaveChangesAsync(_cancellationTokenSource?.Token ?? default);
            await LoadProductsAsync();
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            // Ignore
        }
    }

    [RelayCommand]
    private async Task EditProductAsync()
    {
        if (SelectedProduct == null || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;

        try
        {
            await using var context = await _dbContextService.CreateDbContextAsync();
            context.Products.Update(SelectedProduct);
            await context.SaveChangesAsync(_cancellationTokenSource?.Token ?? default);
            await LoadProductsAsync();
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            // Ignore
        }
    }

    [RelayCommand]
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null || _cancellationTokenSource?.Token.IsCancellationRequested == true) return;

        try
        {
            await using var context = await _dbContextService.CreateDbContextAsync();
            context.Products.Remove(SelectedProduct);
            await context.SaveChangesAsync(_cancellationTokenSource?.Token ?? default);
            await LoadProductsAsync();
        }
        catch (DbUpdateException)
        {
            ErrorMessage = "Product is referenced by orders.";
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            // Ignore
        }
    }

    // =========================
    // INIT
    // =========================

    private async Task SafeInitializeAsync()
    {
        try
        {
            await InitializeAsync();
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            // Ignore if app is closing
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in InitializeAsync: {ex.Message}");
            ErrorMessage = "Không thể tải dữ liệu ban đầu.";
        }
    }

    private async Task InitializeAsync()
    {
        if (_cancellationTokenSource?.Token.IsCancellationRequested == true) return;

        await LoadCategoriesAsync();
        await LoadProductsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        if (_cancellationTokenSource?.Token.IsCancellationRequested == true) return;

        try
        {
            await using var context = await _dbContextService.CreateDbContextAsync();

            var list = await context.Categories
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync(_cancellationTokenSource?.Token ?? default);

            Categories.Clear();
            foreach (var c in list)
                Categories.Add(c);
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            // Ignore
        }
    }

    // Dispose để cancel tất cả operations
    public void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}
