using Business.Interfaces;
using Business.DTOs.Input;
using Business.DTOs.Output;
using Core.Entities;
using Core.Interfaces;

namespace Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public IEnumerable<SanPhamViewModel> GetAll()
        {
            var data = _unitOfWork.SanPham.GetAll();

            // Map Entity -> ViewModel
            return data.Select(sp => new SanPhamViewModel
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,

                // SỬA LỖI 1: Thêm "?? 0" 
                // Nghĩa là: Nếu giá bị null thì lấy bằng 0
                GiaBan = sp.GiaBan ?? 0,

                // SỬA LỖI 2: Đổi TenDM thành NhomHang
                // Tạm thời lấy Mã danh mục gán vào Nhóm hàng để lọc
                NhomHang = sp.MaDM,

                // Bổ sung các trường còn thiếu (để tránh lỗi null khi lên View)
                HinhAnh = "", // Tạm thời để trống
                IsYeuThich = false
            }).ToList();
        }

        public void Create(CreateSanPhamRequest request)
        {
            // Map DTO -> Entity
            var sp = new SanPham
            {
                MaSP = request.MaSP,
                TenSP = request.TenSP,
                MaDM = request.MaDM,
                GiaBan = request.GiaBan,
                GiaNhap = request.GiaNhap
            };
            _unitOfWork.SanPham.Add(sp);
            _unitOfWork.Complete();
        }

        public IEnumerable<SanPhamViewModel> Search(string keyword)
        {
            return _unitOfWork.SanPham.Find(x => x.TenSP.Contains(keyword))
                .Select(sp => new SanPhamViewModel { MaSP = sp.MaSP, TenSP = sp.TenSP });
        }
    }
}