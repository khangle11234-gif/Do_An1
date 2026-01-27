using Business.DTOs.Input;

namespace Business.Interfaces
{
    public interface ISaleService
    {
        // Hàm bán hàng: Trả về Mã hóa đơn (string) nếu thành công
        string TaoHoaDon(CreateHoaDonRequest request);
    }
}