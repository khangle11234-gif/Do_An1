using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("CT_Hoa_Don")]
    public class CT_HoaDon
    {
        // Khóa chính phức hợp (Composite Key) sẽ cấu hình trong DbContext sau
        [Key, Column(Order = 0)]
        [StringLength(20)]
        public string MaHD { get; set; }

        [Key, Column(Order = 1)]
        [StringLength(50)]
        public string MaSerial { get; set; }

        public decimal? GiaBanThucTe { get; set; }

        // --- QUAN HỆ ---
        [ForeignKey("MaHD")]
        public virtual HoaDon HoaDon { get; set; }

        [ForeignKey("MaSerial")]
        public virtual SerialSP SerialSP { get; set; }
        // Thêm 2 dòng này vào để bán hàng theo số lượng
        public string MaSP { get; set; }
        public int ? SoLuong { get; set; }
      
    }
}