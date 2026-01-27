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

       public UserViewModel Login(LoginRequest request)
{
    // Gọi xuống Data (Repository)
    var user = _unitOfWork.NguoiDung.CheckLogin(request.TenDangNhap, request.MatKhau);
    
    if (user == null) return null;

    // Trả về DTO kèm Vai Trò
    return new UserViewModel
    {
        MaND = user.MaND.ToString(),
        HoTen = user.HoTen ?? "Người dùng",
        VaiTro = user.VaiTro ?? "Staff" // Nếu null thì coi là Staff
    };
}

        public bool ChangePassword(string maND, string oldPass, string newPass)
        {
            return true; // Logic xử lý sau
        }
    }
}