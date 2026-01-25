using Core.Entities;

namespace Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        INguoiDungRepository NguoiDung { get; }
        ISanPhamRepository SanPham { get; }
        ISerialSPRepository SerialSP { get; }
        IHoaDonRepository HoaDon { get; }
        IPhieuNhapRepository PhieuNhap { get; }
        IKhachHangRepository KhachHang { get; }
        IPhanQuyenRepository PhanQuyen { get; }

        // Các bảng phụ dùng Generic trực tiếp
        IGenericRepository<DanhMuc> DanhMuc { get; }
        IGenericRepository<NhomQuyen> NhomQuyen { get; }
        IGenericRepository<ChucNang> ChucNang { get; }
        IGenericRepository<ChiNhanh> ChiNhanh { get; }
        IGenericRepository<CT_HoaDon> CT_HoaDon { get; }
        IGenericRepository<CT_PhieuNhap> CT_PhieuNhap { get; }

        int Complete(); // Hàm SaveChanges
    }
}