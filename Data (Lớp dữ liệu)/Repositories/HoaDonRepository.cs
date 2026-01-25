using Core.Entities;
using Core.Interfaces;
using Data.Context;

namespace Data.Repositories
{
    public class HoaDonRepository : GenericRepository<HoaDon>, IHoaDonRepository
    {
        public HoaDonRepository(QuanLyKhoContext context) : base(context)
        {
        }

        // Hiện tại chưa có nghiệp vụ phức tạp nào khác ngoài CRUD cơ bản
        // Nếu sau này cần tính doanh thu, bạn sẽ viết hàm vào đây
    }
}