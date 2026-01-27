using System.ComponentModel.DataAnnotations;

namespace Business.DTOs.Input
{
    public class UserCreateViewModel
    {
        public string? MaND { get; set; } // Để null nếu là thêm mới

        [Required(ErrorMessage = "Vui lòng chọn loại tài khoản")]
        public string VaiTro { get; set; } // Admin, Owner, Sale, Kho

        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; }

        // Mật khẩu có thể để trống nếu đang sửa (không muốn đổi pass)
        public string? MatKhau { get; set; }

        [Compare("MatKhau", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string? NhapLaiMatKhau { get; set; }

        public bool HienThi { get; set; } = true; // Checkbox "Hiển thị"
        [Required(ErrorMessage = "Vui lòng chọn nhóm quyền cho nhân viên")]
        public string MaNhomQuyen { get; set; }
        public string Email { get; set; }
    }
}