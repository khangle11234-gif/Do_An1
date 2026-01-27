using Business.DTOs.Input;
using Business.DTOs.Output;

namespace Business.Interfaces
{
    public interface IInventoryService
    {
        // Hàm nhập hàng: Trả về true nếu thành công, false nếu lỗi
        bool NhapHang(CreatePhieuNhapRequest request);

        // Lấy lịch sử nhập
        IEnumerable<PhieuNhapViewModel> GetLichSuNhap();
    }
}