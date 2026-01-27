using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("LichSuHeThong")]
    public class LichSuHeThong
    {
        [Key]
        public int Id { get; set; }
        public string? NguoiThucHien { get; set; }
        public string? HanhDong { get; set; }
        public DateTime ThoiGian { get; set; }
    }
}