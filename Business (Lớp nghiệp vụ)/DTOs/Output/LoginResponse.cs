namespace Business.DTOs.Output
{
    public class LoginResponse
    {
        public string MaND { get; set; }
        public string HoTen { get; set; }
        public string TenNhomQuyen { get; set; }
        public string Token { get; set; } // Để dành cho xác thực sau này
    }
}