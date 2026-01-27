
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Danh_Muc")]
    public class DanhMuc
    {
        [Key]
        [StringLength(20)]
        public string MaDM { get; set; }

        [StringLength(100)]
        public string TenDM { get; set; }

        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}