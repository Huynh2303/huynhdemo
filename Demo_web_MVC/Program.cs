using Demo_web_MVC.Data;
using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Repository;
using Demo_web_MVC.Repository.Addresss;
using Demo_web_MVC.Repository.Carts;
using Demo_web_MVC.Repository.Category;
using Demo_web_MVC.Repository.Oder;
using Demo_web_MVC.Repository.Paging;
using Demo_web_MVC.Repository.Payment;
using Demo_web_MVC.Repository.Product;
using Demo_web_MVC.Repository.Search;
using Demo_web_MVC.Service;
using Demo_web_MVC.Service.Address;
using Demo_web_MVC.Service.Cart;
using Demo_web_MVC.Service.Category;
using Demo_web_MVC.Service.Oder;
using Demo_web_MVC.Service.Payment;
using Demo_web_MVC.Service.Product;
using Demo_web_MVC.Service.Search;
using Demo_web_MVC.Service.Sendemail;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NETCore.MailKit.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IPagingReponsitory, PagingReponsitory>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ISearchReponsitory, SearchReponsitory>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IOderService, OderService>();
builder.Services.AddScoped<IOderRepository, OderRepository>();
builder.Services.AddScoped< ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped< IProductService,ProductService>();
builder.Services.AddScoped<IEmailServices, Sendemail>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddDbContext<AppDatabase>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString =  
        builder.Configuration.GetConnectionString("DefaultConnection");

    options.SchemaName = "dbo";
    options.TableName = "CacheTable";
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.AccessDeniedPath = "/User/Denied";
    });

builder.Services.AddAuthorization();


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
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});
app.UseSession();
app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
