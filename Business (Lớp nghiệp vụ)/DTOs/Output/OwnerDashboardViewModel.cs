using Core.Entities;
using System.Collections.Generic;

namespace Business.DTOs.Output
{
    public class OwnerDashboardViewModel
    {
        public List<NguoiDung> DanhSachNhanVien { get; set; }
        public List<SanPham> DSSanPham { get; set; }

        // BỔ SUNG THÊM DÒNG NÀY ĐỂ CHỨA NHÀ CUNG CẤP
        public List<NhaCungCap> DSNhaCungCap { get; set; }
    }
}