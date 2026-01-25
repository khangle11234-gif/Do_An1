using Core.Entities;
using Core.Interfaces;
using Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class SanPhamRepository : GenericRepository<SanPham>, ISanPhamRepository
    {
        public SanPhamRepository(QuanLyKhoContext context) : base(context)
        {
        }

        public IEnumerable<SanPham> GetByDanhMuc(string maDM)
        {
            // Lấy sản phẩm theo mã danh mục
            return _context.SanPham
                           .Where(sp => sp.MaDM == maDM)
                           .ToList();
        }
    }
}