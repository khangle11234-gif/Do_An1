
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Hoa_Don")]
    public class HoaDon
    {
        [Key]
        [StringLength(20)]
        public string MaHD { get; set; }

        [StringLength(10)]
        public string MaND { get; set; }

        [StringLength(10)]
        public string MaCN { get; set; }

        [StringLength(10)]
        public string MaKH { get; set; }

        public DateTime? NgayLap { get; set; }
        public decimal? TongTienHang { get; set; }
        public decimal? TienGiam { get; set; }
        public decimal? TongThanhToan { get; set; }

        [StringLength(50)]
        public string PhuongThucTT { get; set; }

        // --- QUAN HỆ ---
        [ForeignKey("MaND")]
        public virtual NguoiDung NguoiDung { get; set; }

        [ForeignKey("MaKH")]
        public virtual KhachHang KhachHang { get; set; }

        public virtual ICollection<CT_HoaDon> CT_HoaDons { get; set; }
     
      
    }
}