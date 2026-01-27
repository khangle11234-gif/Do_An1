using Microsoft.AspNetCore.Mvc;
using Business.DTOs.Output; // <--- QUAN TRỌNG: Gọi class từ tầng Business

namespace Presentation.Controllers
{
    public class SaleController : Controller
    {
        // GET: /Sale/Index
        public IActionResult Index()
        {
            // 1. Kiểm tra đăng nhập (Bảo mật)
            if (HttpContext.Session.GetString("MaND") == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // 2. Tạo dữ liệu giả (Dummy Data) chuẩn ngành Vật tư
            // Lưu ý: Class SanPhamViewModel này được lấy từ Business.DTOs.Output
            // --- DỮ LIỆU GIẢ: VẬT TƯ & KHO HÀNG (ĐÃ SỬA LINK ẢNH) ---
            var dummyProducts = new List<SanPhamViewModel>
{
    new SanPhamViewModel {
        MaSP = "XM01",
        TenSP = "Xi măng Hà Tiên (Bao 50kg)",
        GiaBan = 85000, 
        // Ảnh màu xám, có chữ Xi Mang
        HinhAnh = "https://vlxdnambinhduong.com/wp-content/uploads/2024/03/Vicem-ha-tien.jpg",
        IsYeuThich = true,
        NhomHang = "VLXD"
    },
    new SanPhamViewModel {
        MaSP = "THEP01",
        TenSP = "Thép cuộn Hòa Phát Ø6",
        GiaBan = 16500, 
        // Ảnh màu xanh đen, có chữ Thep
        HinhAnh = "https://hoathuong.vn/wp-content/uploads/2022/03/hp-phi6-598x400.png",
        IsYeuThich = true,
        NhomHang = "VLXD"
    },
    new SanPhamViewModel {
        MaSP = "MK01",
        TenSP = "Máy khoan cầm tay Bosch GSB",
        GiaBan = 1250000, 
        // Ảnh màu xanh lá, có chữ Bosch
        HinhAnh = "https://placehold.co/300x300/27ae60/FFFFFF?text=May+Khoan+Bosch",
        IsYeuThich = false,
        NhomHang = "DungCu"
    },
    new SanPhamViewModel {
        MaSP = "SON01",
        TenSP = "Sơn Dulux Trắng (Thùng 18L)",
        GiaBan = 2100000, 
        // Ảnh màu xanh dương, có chữ Dulux
        HinhAnh = "https://placehold.co/300x300/2980b9/FFFFFF?text=Son+Dulux",
        IsYeuThich = false,
        NhomHang = "Son"
    },
    new SanPhamViewModel {
        MaSP = "DAY01",
        TenSP = "Dây điện Cadivi 2.5mm (Cuộn)",
        GiaBan = 750000, 
        // Ảnh màu vàng cam, có chữ Cadivi
        HinhAnh = "https://placehold.co/300x300/e67e22/FFFFFF?text=Day+Dien+Cadivi",
        IsYeuThich = true,
        NhomHang = "DienNuoc"
    },
    new SanPhamViewModel {
        MaSP = "ONG01",
        TenSP = "Ống nhựa Bình Minh Ø27",
        GiaBan = 18000, 
        // Để trống để test ảnh mặc định
        HinhAnh = "",
        IsYeuThich = false,
        NhomHang = "DienNuoc"
    }
};

            // 3. Trả về View kèm dữ liệu
            return View(dummyProducts);
        }
    }
}