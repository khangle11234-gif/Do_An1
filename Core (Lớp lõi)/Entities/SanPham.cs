
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

        [StringLength(20)]
        public string MaDM { get; set; }

        [StringLength(200)]
        public string TenSP { get; set; }

        [StringLength(20)]
        public string DonViTinh { get; set; }

        public decimal? GiaNhap { get; set; }
        public decimal? GiaBan { get; set; }
        public int? ThoiGianBaoHanh { get; set; }
        public string HinhAnh { get; set; }

        // --- QUAN HỆ ---

        [ForeignKey("MaDM")]
        public virtual DanhMuc DanhMuc { get; set; }

        // Một sản phẩm có nhiều Serial (Quan hệ 1-N) -> Dùng ICollection
        public virtual ICollection<SerialSP> SerialSPs { get; set; }
        
    }
}