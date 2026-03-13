using Microsoft.EntityFrameworkCore;
using Data.Context;
using Core.Interfaces;
using Data.Repositories;
using Business.Interfaces;
using Business.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Kết nối Database
builder.Services.AddDbContext<QuanLyKhoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký Data Layer
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<INguoiDungRepository, NguoiDungRepository>();
builder.Services.AddScoped<ISerialSPRepository, SerialSPRepository>();
// ... (Thêm các repo khác nếu cần)

// 3. Đăng ký Business Layer
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ISaleService, SaleService>();

// 4. Kích hoạt Session & MVC
builder.Services.AddSession();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(); // Bật tính năng Real-time

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession(); // Quan trọng

// Cấu hình mặc định: Chạy vào HomeController -> Action Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<Web.Hubs.StoreHub>("/storeHub"); // Định tuyến trạm phát sóng (Nhớ using Web.Hubs trên cùng nếu nó báo lỗi)

app.Run();