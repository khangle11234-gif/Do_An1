using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("ThongTinCongTy")]
    public class ThongTinCongTy
    {
        [Key]
        public int Id { get; set; }
        public string? TenCongTy { get; set; }
        public string? MaSoThue { get; set; }
        public string? DiaChi { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public bool? ChoPhepBanHang { get; set; }
    }
}