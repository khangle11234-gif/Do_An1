using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Chi_Nhanh")]
    public class ChiNhanh
    {
        [Key]
        [StringLength(10)]
        public string MaCN { get; set; }

        [Required]
        [StringLength(100)]
        public string TenCN { get; set; }

        [StringLength(15)]
        public string SDT { get; set; }

        [StringLength(200)]
        public string DiaChi { get; set; }
    }
}