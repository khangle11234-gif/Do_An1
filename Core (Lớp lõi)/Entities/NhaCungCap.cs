using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Nha_Cung_Cap")]
    public class NhaCungCap
    {
        [Key]
        [StringLength(10)]
        public string MaNCC { get; set; }

        [StringLength(100)]
        public string TenNCC { get; set; }

        [StringLength(15)]
        public string SDT { get; set; }

        [StringLength(200)]
        public string DiaChi { get; set; }

        public decimal? CongNoPhaiTra { get; set; }
    }
}