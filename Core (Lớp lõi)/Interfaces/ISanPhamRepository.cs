using Core.Entities;
namespace Core.Interfaces
{
    public interface ISanPhamRepository : IGenericRepository<SanPham>
    {
        // Ví dụ: Tìm hàng sắp hết, tìm theo danh mục...
        IEnumerable<SanPham> GetByDanhMuc(string maDM);
    }
}