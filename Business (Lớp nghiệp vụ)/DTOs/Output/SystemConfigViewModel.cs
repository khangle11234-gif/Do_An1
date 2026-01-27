namespace Business.DTOs.Output
{
    public class SystemConfigViewModel
    {
        public string TenCongTy { get; set; }
        public string DiaChi { get; set; }
        public string MaSoThue { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public bool ChoPhepBanHang { get; set; } // Nút gạt tắt/bật hệ thống
    }
}