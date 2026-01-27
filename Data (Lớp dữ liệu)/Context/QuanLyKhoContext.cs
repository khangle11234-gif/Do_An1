using Microsoft.EntityFrameworkCore;
using Core.Entities;

namespace Data.Context
{
    public class QuanLyKhoContext : DbContext
    {
        // 1. Constructor mặc định
        public QuanLyKhoContext() { }

        // 2. Constructor nhận cấu hình (Để sau này Web tiêm kết nối vào)
        public QuanLyKhoContext(DbContextOptions<QuanLyKhoContext> options) : base(options) { }

        // 3. Khai báo các bảng dữ liệu (DbSet)
        // Nhóm Quản Trị
        public DbSet<NguoiDung> NguoiDung { get; set; }
        public DbSet<NhomQuyen> NhomQuyen { get; set; }
        public DbSet<ChucNang> ChucNang { get; set; }
        public DbSet<PhanQuyen> PhanQuyen { get; set; }

        // Nhóm Kho & Sản Phẩm
        public DbSet<SanPham> SanPham { get; set; }
        public DbSet<SerialSP> SerialSP { get; set; }
        public DbSet<DanhMuc> DanhMuc { get; set; }
        public DbSet<ChiNhanh> ChiNhanh { get; set; }

        // Nhóm Giao Dịch
        public DbSet<PhieuNhap> PhieuNhap { get; set; }
        public DbSet<CT_PhieuNhap> CT_PhieuNhap { get; set; }
        public DbSet<HoaDon> HoaDon { get; set; }
        public DbSet<CT_HoaDon> CT_HoaDon { get; set; }

        // Nhóm Đối Tác & Hậu Mãi
        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<NhaCungCap> NhaCungCap { get; set; }
        public DbSet<PhieuBaoHanh> PhieuBaoHanh { get; set; }
        public DbSet<Core.Entities.ThongTinCongTy> ThongTinCongTy { get; set; }
        public DbSet<Core.Entities.LichSuHeThong> LichSuHeThong { get; set; }

        // 4. Cấu hình kết nối (Chuỗi kết nối)
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Chuỗi kết nối lấy từ script SQL của bạn (Server=MSI, DB=QuanLyKho)
                optionsBuilder.UseSqlServer("Data Source=MSI;Initial Catalog=QuanLyKho;Integrated Security=True;TrustServerCertificate=True");
            }
        }

        // 5. Cấu hình nâng cao (Fluent API)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Khóa Chính Phức Hợp (Composite Key) cho các bảng nhiều nhiều
            // Bảng Phân Quyền (Khóa là MaNhomQuyen + MaChucNang)
            modelBuilder.Entity<PhanQuyen>()
                .HasKey(p => new { p.MaNhomQuyen, p.MaChucNang });

            // Bảng Chi Tiết Phiếu Nhập (Khóa là MaPN + MaSP)
            modelBuilder.Entity<CT_PhieuNhap>()
                .HasKey(c => new { c.MaPN, c.MaSP });

            // Bảng Chi Tiết Hóa Đơn (Khóa là MaHD + MaSerial)
            // Lưu ý: Bảng này bán theo Serial chứ không theo Sản phẩm chung chung
            modelBuilder.Entity<CT_HoaDon>()
                .HasKey(c => new { c.MaHD, c.MaSerial });

            // Cấu hình xóa đệ quy (Optional): Tránh lỗi khi xóa dữ liệu cha
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}