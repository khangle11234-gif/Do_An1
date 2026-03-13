using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("CauHinhHeThong")]
    public class CauHinhHeThong
    {
        [Key]
        public int Id { get; set; }
        public string ?TenCuaHang { get; set; }
        public string ?MauChuDao { get; set; }
        public string ?MauNhan { get; set; }
        public string ?MauNen { get; set; }
        [StringLength(500)]
        public string? NganHang { get; set; }
        public string? SoTaiKhoan { get; set; }
        public string? TenChuTaiKhoan { get; set; }
    }
}