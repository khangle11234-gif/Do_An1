using Core.Entities;

namespace Business.DTOs.Output
{
    public class AdminDashboardViewModel
    {
        // Phần 1: Để hứng thông tin công ty
        public ThongTinCongTy CongTy { get; set; }

        // Phần 2: Để chứa danh sách nhân viên
        public List<NguoiDung> DanhSachNhanVien { get; set; }
        public List<LichSuHeThong> LichSu { get; set; }
        public List<NhaCungCap> DSNhaCungCap { get; set; }
        public List<SanPham> DSSanPham { get; set; }
    }
}