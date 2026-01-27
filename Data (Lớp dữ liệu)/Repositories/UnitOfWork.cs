using Core.Entities;
using Core.Interfaces;
using Data.Context;


namespace Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QuanLyKhoContext _context;

        public UnitOfWork(QuanLyKhoContext context)
        {
            _context = context;
            // Khởi tạo các Repository
            NguoiDung = new NguoiDungRepository(_context);
            SanPham = new SanPhamRepository(_context);
            SerialSP = new SerialSPRepository(_context);
            HoaDon = new HoaDonRepository(_context);
            PhieuNhap = new PhieuNhapRepository(_context);
            KhachHang = new KhachHangRepository(_context);
            PhanQuyen = new PhanQuyenRepository(_context);

            // Các bảng phụ
            DanhMuc = new GenericRepository<DanhMuc>(_context);
            NhomQuyen = new GenericRepository<NhomQuyen>(_context);
            ChucNang = new GenericRepository<ChucNang>(_context);
            ChiNhanh = new GenericRepository<ChiNhanh>(_context);
            CT_HoaDon = new GenericRepository<CT_HoaDon>(_context);
            CT_PhieuNhap = new GenericRepository<CT_PhieuNhap>(_context);
        }

        public INguoiDungRepository NguoiDung { get; private set; }
        public ISanPhamRepository SanPham { get; private set; }
        public ISerialSPRepository SerialSP { get; private set; }
        public IHoaDonRepository HoaDon { get; private set; }
        public IPhieuNhapRepository PhieuNhap { get; private set; }
        public IKhachHangRepository KhachHang { get; private set; }
        public IPhanQuyenRepository PhanQuyen { get; private set; }

        public IGenericRepository<DanhMuc> DanhMuc { get; private set; }
        public IGenericRepository<NhomQuyen> NhomQuyen { get; private set; }
        public IGenericRepository<ChucNang> ChucNang { get; private set; }
        public IGenericRepository<ChiNhanh> ChiNhanh { get; private set; }
        public IGenericRepository<CT_HoaDon> CT_HoaDon { get; private set; }
        public IGenericRepository<CT_PhieuNhap> CT_PhieuNhap { get; private set; }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}