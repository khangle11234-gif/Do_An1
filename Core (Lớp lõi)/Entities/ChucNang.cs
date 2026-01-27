
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Chuc_Nang")]
    public class ChucNang
    {
        [Key]
        [StringLength(50)]
        public string MaChucNang { get; set; } // Ví dụ: "BTN_THEM_HANG"

        [Required]
        [StringLength(100)]
        public string TenChucNang { get; set; }

        [StringLength(50)]
        public string NhomChucNang { get; set; } // Ví dụ: "Quản lý kho"

        public virtual ICollection<PhanQuyen> PhanQuyens { get; set; }
    }
}