using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddScoped<DapperContext>();
builder.Services.AddScoped<IBranchesService, BranchesService>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<IColorsService, ColorsService>();
builder.Services.AddScoped<ICustomersService, CustomersService>();
builder.Services.AddScoped<IDiscountProductsService, DiscountProductsService>();
builder.Services.AddScoped<IDiscountsService, DiscountsService>();
builder.Services.AddScoped<IPaymentMethodsService, PaymentMethodsService>();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<IProductTypesService, ProductTypesService>();
builder.Services.AddScoped<IPurchaseInvoiceItemsService, PurchaseInvoiceItemsService>();
builder.Services.AddScoped<IPurchaseInvoicesService, PurchaseInvoicesService>();
builder.Services.AddScoped<IPurchaseItemsService, PurchaseItemsService>();
builder.Services.AddScoped<ISalesInvoiceItemsService, SalesInvoiceItemsService>();
builder.Services.AddScoped<ISalesInvoicesService, SalesInvoicesService>();
builder.Services.AddScoped<ISalesItemsService, SalesItemsService>();
builder.Services.AddScoped<ISizesService, SizesService>();
builder.Services.AddScoped<ISuppliersService, SuppliersService>();
builder.Services.AddScoped<IUserBranchesService, UserBranchesService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IUserTypesService, UserTypesService>();
builder.Services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<IInventoryMovementService, InventoryMovementService>();
builder.Services.AddScoped<IBranchInventoryService, BranchInventoryService>();
builder.Services.AddScoped<IExpensesService, ExpensesService>();



builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Access/Login";
        option.AccessDeniedPath = "/Access/AccessDenied";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");

app.Run();
