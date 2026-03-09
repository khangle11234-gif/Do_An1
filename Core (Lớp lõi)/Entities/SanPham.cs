using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("San_Pham")]
    public class SanPham
    {
        [Key]
        [StringLength(10)]
        public string MaSP { get; set; }

        // [SỬA] Bỏ required, cho phép null nếu database cho phép
        [StringLength(20)]
        public string? MaDM { get; set; }

        [StringLength(200)]
        public string TenSP { get; set; }

        public bool ? TrangThai { get; set; } = true;

        public string? MaVach { get; set; }

        [StringLength(20)]
        public string? DonViTinh { get; set; }

        public decimal? DonGiaNhap { get; set; }
        public int? SoLuongTon { get; set; }

        public decimal? GiaNhap { get; set; }
        public decimal? GiaBan { get; set; }
        public int? ThoiGianBaoHanh { get; set; }
        public string? HinhAnh { get; set; }

        // --- QUAN HỆ ---

        // [SỬA] Thêm dấu ? để báo hiệu mối quan hệ này là không bắt buộc (Optional)
        [ForeignKey("MaDM")]
        public virtual DanhMuc? DanhMuc { get; set; }

        public virtual ICollection<SerialSP> SerialSPs { get; set; }
    }
}