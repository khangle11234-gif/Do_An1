using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Nguoi_Dung")] // Báo cho code biết tên bảng trong SQL là có dấu gạch dưới
    public class NguoiDung
    {
        [Key] // Đây là khóa chính
        [StringLength(10)]
        public string MaND { get; set; }

        [StringLength(10)]
        public string MaCN { get; set; } // Khóa ngoại

        [StringLength(20)]
        public string MaNhomQuyen { get; set; } // Khóa ngoại

        [Required] // Bắt buộc nhập (Not Null)
        [StringLength(50)]
        public string TenDangNhap { get; set; }

        [Required]
        [StringLength(255)]
        public string MatKhau { get; set; }

        [StringLength(100)]
        public string HoTen { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        public bool? TrangThai { get; set; } // SQL là bit -> C# là bool

        // --- KHAI BÁO MỐI QUAN HỆ (Foreign Key) ---

        [ForeignKey("MaCN")]
        public virtual ChiNhanh ChiNhanh { get; set; }

        [ForeignKey("MaNhomQuyen")]
        public virtual NhomQuyen NhomQuyen { get; set; }
        public string? VaiTro { get; set; }
    }
}