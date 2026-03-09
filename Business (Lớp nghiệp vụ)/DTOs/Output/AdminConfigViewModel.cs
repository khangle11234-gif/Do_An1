using System.Collections.Generic;
using Core.Entities; // Dòng này để nhận diện các bảng: NguoiDung, SanPham...

namespace Business.DTOs.Output
{
    // Đây là cái "túi" chứa mọi dữ liệu hiển thị trên Dashboard
    public class AdminDashboardViewModel
    {
        // 1. Dữ liệu cũ (Cấu hình, Nhân sự, Log)
        public ThongTinCongTy CongTy { get; set; }
        public List<NguoiDung> DanhSachNhanVien { get; set; }
        public List<LichSuHeThong> LichSu { get; set; }

        // 2. Dữ liệu MỚI (Dành cho Tab Nhập Hàng)
        public List<NhaCungCap> DSNhaCungCap { get; set; }
        public List<SanPham> DSSanPham { get; set; }
    }
}