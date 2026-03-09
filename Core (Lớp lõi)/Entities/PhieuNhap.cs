using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Phieu_Nhap")]
    public class PhieuNhap
    {
        [Key]
        [StringLength(20)]
        public string MaPN { get; set; }

        [StringLength(10)]
        public string MaND { get; set; }

        [StringLength(10)]
        public string MaNCC { get; set; }

        [StringLength(10)]
        public string MaCN { get; set; }

        public DateTime NgayNhap { get; set; } = DateTime.Now;
        public decimal TongTien { get; set; }

        [NotMapped]
        public int TrangThai { get; set; }

        [NotMapped]
        public string NguoiDuyet { get; set; }

        [NotMapped]
        public DateTime? NgayDuyet { get; set; }

        [StringLength(200)]
        public string GhiChu { get; set; }

        [ForeignKey("MaND")]
        public virtual NguoiDung NguoiDung { get; set; }

        [ForeignKey("MaNCC")]
        public virtual NhaCungCap NhaCungCap { get; set; }


        [ForeignKey("MaCN")]
        public virtual ChiNhanh ChiNhanh { get; set; }

        public virtual ICollection<CT_PhieuNhap> CT_PhieuNhaps { get; set; }
    }
}