using Core.Entities;
using Core.Interfaces;
using Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class PhanQuyenRepository : GenericRepository<PhanQuyen>, IPhanQuyenRepository
    {
        public PhanQuyenRepository(QuanLyKhoContext context) : base(context)
        {
        }

        public IEnumerable<PhanQuyen> GetQuyenByNhom(string maNhomQuyen)
        {
            // Lấy danh sách quyền + kèm theo thông tin chi tiết của Chức năng (Include)
            return _context.PhanQuyen
                           .Include(pq => pq.ChucNang)
                           .Where(pq => pq.MaNhomQuyen == maNhomQuyen)
                           .ToList();
        }
    }
}