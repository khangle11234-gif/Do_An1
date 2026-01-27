using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("SerialSP")]
    public class SerialSP
    {
        [Key]
        [StringLength(10)]
        public string MaSerial { get; set; }

        [StringLength(10)]
        public string MaSP { get; set; }

        [StringLength(10)]
        public string MaCN { get; set; }

        [Required]
        [StringLength(50)]
        public string SoSerial { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } // 'TonKho', 'DaBan'

        // --- QUAN HỆ ---
        [ForeignKey("MaSP")]
        public virtual SanPham SanPham { get; set; }

        [ForeignKey("MaCN")]
        public virtual ChiNhanh ChiNhanh { get; set; }
    }
}