namespace Business.DTOs.Input
{
    public class CreateSanPhamRequest
    {
        public string MaSP { get; set; }
        public string MaDM { get; set; }
        public string TenSP { get; set; }
        public string DonViTinh { get; set; }
        public decimal? GiaNhap { get; set; }
        public decimal? GiaBan { get; set; }
        public int? ThoiGianBaoHanh { get; set; }
        public string HinhAnh { get; set; }
    }
}