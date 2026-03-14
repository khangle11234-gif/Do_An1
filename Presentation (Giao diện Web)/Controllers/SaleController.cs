using Microsoft.AspNetCore.Mvc;
using Business.DTOs.Output;
using Core.Entities;
using Data.Context;

namespace Presentation.Controllers
{
    public class SaleController : Controller
    {
        private readonly QuanLyKhoContext _context;

        public SaleController(QuanLyKhoContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. TRANG CHỦ NHÂN VIÊN BÁN HÀNG
        // ============================================================
        public IActionResult Index()
        {
            // Kiểm tra quyền: Chỉ cho Sale hoặc Owner vào
            var role = HttpContext.Session.GetString("VaiTro");
            if (role != "Sale" && role != "Owner") return RedirectToAction("Index", "Home");

            // Chỉ lấy danh sách sản phẩm ĐANG BÁN và CÒN TỒN KHO để nhân viên xem và bán
            var dsSanPham = _context.SanPham.Where(x => x.TrangThai == true).ToList();

            // Mượn tạm ViewModel của Owner cho nhanh, hoặc Boss có thể dùng ViewBag
            var model = new OwnerDashboardViewModel
            {
                DSSanPham = dsSanPham
            };

            ViewBag.ThemeConfig = _context.CauHinhHeThong.FirstOrDefault(c => c.Id == 1)
                               ?? new CauHinhHeThong { TenCuaHang = "SmartWarehouse", MauChuDao = "#2c3e50", MauNhan = "#3498db" };

            return View(model);
        }

        // ============================================================
        // 2. API THANH TOÁN POS (Dành riêng cho Nhân viên)
        // ============================================================
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
        // 3. API XEM LỊCH SỬ HÓA ĐƠN (Dành cho Nhân viên)
        // =========================================================
        [HttpGet]
        public IActionResult GetInvoicesAPI()
        {
            try
            {
                // Chỉ lấy 50 hóa đơn gần nhất để nhân viên xem cho nhẹ máy
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
                    // Cắt mã hóa đơn để lấy ngày (Mã HD có dạng HD240315103045)
                    NgayTao = ma.Length >= 14 ? $"20{ma.Substring(2, 2)}-{ma.Substring(4, 2)}-{ma.Substring(6, 2)}" : "Hóa đơn cũ"
                }).ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + ex.Message });
            }
        }

        // =========================================================
        // 4. API XEM CHI TIẾT HÓA ĐƠN (Dành cho Nhân viên)
        // =========================================================
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

                    // Tìm tên SP từ bảng SP dựa vào mã
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
        // 5. [API] LẤY THÔNG TIN DOANH NGHIỆP CHO HÓA ĐƠN (IN ẤN)
        // =========================================================
        [HttpGet]
        public IActionResult GetStoreInfoAPI()
        {
            try
            {
                var store = _context.ThongTinCongTy.Select(c => new
                {
                    Ten = c.TenCongTy ?? "SMART WAREHOUSE",
                    DiaChi = c.DiaChi ?? "Chưa cập nhật địa chỉ",
                    Hotline = c.SoDienThoai ?? "Chưa cập nhật số điện thoại",
                    MST = c.MaSoThue ?? "Chưa cập nhật MST"
                }).FirstOrDefault();

                if (store != null)
                {
                    return Json(new { success = true, data = store });
                }

                // Backup nếu Database trống
                return Json(new { success = true, data = new { Ten = "SMART WAREHOUSE", DiaChi = "Chi nhánh chính", Hotline = "---", MST = "---" } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi C#: " + ex.Message });
            }
        }
    }
}