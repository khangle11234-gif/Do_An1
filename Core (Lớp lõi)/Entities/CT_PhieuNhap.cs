
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("CT_Phieu_Nhap")]
    public class CT_PhieuNhap
    {
        [Key, Column(Order = 0)]
        [StringLength(20)]
        public string MaPN { get; set; }

        [Key, Column(Order = 1)]
        [StringLength(10)]
        public string MaSP { get; set; }

        public int? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? ThanhTien { get; set; }

        [ForeignKey("MaPN")]
        public virtual PhieuNhap PhieuNhap { get; set; }

        [ForeignKey("MaSP")]
        public virtual SanPham SanPham { get; set; }
    }
}