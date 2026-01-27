using Core.Entities;
namespace Core.Interfaces
{
    public interface INguoiDungRepository : IGenericRepository<NguoiDung>
    {
        NguoiDung CheckLogin(string username, string password);
    }
}