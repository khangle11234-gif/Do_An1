namespace Business.DTOs.Output  // <--- Quan trọng nhất là dòng này phải khớp với dòng using của bạn
{
    public class UserViewModel
    {
        public string MaND { get; set; }
        public string HoTen { get; set; }
        public string VaiTro { get; set; } // Chứa: Admin, Owner, Staff
        public string Email { get; set; }
    }
}