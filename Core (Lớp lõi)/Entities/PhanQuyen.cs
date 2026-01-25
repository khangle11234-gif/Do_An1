using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Phan_Quyen")]
    public class PhanQuyen
    {
        // Khóa chính thứ 1
        [Key, Column(Order = 0)]
        [StringLength(20)]
        public string MaNhomQuyen { get; set; }

        // Khóa chính thứ 2
        [Key, Column(Order = 1)]
        [StringLength(50)]
        public string MaChucNang { get; set; }

        public bool? DuocPhep { get; set; } // Check box: Được dùng hay không?

        [ForeignKey("MaNhomQuyen")]
        public virtual NhomQuyen NhomQuyen { get; set; }

        [ForeignKey("MaChucNang")]
        public virtual ChucNang ChucNang { get; set; }
    }
}