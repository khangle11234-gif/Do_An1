using Business.DTOs.Output;
using Core.Entities;
using Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Controllers
{
    // =========================================================
    // CÁC CLASS HỖ TRỢ NHẬN DỮ LIỆU TỪ GIAO DIỆN (AJAX)
    // =========================================================
    public class ProductToggleRequest
    {
        public string MaSP { get; set; }
    }

    public class PosCheckoutRequest
    {
        public string GhiChu { get; set; }
        public List<PosCartItem> CartItems { get; set; }
    }

    public class PosCartItem
    {
        public string MaSP { get; set; }
        public int SoLuong { get; set; }
        public decimal GiaBan { get; set; }
    }

    public class KiemKeRequest
    {
        public string MaSP { get; set; }
        public int TonThucTe { get; set; }
        public string GhiChu { get; set; }
    }

    public class DeleteSupplierRequest
    {
        public string MaNCC { get; set; }
    }

    public class PhieuNhapRequest
    {
        public string MaNCC { get; set; }
        public DateTime NgayNhap { get; set; }
        public string GhiChu { get; set; }
        public List<PhieuNhapCartItem> ChiTiet { get; set; }
    }

    public class PhieuNhapCartItem
    {
        public string MaSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
    public class DeleteUserRequest
    {
        public string MaND { get; set; }
    }

    // =========================================================
    // BỘ NÃO ĐIỀU KHIỂN CỦA CHỦ SHOP
    // =========================================================
    public class OwnerController : Controller
    {
        private readonly QuanLyKhoContext _context;

        public OwnerController(QuanLyKhoContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1. LOAD GIAO DIỆN CHÍNH
        // =========================================================
        public IActionResult Index()
        {
            // Kiểm tra phân quyền
            var role = HttpContext.Session.GetString("VaiTro");
            if (role != "Owner") return RedirectToAction("Index", "Home");

            // Lấy dữ liệu
            var users = _context.NguoiDung.Where(u => u.VaiTro != "Admin").OrderByDescending(u => u.MaND).ToList();
            var products = _context.SanPham.OrderByDescending(sp => sp.MaSP).ToList();
            var suppliers = _context.NhaCungCap.ToList();

            ViewBag.DanhSachQuyen = _context.NhomQuyen.ToList();

            var model = new OwnerDashboardViewModel
            {
                DanhSachNhanVien = users,
                DSSanPham = products,
                DSNhaCungCap = suppliers
            };
            ViewBag.TongVonTienHang = products.Sum(sp => (sp.SoLuongTon ?? 0) * (sp.GiaNhap ?? 0));
            ViewBag.GiaTriBanUocTinh = products.Sum(sp => (sp.SoLuongTon ?? 0) * (sp.GiaBan ?? 0));
            ViewBag.LoiNhuanUocTinh = ViewBag.GiaTriBanUocTinh - ViewBag.TongVonTienHang;

            return View(model);
        }

        // =========================================================
        // 2. [API] SỬA GIÁ TRỰC TIẾP KHÔNG TẢI LẠI TRANG (AJAX)
        // =========================================================
        [HttpPost]
        public IActionResult UpdateProductAPI([FromBody] SanPham model)
        {
            try
            {
                var sp = _context.SanPham.Find(model.MaSP);
                if (sp == null) return Json(new { success = false, msg = "Không tìm thấy sản phẩm!" });

                sp.TenSP = model.TenSP;
                sp.DonViTinh = model.DonViTinh;
                sp.MaVach = model.MaVach;
                sp.GiaBan = model.GiaBan;
                sp.ThoiGianBaoHanh = model.ThoiGianBaoHanh;
                sp.TrangThai = model.TrangThai;

                _context.SanPham.Update(sp);
                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    msg = "Cập nhật thành công!",
                    giaBanMoi = sp.GiaBan?.ToString("N0") + " đ",
                    tenSPMoi = sp.TenSP,
                    trangThaiMoi = sp.TrangThai
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi Database: " + ex.Message });
            }
        }

        // =========================================================
        // 3. [API] BẬT/TẮT TRẠNG THÁI SẢN PHẨM BẰNG CÔNG TẮC
        // =========================================================
        [HttpPost]
        public IActionResult ToggleProductStatusAPI([FromBody] ProductToggleRequest req)
        {
            try
            {
                var sp = _context.SanPham.Find(req.MaSP);
                if (sp == null) return Json(new { success = false, msg = "Không tìm thấy sản phẩm!" });

                sp.TrangThai = !(sp.TrangThai ?? true);

                _context.SanPham.Update(sp);
                _context.SaveChanges();

                return Json(new { success = true, newStatus = sp.TrangThai });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
        }

        // =========================================================
        // 4. [API] XỬ LÝ THANH TOÁN POS - BÁN SỐ LƯỢNG (VẬT TƯ)
        // =========================================================
        [HttpPost]
        public IActionResult ProcessPosCheckout([FromBody] PosCheckoutRequest req)
        {
            try
            {
                if (req.CartItems == null || !req.CartItems.Any())
                    return Json(new { success = false, msg = "Giỏ hàng trống, không thể thanh toán!" });

                var maND = HttpContext.Session.GetString("MaND") ?? "ADMIN";
                string maHD = "HD" + DateTime.Now.ToString("yyMMddHHmmss");

                // 1. Tạo Hóa Đơn 
                var hoaDon = new HoaDon // Lưu ý: Đổi thành HoaDon nếu class Entity tên là HoaDon
                {
                    MaHD = maHD
                };
                _context.Add(hoaDon);

                // 2. Duyệt qua từng sản phẩm trong Giỏ hàng
                foreach (var item in req.CartItems)
                {
                    var sp = _context.SanPham.Find(item.MaSP);
                    if (sp != null)
                    {
                        if (sp.SoLuongTon < item.SoLuong)
                            return Json(new { success = false, msg = $"Sản phẩm '{sp.TenSP}' chỉ còn {sp.SoLuongTon} cái trong kho!" });

                        sp.SoLuongTon -= item.SoLuong;
                        _context.SanPham.Update(sp);

                        // 3. LƯU CHI TIẾT HÓA ĐƠN
                        var chiTiet = new CT_HoaDon
                        {
                            MaHD = maHD,
                            MaSP = item.MaSP,
                            SoLuong = item.SoLuong,
                            // Sinh mã Serial ảo độc nhất để lách qua khóa chính (nếu có)
                            MaSerial = item.MaSP + "-" + Guid.NewGuid().ToString().Substring(0, 5),
                            GiaBanThucTe = item.GiaBan
                        };
                        _context.Add(chiTiet);
                    }
                }

                _context.SaveChanges();

                return Json(new { success = true, maHD = maHD });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // =========================================================
        // 5. [API] KIỂM KÊ VÀ CÂN BẰNG KHO
        // =========================================================
        [HttpPost]
        public IActionResult KiemKeKhoAPI([FromBody] KiemKeRequest req)
        {
            try
            {
                var sp = _context.SanPham.Find(req.MaSP);
                if (sp == null) return Json(new { success = false, msg = "Không tìm thấy sản phẩm!" });

                sp.SoLuongTon = req.TonThucTe;

                _context.SanPham.Update(sp);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi Database: " + ex.Message });
            }
        }

        // =========================================================
        // 6. [API] QUẢN LÝ NHÀ CUNG CẤP
        // =========================================================
        [HttpPost]
        public IActionResult UpdateSupplierAPI([FromBody] NhaCungCap model)
        {
            try
            {
                var ncc = _context.NhaCungCap.Find(model.MaNCC);
                if (ncc == null) return Json(new { success = false, msg = "Không tìm thấy nhà cung cấp!" });

                ncc.TenNCC = model.TenNCC;
                ncc.SDT = model.SDT;
                ncc.DiaChi = model.DiaChi;
                ncc.GhiChu = model.GhiChu;

                _context.NhaCungCap.Update(ncc);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteSupplierAPI([FromBody] DeleteSupplierRequest req)
        {
            try
            {
                var ncc = _context.NhaCungCap.Find(req.MaNCC);
                if (ncc == null) return Json(new { success = false, msg = "Không tìm thấy nhà cung cấp!" });

                _context.NhaCungCap.Remove(ncc);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, msg = "Không thể xóa vì Nhà cung cấp này đã có dữ liệu Nhập Hàng!" });
            }
        }

        // =========================================================
        // 7. [API] LƯU PHIẾU NHẬP KHO & CỘNG TỒN KHO
        // =========================================================
        [HttpPost]
        public IActionResult SavePhieuNhap([FromBody] PhieuNhapRequest req)
        {
            try
            {
                if (req.ChiTiet == null || !req.ChiTiet.Any())
                    return Json(new { success = false, msg = "Giỏ hàng nhập đang trống!" });

                var maND = HttpContext.Session.GetString("MaND") ?? "ADMIN";
                string maPN = "PN" + DateTime.Now.ToString("yyMMddHHmmss");
                decimal tongTien = req.ChiTiet.Sum(x => x.SoLuong * x.DonGia);

                var phieuNhap = new PhieuNhap
                {
                    MaPN = maPN,
                    MaNCC = req.MaNCC,
                    MaND = maND,
                    NgayNhap = req.NgayNhap,
                    TongTien = tongTien,
                    GhiChu = req.GhiChu
                };
                _context.Add(phieuNhap);

                foreach (var item in req.ChiTiet)
                {
                    var chiTiet = new CT_PhieuNhap
                    {
                        MaPN = maPN,
                        MaSP = item.MaSP,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia,
                        ThanhTien = item.SoLuong * item.DonGia
                    };
                    _context.Add(chiTiet);

                    var sp = _context.SanPham.Find(item.MaSP);
                    if (sp != null)
                    {
                        sp.SoLuongTon = (sp.SoLuongTon ?? 0) + item.SoLuong;
                        sp.GiaNhap = item.DonGia;
                        _context.SanPham.Update(sp);
                    }
                }

                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi Database: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // =========================================================
        // 8. [API] TẠO NHANH SẢN PHẨM MỚI (TỒN KHO = 0)
        // =========================================================
        [HttpPost]
        public IActionResult CreateQuickProductAPI([FromBody] SanPham model)
        {
            try
            {
                if (_context.SanPham.Any(x => x.MaSP == model.MaSP))
                    return Json(new { success = false, msg = "Mã sản phẩm này đã tồn tại!" });

                model.SoLuongTon = 0;
                model.TrangThai = true;

                _context.SanPham.Add(model);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi Database: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // =========================================================
        // 9. [API] XEM LỊCH SỬ HÓA ĐƠN (ĐÃ BỌC GIÁP CHỐNG LỖI NULL)
        // =========================================================
        [HttpGet]
        public IActionResult GetInvoicesAPI()
        {
            try
            {
                // Chỉ lấy đúng cột MaHD lên trước để né lỗi Null ở các cột khác trong bảng Hoa_Don
                var maHDs = _context.HoaDon
                                    .OrderByDescending(h => h.MaHD)
                                    .Select(h => h.MaHD)
                                    .Take(50)
                                    .ToList();

                var chiTiets = _context.CT_HoaDon.Where(c => maHDs.Contains(c.MaHD)).ToList();

                var result = maHDs.Select(ma => new {
                    MaHD = ma,
                    // Dùng ?? 0 để nếu hóa đơn cũ bị Null số lượng/giá thì tự tính là 0đ
                    TongTien = chiTiets.Where(c => c.MaHD == ma).Sum(c => (c.SoLuong ?? 0) * (c.GiaBanThucTe ?? 0)),
                    // Cắt mã hóa đơn để lấy ngày. Nếu mã ngắn quá (Hóa đơn test cũ) thì để "Hóa đơn cũ"
                    NgayTao = ma.Length >= 14 ? $"20{ma.Substring(2, 2)}-{ma.Substring(4, 2)}-{ma.Substring(6, 2)}" : "Hóa đơn cũ"
                }).ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Vui lòng làm Bước 1 (Thêm dấu ?)! Lỗi gốc: " + ex.Message });
            }
        }

       
        [HttpGet]
        public IActionResult GetInvoiceDetailsAPI(string maHD)
        {
            try
            {
                var details = _context.CT_HoaDon.Where(c => c.MaHD == maHD).ToList();
                var spList = _context.SanPham.ToList();

                var result = details.Select(c => new {
                    // Cắt khoảng trắng cho mã SP hiển thị đẹp
                    MaSP = c.MaSP != null ? c.MaSP.Trim() : "Không có mã",

                    // [ĐÃ SỬA DÒNG NÀY] Thêm .Trim() vào cả 2 bên để máy tính nhận diện đúng tên SP 100%
                    TenSP = spList.FirstOrDefault(s => s.MaSP.Trim() == (c.MaSP ?? "").Trim())?.TenSP ?? "SP không xác định",

                    SoLuong = c.SoLuong ?? 0,
                    GiaBan = c.GiaBanThucTe ?? 0,
                    ThanhTien = (c.SoLuong ?? 0) * (c.GiaBanThucTe ?? 0)
                }).ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + ex.Message });
            }
        }
        // =========================================================
        // 10. [API] QUẢN LÝ NHÂN SỰ (THÊM / SỬA / XÓA)
        // =========================================================
        [HttpPost]
        public IActionResult SaveUserAPI([FromBody] NguoiDung model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.MaND))
                {
                    // [ĐÃ SỬA 1] Rút ngắn mã NV xuống 10 ký tự để không bị SQL chửi (VD: NV + Ngày + Giờ + Phút + Giây)
                    model.MaND = "NV" + DateTime.Now.ToString("ddHHmmss");

                    if (_context.NguoiDung.Any(u => u.TenDangNhap == model.TenDangNhap))
                        return Json(new { success = false, msg = "Tên đăng nhập này đã có người sử dụng!" });

                    // [ĐÃ SỬA 2] Nếu Boss quên nhập mật khẩu cho NV mới, tự đặt mặc định là 123456
                    if (string.IsNullOrEmpty(model.MatKhau))
                        model.MatKhau = "123456";

                    // Gán quyền mặc định nếu trên Web gửi về bị rỗng
                    if (string.IsNullOrEmpty(model.MaNhomQuyen))
                    {
                        if (model.VaiTro == "Sale") model.MaNhomQuyen = "STAFF_SALE";
                        else if (model.VaiTro == "Kho") model.MaNhomQuyen = "STAFF_KHO";
                        else model.MaNhomQuyen = "OWNER_FULL";
                    }

                    _context.NguoiDung.Add(model);
                }
                else
                {
                    // BƯỚC CẬP NHẬT
                    var user = _context.NguoiDung.Find(model.MaND);
                    if (user == null) return Json(new { success = false, msg = "Không tìm thấy nhân viên!" });

                    user.HoTen = model.HoTen;
                    user.Email = model.Email;
                    user.VaiTro = model.VaiTro;

                    if (!string.IsNullOrEmpty(model.MaNhomQuyen))
                        user.MaNhomQuyen = model.MaNhomQuyen;

                    user.TrangThai = model.TrangThai;

                    // Nếu nhập mật khẩu mới thì mới cập nhật
                    if (!string.IsNullOrEmpty(model.MatKhau))
                        user.MatKhau = model.MatKhau;

                    _context.NguoiDung.Update(user);
                }

                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // [ĐÃ SỬA 3] Bắt C# phải khai ra cái lỗi thực sự (InnerException) đưa lên Web cho Boss thấy
                string realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, msg = "Lỗi SQL: " + realError });
            }
        }
        // =========================================================
        // 11. [API] LẤY THỐNG KÊ CHI TIẾT THEO THÁNG (CÓ DS SẢN PHẨM)
        // =========================================================
        [HttpGet]
        public IActionResult GetMonthlyStatsAPI(int month)
        {
            try
            {
                string monthStr = month.ToString("D2");
                var currentYear = DateTime.Now.Year;

                var allMaHDs = _context.HoaDon.Where(h => h.MaHD != null).Select(h => h.MaHD).ToList();
                var monthlyMaHDs = allMaHDs.Where(m => m.Length >= 6 && m.Substring(4, 2) == monthStr).ToList();

                var chiTiets = _context.CT_HoaDon.Where(c => monthlyMaHDs.Contains(c.MaHD)).ToList();
                var sanPhams = _context.SanPham.ToList();

                decimal doanhThu = 0;
                decimal tienVon = 0;
                int tongSpBan = 0;

                foreach (var ct in chiTiets)
                {
                    decimal giaBan = ct.GiaBanThucTe ?? 0;
                    int sl = ct.SoLuong ?? 0;

                    doanhThu += sl * giaBan;
                    tongSpBan += sl;

                    var sp = sanPhams.FirstOrDefault(s => s.MaSP != null && s.MaSP.Trim() == (ct.MaSP ?? "").Trim());
                    if (sp != null) tienVon += sl * (sp.GiaNhap ?? 0);
                }

                // --- [MỚI] GOM NHÓM ĐỂ LẤY DANH SÁCH SẢN PHẨM ĐÃ BÁN ---
                var soldItems = chiTiets
                    .GroupBy(c => c.MaSP != null ? c.MaSP.Trim() : "Không rõ")
                    .Select(g => {
                        var maSP = g.Key;
                        var sp = sanPhams.FirstOrDefault(s => s.MaSP != null && s.MaSP.Trim() == maSP);
                        var tenSP = sp != null ? sp.TenSP : "Sản phẩm đã xóa";
                        var tongSL = g.Sum(x => x.SoLuong ?? 0);
                        var dt = g.Sum(x => (x.SoLuong ?? 0) * (x.GiaBanThucTe ?? 0));

                        return new { MaSP = maSP, TenSP = tenSP, SoLuong = tongSL, DoanhThu = dt };
                    })
                    .Where(x => x.SoLuong > 0)
                    .OrderByDescending(x => x.DoanhThu) // Thằng nào doanh thu cao nhất xếp lên đầu
                    .ToList();

                return Json(new
                {
                    success = true,
                    thang = monthStr,
                    nam = currentYear,
                    soHoaDon = monthlyMaHDs.Count,
                    tongSpBan = tongSpBan,
                    doanhThu = doanhThu,
                    loiNhuan = doanhThu - tienVon,
                    soldItems = soldItems // Trả danh sách về cho Web
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }
    }
}