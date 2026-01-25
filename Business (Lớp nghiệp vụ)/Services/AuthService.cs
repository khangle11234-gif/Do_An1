using Business.Interfaces;
using Business.DTOs.Input;  // Đã sửa namespace
using Business.DTOs.Output; // Đã sửa namespace
using Core.Interfaces;

namespace Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public LoginResponse Login(LoginRequest request)
        {
            // Gọi xuống Data
            var user = _unitOfWork.NguoiDung.CheckLogin(request.TenDangNhap, request.MatKhau);

            if (user == null) return null;

            // Map dữ liệu từ Entity sang DTO Output
            return new LoginResponse
            {
                MaND = user.MaND,
                HoTen = user.HoTen,
                TenNhomQuyen = user.MaNhomQuyen // Tạm thời lấy mã, sau này Include lấy tên sau
            };
        }

        public bool ChangePassword(string maND, string oldPass, string newPass)
        {
            return true; // Logic xử lý sau
        }
    }
}