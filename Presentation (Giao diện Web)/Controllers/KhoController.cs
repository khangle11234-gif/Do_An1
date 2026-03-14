using Business.DTOs.Output;
using Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Presentation.Controllers
{
    public class KhoController : Controller
    {
        private readonly QuanLyKhoContext _context;

        public KhoController(QuanLyKhoContext context)
        {
            _context = context;
        }

        // Bốc dữ liệu lên giao diện Thủ Kho
        public IActionResult Index()
        {
            // 1. Kiểm tra Quyền: Cửa này chỉ dành cho Thủ kho và Chủ shop
            var role = HttpContext.Session.GetString("VaiTro");
            if (role != "Kho" && role != "Owner")
                return RedirectToAction("Index", "Home");

            // 2. Lấy dữ liệu Sản phẩm (để nhập hàng/kiểm kê) & Nhà cung cấp (để tạo phiếu nhập)
            var dsSanPham = _context.SanPham.OrderByDescending(sp => sp.MaSP).ToList();
            var dsNhaCungCap = _context.NhaCungCap.ToList();

            // Lấy thông tin cửa hàng để trang trí
            ViewBag.ThemeConfig = _context.CauHinhHeThong.FirstOrDefault(c => c.Id == 1)
                               ?? new Core.Entities.CauHinhHeThong { TenCuaHang = "SMART WAREHOUSE", MauChuDao = "#2c3e50", MauNhan = "#3498db" };

            // 3. Đóng gói vào Model và gửi sang View
            var model = new OwnerDashboardViewModel
            {
                DSSanPham = dsSanPham,
                DSNhaCungCap = dsNhaCungCap
            };

            return View(model);
        }
    }
}