
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Nhom_Quyen")]
    public class NhomQuyen
    {
        [Key]
        [StringLength(20)]
        public string MaNhomQuyen { get; set; }

        [Required]
        [StringLength(50)]
        public string TenNhomQuyen { get; set; }

        [StringLength(100)]
        public string? MoTa { get; set; }

        // Quan hệ: Một nhóm quyền có nhiều người dùng
        public virtual ICollection<NguoiDung> NguoiDungs { get; set; }

        // Quan hệ: Một nhóm quyền có bảng phân quyền chi tiết
        public virtual ICollection<PhanQuyen> PhanQuyens { get; set; }
        public DateTime? NgayTao { get; set; }
    }
}