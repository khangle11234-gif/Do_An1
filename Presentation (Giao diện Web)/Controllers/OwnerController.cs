using Business.DTOs.Output;
using Core.Entities;
using Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using Web.Hubs;

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
        private readonly IHubContext<StoreHub> _hubContext;

        // [ĐÃ SỬA]: Nhét thêm IHubContext<StoreHub> hubContext vào trong ngoặc
        public OwnerController(QuanLyKhoContext context, IHubContext<StoreHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext; // Gán nó vào biến cục bộ để xài
        }

        // =========================================================
        // 1. LOAD GIAO DIỆN CHÍNH
        // =========================================================
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("VaiTro");
            if (role != "Owner") return RedirectToAction("Index", "Home");

            var users = _context.NguoiDung.Where(u => u.VaiTro != "Admin").OrderByDescending(u => u.MaND).ToList();
            var products = _context.SanPham.OrderByDescending(sp => sp.MaSP).ToList();
            var suppliers = _context.NhaCungCap.ToList();

            ViewBag.DanhSachQuyen = _context.NhomQuyen.ToList();

            ViewBag.TongVonTienHang = products.Sum(sp => (sp.SoLuongTon ?? 0) * (sp.GiaNhap ?? 0));
            ViewBag.GiaTriBanUocTinh = products.Sum(sp => (sp.SoLuongTon ?? 0) * (sp.GiaBan ?? 0));
            ViewBag.LoiNhuanUocTinh = ViewBag.GiaTriBanUocTinh - ViewBag.TongVonTienHang;

            ViewBag.ThemeConfig = _context.CauHinhHeThong.FirstOrDefault(c => c.Id == 1)
                               ?? new CauHinhHeThong { TenCuaHang = "SMART WAREHOUSE", MauChuDao = "#2c3e50", MauNhan = "#3498db" };
            // =======================================================
            // 1. TÍNH DỮ LIỆU BIỂU ĐỒ 7 NGÀY GẦN NHẤT (Sửa thành i = 6)
            // =======================================================
            var allInvoices = _context.HoaDon.Where(h => h.MaHD != null).Select(h => h.MaHD).ToList();
            var allDetails = _context.CT_HoaDon.ToList();

            var dailyLabels = new List<string>();
            var dailyRevenues = new List<decimal>();
            var dailyQueryDates = new List<string>();
            decimal maxDaily = 0;

            for (int i = 6; i >= 0; i--) // Lùi 7 ngày (từ 6 về 0)
            {
                var d = DateTime.Now.AddDays(-i);
                string prefix = $"HD{d.ToString("yyMMdd")}";

                var invs = allInvoices.Where(m => m.StartsWith(prefix)).ToList();
                decimal rev = allDetails.Where(c => invs.Contains(c.MaHD)).Sum(c => (c.SoLuong ?? 0) * (c.GiaBanThucTe ?? 0));

                if (rev > maxDaily) maxDaily = rev;

                dailyLabels.Add(i == 0 ? "Hôm nay" : d.ToString("dd/MM"));
                dailyRevenues.Add(rev);
                dailyQueryDates.Add(d.ToString("yyyy-MM-dd"));
            }

            ViewBag.DailyLabels = dailyLabels;
            ViewBag.DailyRevenues = dailyRevenues;
            ViewBag.DailyQueryDates = dailyQueryDates;
            ViewBag.MaxDaily = maxDaily > 0 ? maxDaily : 1;

            // =======================================================
            // 2. TÍNH DỮ LIỆU BIỂU ĐỒ 12 THÁNG GẦN NHẤT (Sửa thành i = 11)
            // =======================================================
            var monthlyLabels = new List<string>();
            var monthlyRevenues = new List<decimal>();
            var monthlyQueryMonths = new List<int>();
            var monthlyQueryYears = new List<int>();
            decimal maxMonthly = 0;

            for (int i = 11; i >= 0; i--) // Lùi 12 tháng (từ 11 về 0)
            {
                var m = DateTime.Now.AddMonths(-i);
                string prefix = $"HD{m.ToString("yyMM")}"; // HD2403

                var invs = allInvoices.Where(x => x.StartsWith(prefix)).ToList();
                decimal rev = allDetails.Where(c => invs.Contains(c.MaHD)).Sum(c => (c.SoLuong ?? 0) * (c.GiaBanThucTe ?? 0));

                if (rev > maxMonthly) maxMonthly = rev;

                // [CẢI TIẾN] Để tên tháng là "T1", "T2" cho gọn để nhét vừa 12 cột
                monthlyLabels.Add(i == 0 ? "Tháng này" : "T" + m.Month);
                monthlyRevenues.Add(rev);
                monthlyQueryMonths.Add(m.Month);
                monthlyQueryYears.Add(m.Year);
            }

            ViewBag.MonthlyLabels = monthlyLabels;
            ViewBag.MonthlyRevenues = monthlyRevenues;
            ViewBag.MonthlyQueryMonths = monthlyQueryMonths;
            ViewBag.MonthlyQueryYears = monthlyQueryYears;
            ViewBag.MaxMonthly = maxMonthly > 0 ? maxMonthly : 1;

            var model = new OwnerDashboardViewModel
            {
                DanhSachNhanVien = users,
                DSSanPham = products,
                DSNhaCungCap = suppliers
            };

            return View(model);
        }

        // =========================================================
        // 2. [API] SỬA GIÁ TRỰC TIẾP KHÔNG TẢI LẠI TRANG (AJAX)
        // =========================================================
       
        [HttpPost]
        public IActionResult UpdateProductAPI([FromBody] Core.Entities.SanPham model)
        {
            try
            {
                var sp = _context.SanPham.FirstOrDefault(x => x.MaSP == model.MaSP);
                if (sp == null) return Json(new { success = false, msg = "Không tìm thấy mã sản phẩm!" });

                // Cập nhật các trường thông tin
                sp.TenSP = model.TenSP;
                sp.MaVach = model.MaVach;

                // [ĐÂY RỒI NÈ BOSS: Dòng bốc thuốc chữa lỗi không lưu giá vốn]
                sp.GiaNhap = model.GiaNhap;

                sp.GiaBan = model.GiaBan;

                // Nếu Boss có dùng Đơn vị tính hay Nhóm hàng thì để nguyên mấy dòng dưới của Boss nhé
                // sp.DonViTinh = model.DonViTinh;

                _context.SanPham.Update(sp);
                _context.SaveChanges();

                return Json(new { success = true, msg = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + ex.Message });
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
        [HttpPost] // <-- [BẮT BUỘC PHẢI CÓ] Để nhận dữ liệu từ Javascript
        public IActionResult ProcessPosCheckout([FromBody] PosCheckoutRequest req)
        {
            // 1. KIỂM TRA QUYỀN (CHO PHÉP ADMIN, OWNER VÀ SALE ĐƯỢC BÁN HÀNG)
            var role = HttpContext.Session.GetString("VaiTro");
            if (role != "Admin" && role != "Owner" && role != "Sale")
                return Json(new { success = false, msg = "Tài khoản của bạn không có quyền bán hàng!" });

            try
            {
                if (req.CartItems == null || !req.CartItems.Any())
                    return Json(new { success = false, msg = "Giỏ hàng trống, không thể thanh toán!" });

                // Lấy mã nhân viên đang đứng quầy
                var maND = HttpContext.Session.GetString("MaND") ?? "ADMIN";
                string maHD = "HD" + DateTime.Now.ToString("yyMMddHHmmss");

                // [MỚI] Biến để cộng dồn tổng tiền hóa đơn
                decimal tongTienHD = 0;

                // 2. KHỞI TẠO HÓA ĐƠN
                var hoaDon = new HoaDon // (Đổi lại thành HoaDon nếu class của Boss viết liền)
                {
                    MaHD = maHD,
                    MaND = maND, // Lưu người lập hóa đơn
                    NgayTao = DateTime.Now, // Lưu ngày giờ bán
                    GhiChu = req.GhiChu // Lấy ghi chú từ Form (VD: Khách chuyển khoản VCB)
                };

                // 3. DUYỆT QUA TỪNG SẢN PHẨM TRONG GIỎ HÀNG
                foreach (var item in req.CartItems)
                {
                    var sp = _context.SanPham.Find(item.MaSP); // (Đổi thành SanPham nếu DB viết liền)
                    if (sp != null)
                    {
                        // Kiểm tra kho cực ngặt
                        if (sp.SoLuongTon < item.SoLuong)
                            return Json(new { success = false, msg = $"Sản phẩm '{sp.TenSP}' chỉ còn {sp.SoLuongTon} cái trong kho!" });

                        // Trừ tồn kho lập tức
                        sp.SoLuongTon -= item.SoLuong;
                        _context.SanPham.Update(sp);

                        // Cộng dồn tổng tiền
                        tongTienHD += (item.GiaBan * item.SoLuong);

                        // 4. LƯU CHI TIẾT HÓA ĐƠN
                        var chiTiet = new CT_HoaDon // (Đổi thành CT_HoaDon nếu DB viết liền)
                        {
                            MaHD = maHD,
                            MaSP = item.MaSP,
                            SoLuong = item.SoLuong,
                            // Lúc trước ta đã nới rộng cột MaSerial lên 50 ký tự, nên giờ xài Guid bao la
                            MaSerial = item.MaSP + "-" + Guid.NewGuid().ToString().Substring(0, 8),
                            // Nếu class của Boss đặt tên cột giá là GiaBan thì đổi lại nhé
                            GiaBanThucTe = item.GiaBan
                        };
                        _context.Add(chiTiet);
                    }
                    else
                    {
                        return Json(new { success = false, msg = $"Lỗi: Không tìm thấy SP mã {item.MaSP}!" });
                    }
                }

                // Gán tổng tiền đã cộng dồn vào hóa đơn
                hoaDon.TongTien = tongTienHD;

                // Thêm hóa đơn vào Context (Phải làm sau cùng để nó lấy được Tổng tiền)
                _context.Add(hoaDon);

                // 5. LƯU TẤT CẢ VÀO DATABASE CÙNG MỘT LÚC (TRANSACTION AN TOÀN)
                _context.SaveChanges();

                return Json(new { success = true, maHD = maHD });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + (ex.InnerException?.Message ?? ex.Message) });
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

                var result = maHDs.Select(ma => new
                {
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

                var result = details.Select(c => new
                {
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
        // 11. [API] LẤY THỐNG KÊ CHI TIẾT THEO NGÀY (CÓ DS SẢN PHẨM)
        // =========================================================
        [HttpGet]
        public IActionResult GetDailyStatsAPI(string date)
        {
            try
            {
                DateTime queryDate = DateTime.Parse(date);
                string prefix = $"HD{queryDate.ToString("yyMMdd")}"; // Cắt mã HD để dò theo ngày

                var allMaHDs = _context.HoaDon.Where(h => h.MaHD != null && h.MaHD.StartsWith(prefix)).Select(h => h.MaHD).ToList();
                var chiTiets = _context.CT_HoaDon.Where(c => allMaHDs.Contains(c.MaHD)).ToList();
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

                var soldItems = chiTiets
                    .GroupBy(c => c.MaSP != null ? c.MaSP.Trim() : "Không rõ")
                    .Select(g =>
                    {
                        var maSP = g.Key;
                        var sp = sanPhams.FirstOrDefault(s => s.MaSP != null && s.MaSP.Trim() == maSP);
                        var tenSP = sp != null ? sp.TenSP : "Sản phẩm đã xóa";
                        var tongSL = g.Sum(x => x.SoLuong ?? 0);
                        var dt = g.Sum(x => (x.SoLuong ?? 0) * (x.GiaBanThucTe ?? 0));

                        return new { MaSP = maSP, TenSP = tenSP, SoLuong = tongSL, DoanhThu = dt };
                    })
                    .Where(x => x.SoLuong > 0)
                    .OrderByDescending(x => x.DoanhThu)
                    .ToList();

                return Json(new
                {
                    success = true,
                    ngay = queryDate.ToString("dd/MM/yyyy"), // Trả về ngày
                    soHoaDon = allMaHDs.Count,
                    tongSpBan = tongSpBan,
                    doanhThu = doanhThu,
                    loiNhuan = doanhThu - tienVon,
                    soldItems = soldItems
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }
        [HttpGet]
        public IActionResult GetMonthlyStatsAPI(int month, int year)
        {
            try
            {
                string prefix = $"HD{year.ToString().Substring(2, 2)}{month.ToString("D2")}"; // Ghép thành HD2403
                var allMaHDs = _context.HoaDon.Where(h => h.MaHD != null && h.MaHD.StartsWith(prefix)).Select(h => h.MaHD).ToList();
                var chiTiets = _context.CT_HoaDon.Where(c => allMaHDs.Contains(c.MaHD)).ToList();
                var sanPhams = _context.SanPham.ToList();

                decimal doanhThu = 0; decimal tienVon = 0; int tongSpBan = 0;

                foreach (var ct in chiTiets)
                {
                    decimal giaBan = ct.GiaBanThucTe ?? 0; int sl = ct.SoLuong ?? 0;
                    doanhThu += sl * giaBan; tongSpBan += sl;
                    var sp = sanPhams.FirstOrDefault(s => s.MaSP != null && s.MaSP.Trim() == (ct.MaSP ?? "").Trim());
                    if (sp != null) tienVon += sl * (sp.GiaNhap ?? 0);
                }

                var soldItems = chiTiets.GroupBy(c => c.MaSP != null ? c.MaSP.Trim() : "Không rõ").Select(g =>
                {
                    var sp = sanPhams.FirstOrDefault(s => s.MaSP != null && s.MaSP.Trim() == g.Key);
                    return new { MaSP = g.Key, TenSP = sp != null ? sp.TenSP : "Sản phẩm đã xóa", SoLuong = g.Sum(x => x.SoLuong ?? 0), DoanhThu = g.Sum(x => (x.SoLuong ?? 0) * (x.GiaBanThucTe ?? 0)) };
                }).Where(x => x.SoLuong > 0).OrderByDescending(x => x.DoanhThu).ToList();

                return Json(new { success = true, ngay = $"Tháng {month}/{year}", soHoaDon = allMaHDs.Count, tongSpBan = tongSpBan, doanhThu = doanhThu, loiNhuan = doanhThu - tienVon, soldItems = soldItems });
            }
            catch (Exception ex) { return Json(new { success = false, msg = "Lỗi C#: " + ex.Message }); }
        }
        // =========================================================
        // [API MỚI] LẤY DỮ LIỆU BIỂU ĐỒ THEO 7 NGÀY / 12 THÁNG
        // =========================================================
        [HttpGet]
        public IActionResult GetCustomChartAPI(string type, string selectedDate, int year)
        {
            try
            {
                var allInvoices = _context.HoaDon.Where(h => h.MaHD != null).Select(h => h.MaHD).ToList();
                var allDetails = _context.CT_HoaDon.ToList();

                var labels = new List<string>();
                var revenues = new List<decimal>();
                var queryKeys = new List<string>();
                decimal maxRev = 0;

                if (type == "daily")
                {
                    // Chế độ xem 7 ngày lùi từ ngày được chọn
                    DateTime endDate = string.IsNullOrEmpty(selectedDate) ? DateTime.Now : DateTime.Parse(selectedDate);

                    // Lùi 7 ngày (từ 6 về 0)
                    for (int i = 6; i >= 0; i--)
                    {
                        var d = endDate.AddDays(-i);
                        string prefix = $"HD{d.ToString("yyMMdd")}";

                        var invs = allInvoices.Where(m => m.StartsWith(prefix)).ToList();
                        decimal rev = allDetails.Where(c => invs.Contains(c.MaHD)).Sum(c => (c.SoLuong ?? 0) * (c.GiaBanThucTe ?? 0));

                        if (rev > maxRev) maxRev = rev;

                        // Nếu là ngày cuối cùng trong 7 ngày thì để chữ đậm
                        labels.Add(i == 0 ? d.ToString("dd/MM") : d.ToString("dd/MM"));
                        revenues.Add(rev);
                        queryKeys.Add(d.ToString("yyyy-MM-dd"));
                    }
                }
                else
                {
                    // Chế độ xem 12 Tháng trong 1 Năm
                    for (int i = 1; i <= 12; i++)
                    {
                        string prefix = $"HD{year.ToString().Substring(2, 2)}{i.ToString("D2")}";

                        var invs = allInvoices.Where(m => m.StartsWith(prefix)).ToList();
                        decimal rev = allDetails.Where(c => invs.Contains(c.MaHD)).Sum(c => (c.SoLuong ?? 0) * (c.GiaBanThucTe ?? 0));

                        if (rev > maxRev) maxRev = rev;
                        labels.Add("T" + i);
                        revenues.Add(rev);
                        queryKeys.Add($"{i},{year}");
                    }
                }

                return Json(new { success = true, labels, revenues, queryKeys, maxRev = maxRev > 0 ? maxRev : 1 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }
        // =========================================================
        // [CÔNG CỤ] BỘ NÃO TỰ ĐỘNG VIẾT TẮT THÔNG MINH
        // =========================================================

        // Hàm 1: Xóa dấu Tiếng Việt (Sơn -> Son, Thép -> Thep)
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            string[] vietnameseSigns = new string[] {
                "aAeEoOuUiIdDyY",
                "áàạảãâăắằẳẵâấầậẩẫ", "ÁÀẠẢÃÂĂẮẰẲẴÂẤẦẬẨẪ",
                "éèẹẻẽêếềệểễ", "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ", "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ", "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ", "ÍÌỊỈĨ",
                "đ", "Đ",
                "ýỳỵỷỹ", "ÝỲỴỶỸ"
            };
            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    text = text.Replace(vietnameseSigns[i][j].ToString(), vietnameseSigns[0][i - 1].ToString());
            }
            return text.ToUpper().Trim(); // Trả về chữ in hoa
        }

        // Hàm 2: Tự động đẻ ra mã viết tắt
        private string GenerateAbbreviation(string input, bool isCategory)
        {
            if (string.IsNullOrWhiteSpace(input)) return "XXX";
            string normalized = RemoveDiacritics(input); // Chuyển "Hòa Phát" thành "HOA PHAT"

            if (isCategory)
            {
                // Nhóm hàng: Lấy 3 ký tự đầu. VD: "THÉP" -> "THEP" -> "THE", "SƠN" -> "SON"
                string noSpace = normalized.Replace(" ", "");
                return noSpace.Length >= 3 ? noSpace.Substring(0, 3) : noSpace;
            }
            else
            {
                // Nhà cung cấp
                var words = normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 1)
                {
                    // Nhiều từ: "HOA PHAT" -> "HP", "XI MANG HA TIEN" -> "XMH"
                    string abbr = "";
                    foreach (var w in words) abbr += w[0];
                    return abbr.Length > 3 ? abbr.Substring(0, 3) : abbr;
                }
                else
                {
                    // Một từ: "POMINA" -> lấy P và phụ âm tiếp theo -> "PM", "NIPPON" -> "NP"
                    string word = words[0];
                    if (word.Length <= 2) return word;
                    string vowels = "AEIOUY"; // Nguyên âm
                    for (int i = 1; i < word.Length; i++)
                    {
                        // Tìm phụ âm đầu tiên sau ký tự đầu
                        if (!vowels.Contains(word[i].ToString()))
                            return $"{word[0]}{word[i]}";
                    }
                    return word.Substring(0, 2); // Nếu không có phụ âm (hiếm) thì lấy 2 chữ đầu
                }
            }

        }
        // =========================================================
        // [API] TỰ ĐỘNG SINH MÃ SẢN PHẨM THEO TÊN NHÓM VÀ NHÀ CUNG CẤP
        // =========================================================
        [HttpGet]
        public IActionResult GenerateSKU(string tenNhom, string tenNCC)
        {
            try
            {
                // 1. Tự động sinh chữ viết tắt
                string maNhom = GenerateAbbreviation(tenNhom, isCategory: true);  // "Sơn" -> "SON"
                string maNCC = GenerateAbbreviation(tenNCC, isCategory: false);   // "Nippon" -> "NP"

                // 2. Ghép thành tiền tố (Prefix) -> VD: "SON-NP-"
                string prefix = $"{maNhom}-{maNCC}-";

                // 3. Đếm xem trong DB đã có bao nhiêu SP mang tiền tố này rồi
                // (Vì SP cũ của Boss có thể chưa theo chuẩn này, nên ta đếm số lượng sp có chứa tiền tố)
                int countExisting = _context.SanPham
                                    .Where(s => s.MaSP != null && s.MaSP.StartsWith(prefix))
                                    .Count();

                // 4. Sinh số thứ tự tiếp theo (định dạng 4 số 0016)
                string nextNumber = (countExisting + 1).ToString("D4");

                // 5. Chốt hạ Mã Sản Phẩm Cuối Cùng -> SON-NP-0016
                string finalSKU = $"{prefix}{nextNumber}";

                return Json(new { success = true, maSP = finalSKU, prefix = prefix });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
        }
        // =========================================================
        // [API MỚI] XEM LỊCH SỬ PHIẾU NHẬP VÀ CHI TIẾT
        // =========================================================
        [HttpGet]
        public IActionResult GetImportHistoryAPI()
        {
            try
            {
                // Gọi từ DbSet PhieuNhap (nếu DbSet của Boss tên khác thì sửa lại xíu nhé, VD: Phieu_Nhap)
                var pnList = _context.PhieuNhap
                    .OrderByDescending(p => p.NgayNhap)
                    .Select(p => new
                    {
                        MaPN = p.MaPN,
                        // 1. NgayNhap là DateTime (không rỗng) nên gọi .ToString() trực tiếp
                        NgayNhap = p.NgayNhap.ToString("dd/MM/yyyy HH:mm"),

                        // 2. TongTien là decimal (không rỗng) nên gọi thẳng, không dùng ?? 0m
                        TongTien = p.TongTien,

                        GhiChu = p.GhiChu
                    }).ToList();

                return Json(new { success = true, data = pnList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetImportDetailsAPI(string maPN)
        {
            try
            {
                // Dùng đúng tên DonGia thay cho DonGiaNhap
                var details = _context.CT_PhieuNhap
                    .Where(c => c.MaPN == maPN)
                    .Join(_context.SanPham, c => c.MaSP, s => s.MaSP, (c, s) => new
                    {
                        MaSP = c.MaSP,
                        TenSP = s.TenSP,
                        SoLuong = c.SoLuong ?? 0,

                        // 3. Lấy đúng cột DonGia
                        DonGiaNhap = c.DonGia ?? 0m,

                        // Lấy luôn cột ThanhTien đã có sẵn của Boss
                        ThanhTien = c.ThanhTien ?? ((c.SoLuong ?? 0) * (c.DonGia ?? 0m)),

                        // Bỏ trống Số Lô để tránh lỗi không tìm thấy property
                        SoLo = ""
                    }).ToList();

                return Json(new { success = true, data = details });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + ex.Message });
            }
        }
        // =========================================================
        // [API] LẤY THÔNG TIN CỬA HÀNG/CHI NHÁNH CHO HÓA ĐƠN
        // =========================================================
        [HttpGet]
        // =========================================================
        // [API] LẤY THÔNG TIN DOANH NGHIỆP TỪ ADMIN CHO HÓA ĐƠN
        // =========================================================
        [HttpGet]
        public IActionResult GetStoreInfoAPI()
        {
            try
            {
                // [ĐÃ SỬA]: Móc đúng vào bảng ThongTinCongTy của Admin
                var store = _context.ThongTinCongTy.Select(c => new
                {
                    // Ánh xạ đúng tên cột trong Database của Boss
                    Ten = c.TenCongTy ?? "SMART WAREHOUSE",
                    DiaChi = c.DiaChi ?? "Chưa cập nhật địa chỉ",
                    Hotline = c.SoDienThoai ?? "Chưa cập nhật số điện thoại",
                    MST = c.MaSoThue ?? "Chưa cập nhật MST"
                }).FirstOrDefault();

                if (store != null)
                {
                    return Json(new { success = true, data = store });
                }

                // Backup nếu Database trống (Admin chưa điền gì)
                return Json(new { success = true, data = new { Ten = "SMART WAREHOUSE", DiaChi = "Chưa cấu hình", Hotline = "Chưa cấu hình", MST = "Chưa cấu hình" } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + ex.Message });
            }
        }
        // =========================================================
        // [API] CHỈNH SỬA SẢN PHẨM (LẤY DỮ LIỆU & LƯU CẬP NHẬT)
        // =========================================================

        // 1. Kéo dữ liệu cũ của sản phẩm lên form
        [HttpGet]
        public IActionResult GetProductDetailAPI(string maSP)
        {
            try
            {
                var sp = _context.SanPham.FirstOrDefault(x => x.MaSP == maSP);
                if (sp == null) return Json(new { success = false, msg = "Không tìm thấy sản phẩm này!" });

                return Json(new { success = true, data = sp });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteProductAPI(string maSP)
        {
            try
            {
                var sp = _context.SanPham.Find(maSP);
                if (sp == null)
                    return Json(new { success = false, msg = "Không tìm thấy sản phẩm!" });

                // [BẢO VỆ 1]: Kiểm tra xem đã từng NHẬP KHO chưa?
                // (Boss xem trong file QuanLyKhoContext.cs chỗ DbSet nó tên là CT_Phieu_Nhap hay CT_Phieu_Nhaps nhé)
                bool daTungNhap = _context.CT_PhieuNhap.Any(c => c.MaSP == maSP);

                // [BẢO VỆ 2]: Kiểm tra xem đã từng BÁN chưa? (Nếu Boss có bảng chi tiết hóa đơn)
                // bool daTungBan = _context.CT_Hoa_Dons.Any(c => c.MaSP == maSP);

                if (daTungNhap) // Nếu có mở comment dòng trên thì sửa thành: if (daTungNhap || daTungBan)
                {
                    // Chặn lại và báo lỗi bằng Tiếng Việt
                    return Json(new
                    {
                        success = false,
                        msg = "Sản phẩm này đã có lịch sử NHẬP KHO. Để đảm bảo toàn vẹn dữ liệu, bạn không thể xóa. Vui lòng tắt công tắc [Trạng thái] để ngừng bán sản phẩm này!"
                    });
                }

                // Nếu là sản phẩm mới tạo, chưa nhập chưa bán thì cho xóa bay màu
                _context.SanPham.Remove(sp);
                _context.SaveChanges();

                return Json(new { success = true, msg = "Đã xóa sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi hệ thống không thể xóa: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }
        [HttpPost]
        public IActionResult DeleteUserAPI(string MaND)
        {
            try
            {
                if (string.IsNullOrEmpty(MaND)) return Json(new { success = false, msg = "Mã nhân viên trống!" });

                string currentUserId = HttpContext.Session.GetString("MaND");

                // 1. Chặn tự sát
                if (MaND == currentUserId)
                    return Json(new { success = false, msg = "Bạn không thể tự xóa tài khoản của chính mình đang đăng nhập!" });

                var userToDelete = _context.NguoiDung.FirstOrDefault(u => u.MaND == MaND);
                if (userToDelete == null)
                    return Json(new { success = false, msg = "Không tìm thấy nhân viên này trong hệ thống!" });

                // 2. Chặn xóa root
                if (userToDelete.TenDangNhap.ToLower() == "admin" || userToDelete.VaiTro == "Owner")
                    return Json(new { success = false, msg = "Không thể xóa tài khoản Quản trị cấp cao gốc!" });

                // 3. Tiến hành xóa
                _context.NguoiDung.Remove(userToDelete);
                _context.SaveChanges();

                return Json(new { success = true, msg = "Đã xóa vĩnh viễn nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                // Nếu báo lỗi SQL (thường là do lỗi ràng buộc khóa ngoại: Nhân viên này đã từng bán hàng/nhập kho)
                return Json(new
                {
                    success = false,
                    msg = "Nhân viên này đã từng có lịch sử giao dịch (bán hàng/nhập kho). Để bảo toàn sổ sách, vui lòng sử dụng chức năng Sửa (cây bút chì) và chuyển Trạng thái sang 'Đã khóa' thay vì xóa vĩnh viễn!"
                });
            }
        }
        // API Tự động lưu Ngân hàng và STK ngầm
        [HttpPost]
        // API Tự động lưu Ngân hàng, STK và Tên Chủ TK ngầm
        [HttpPost]
        public IActionResult SaveBankInfoAPI(string nganHang, string stk, string tenChuTK)
        {
            try
            {
                var config = _context.CauHinhHeThong.FirstOrDefault(c => c.Id == 1);
                if (config == null)
                {
                    config = new CauHinhHeThong { Id = 1, NganHang = nganHang, SoTaiKhoan = stk, TenChuTaiKhoan = tenChuTK };
                    _context.CauHinhHeThong.Add(config);
                }
                else
                {
                    config.NganHang = nganHang;
                    config.SoTaiKhoan = stk;
                    config.TenChuTaiKhoan = tenChuTK; // Lưu thêm tên
                    _context.CauHinhHeThong.Update(config);
                }

                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
        }
    }
}