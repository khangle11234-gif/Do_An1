using Business.DTOs.Input;
using Business.DTOs.Output;

namespace Business.Interfaces
{
    public interface IProductService
    {
        IEnumerable<SanPhamViewModel> GetAll();
        IEnumerable<SanPhamViewModel> Search(string keyword);
        void Create(CreateSanPhamRequest request); // Input dùng DTO
    }
}