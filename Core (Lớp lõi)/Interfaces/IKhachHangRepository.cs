using Core.Entities;
namespace Core.Interfaces
{
    public interface IKhachHangRepository : IGenericRepository<KhachHang>
    {
        KhachHang GetBySDT(string sdt);
    }
}