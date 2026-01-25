using Core.Entities;
namespace Core.Interfaces
{
    public interface IPhanQuyenRepository : IGenericRepository<PhanQuyen>
    {
        // Lấy danh sách quyền của 1 nhóm
        IEnumerable<PhanQuyen> GetQuyenByNhom(string maNhomQuyen);
    }
}