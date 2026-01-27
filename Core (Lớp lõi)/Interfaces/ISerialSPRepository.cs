using Core.Entities;
namespace Core.Interfaces
{
    public interface ISerialSPRepository : IGenericRepository<SerialSP>
    {
        bool CheckTonKho(string serial); // Kiểm tra xem serial này còn trong kho không
        void UpdateTrangThai(string serial, string trangThaiMoi);
    }
}