namespace Business.DTOs.Input
{
    public class CreateHoaDonRequest
    {
        public string MaND { get; set; } // Nhân viên bán
        public string MaKH { get; set; } // Khách mua
        public string MaKho { get; set; }
        public string PhuongThucTT { get; set; } // Tiền mặt/CK

        // Khi bán hàng theo Serial, ta chỉ cần gửi lên danh sách Serial
        public List<string> DanhSachMaSerial { get; set; }
    }
}