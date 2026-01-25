
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Phieu_Bao_Hanh")]
    public class PhieuBaoHanh
    {
        [Key]
        [StringLength(20)]
        public string MaPBH { get; set; }

        [StringLength(10)]
        public string MaND { get; set; }

        [StringLength(10)]
        public string MaSerial { get; set; } // Bảo hành theo từng cái

        public DateTime? NgayNhan { get; set; }
        public DateTime? NgayTraDuKien { get; set; }

        [StringLength(200)]
        public string LoiGhiNhan { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; }

        [ForeignKey("MaND")]
        public virtual NguoiDung NguoiDung { get; set; }

        [ForeignKey("MaSerial")]
        public virtual SerialSP SerialSP { get; set; }
    }
}