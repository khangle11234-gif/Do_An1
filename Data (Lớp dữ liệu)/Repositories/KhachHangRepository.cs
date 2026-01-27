using Core.Entities;
using Core.Interfaces;
using Data.Context;

namespace Data.Repositories
{
    public class KhachHangRepository : GenericRepository<KhachHang>, IKhachHangRepository
    {
        public KhachHangRepository(QuanLyKhoContext context) : base(context)
        {
        }

        public KhachHang GetBySDT(string sdt)
        {
            // Tìm khách hàng dựa trên số điện thoại
            return _context.KhachHang.FirstOrDefault(kh => kh.SDT == sdt);
        }
    }
}