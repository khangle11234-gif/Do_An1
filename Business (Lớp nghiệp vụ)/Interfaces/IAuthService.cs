using Business.DTOs.Input;  // Đã sửa namespace
using Business.DTOs.Output; // Đã sửa namespace

namespace Business.Interfaces
{
    public interface IAuthService
    {
        UserViewModel Login(LoginRequest request);
        bool ChangePassword(string maND, string oldPass, string newPass);
    }
}