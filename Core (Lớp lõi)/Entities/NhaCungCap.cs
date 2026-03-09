using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Nha_Cung_Cap")]
public class NhaCungCap
{
    [Key]
    [StringLength(10)]
    public string MaNCC { get; set; } // Khóa chính không cần ?

    [StringLength(100)]
    public string TenNCC { get; set; }

    [StringLength(15)]
    public string? SDT { get; set; } // Thêm ?

    [StringLength(200)]
    public string? DiaChi { get; set; } // Thêm ?

    public string? GhiChu { get; set; } // Thêm ?

    public bool? TrangThai { get; set; } = true;
    public decimal? CongNoPhaiTra { get; set; }
}