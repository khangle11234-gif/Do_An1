using System;
using System.Collections.Generic;

namespace Business.DTOs.Input
{
    public class PhieuNhapInputModel
    {
        public string MaNCC { get; set; }
        public string GhiChu { get; set; }
        public DateTime NgayNhap { get; set; }

        // Danh sách chi tiết hàng hóa
        public List<ChiTietNhapItem> ChiTiet { get; set; }
    }

    public class ChiTietNhapItem
    {
        public string MaSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; } // Giá nhập
    }
}