using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.EntityFrameworkCore;
using myshop_data.Data;
using myshop_data.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace myshop.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public ProductsViewModel(
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;

        Products = new ObservableCollection<Product>();
        Categories = new ObservableCollection<Category>();

        _ = InitializeAsync();
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
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            await using var context =
                await _dbContextFactory.CreateDbContextAsync();

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

            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var items = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            Products.Clear();
            foreach (var p in items)
                Products.Add(p);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
        if (SelectedProduct == null) return;

        await using var context =
            await _dbContextFactory.CreateDbContextAsync();

        context.Products.Add(SelectedProduct);
        await context.SaveChangesAsync();
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task EditProductAsync()
    {
        if (SelectedProduct == null) return;

        await using var context =
            await _dbContextFactory.CreateDbContextAsync();

        context.Products.Update(SelectedProduct);
        await context.SaveChangesAsync();
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;

        try
        {
            await using var context =
                await _dbContextFactory.CreateDbContextAsync();

            context.Products.Remove(SelectedProduct);
            await context.SaveChangesAsync();
            await LoadProductsAsync();
        }
        catch (DbUpdateException)
        {
            ErrorMessage = "Product is referenced by orders.";
        }
    }

    // =========================
    // INIT
    // =========================

    private async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        await using var context =
            await _dbContextFactory.CreateDbContextAsync();

        var list = await context.Categories
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

        Categories.Clear();
        foreach (var c in list)
            Categories.Add(c);
    }
}
