using Core.Entities;
using Core.Interfaces;
using Data.Context;

namespace Data.Repositories
{
    public class PhieuNhapRepository : GenericRepository<PhieuNhap>, IPhieuNhapRepository
    {
        public PhieuNhapRepository(QuanLyKhoContext context) : base(context)
        {
        }
    }
}