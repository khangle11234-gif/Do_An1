namespace Business.DTOs.Input
{
    public class CreatePhieuNhapRequest
    {
        public string MaND { get; set; } // Người nhập
        public string MaNCC { get; set; } // Nhà cung cấp
        public string MaKho { get; set; } // Nhập vào kho nào
        public string GhiChu { get; set; }

        // Danh sách hàng cần nhập
        public List<ChiTietNhapInput> ChiTiet { get; set; }
    }

    public class ChiTietNhapInput
    {
        public string MaSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
}