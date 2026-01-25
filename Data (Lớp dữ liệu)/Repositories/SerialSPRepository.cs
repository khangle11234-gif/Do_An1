using Core.Entities;
using Core.Interfaces;
using Data.Context;

namespace Data.Repositories
{
    public class SerialSPRepository : GenericRepository<SerialSP>, ISerialSPRepository
    {
        public SerialSPRepository(QuanLyKhoContext context) : base(context) { }

        public bool CheckTonKho(string serial)
        {
            return _context.SerialSP.Any(s => s.SoSerial == serial && s.TrangThai == "TonKho");
        }

        public void UpdateTrangThai(string serial, string trangThaiMoi)
        {
            var item = _context.SerialSP.FirstOrDefault(s => s.SoSerial == serial);
            if (item != null)
            {
                item.TrangThai = trangThaiMoi;
                // Lưu ý: Việc SaveChanges sẽ do UnitOfWork lo
            }
        }
    }
}