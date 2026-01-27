using Core.Entities;
using Core.Interfaces;
using Data.Context;

namespace Data.Repositories
{
    public class NguoiDungRepository : GenericRepository<NguoiDung>, INguoiDungRepository
    {
        private readonly QuanLyKhoContext _context;
        public NguoiDungRepository(QuanLyKhoContext context) : base(context) {
            _context = context;
        }

        public NguoiDung CheckLogin(string username, string password)
        {
            // Lưu ý: Password nên được mã hóa MD5/SHA256, ở đây làm mẫu so sánh trực tiếp
            return _context.NguoiDung.FirstOrDefault(u => u.TenDangNhap == username && u.MatKhau == password && u.TrangThai == true);
        }
    }
}