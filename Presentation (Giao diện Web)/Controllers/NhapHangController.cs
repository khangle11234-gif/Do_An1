using Business.DTOs.Input;
using Core.Entities;
using Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore; // [MỚI THÊM] Thư viện này dùng để JOIN các bảng (Include)
using System;
using System.Linq;

namespace Presentation.Controllers
{
    public class NhapHangController : Controller
    {
        private readonly QuanLyKhoContext _context;

        public NhapHangController(QuanLyKhoContext context)
        {
            _context = context;
        }

        // 1. Giao diện Nhập hàng (GET)
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("MaND")))
                return RedirectToAction("Index", "Home");

            // Lấy NCC đang hoạt động (TrangThai = true)
            ViewBag.NhaCungCap = _context.NhaCungCap.Where(x => x.TrangThai == true).ToList();

            // Lấy danh sách sản phẩm (Lấy tất cả để tìm kiếm cho dễ)
            ViewBag.SanPham = _context.SanPham.ToList();

            return View();
        }

        // 2. Xử lý Lưu phiếu nhập (POST)
        [HttpPost]
        public IActionResult SavePhieuNhap([FromBody] PhieuNhapInputModel model)
        {
            if (model.ChiTiet == null || model.ChiTiet.Count == 0)
                return Json(new { success = false, msg = "Giỏ hàng đang trống!" });

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // A. TẠO HEADER PHIẾU
                    var phieu = new PhieuNhap
                    {
                        MaPN = "PN" + DateTime.Now.ToString("yyMMddHHmmss"),
                        NgayNhap = model.NgayNhap,
                        MaNCC = model.MaNCC,
                        MaND = HttpContext.Session.GetString("MaND"),
                        GhiChu = model.GhiChu,
                        TongTien = model.ChiTiet.Sum(x => x.SoLuong * x.DonGia),

                        // [QUAN TRỌNG] Trạng thái phiếu nhập (1: Hoàn thành)
                        TrangThai = 1,

                        NguoiDuyet = HttpContext.Session.GetString("MaND"),
                        NgayDuyet = DateTime.Now
                    };

                    _context.PhieuNhap.Add(phieu);
                    _context.SaveChanges(); // Lưu phiếu để lấy ID

                    // B. LƯU CHI TIẾT & CỘNG KHO
                    foreach (var item in model.ChiTiet)
                    {
                        var ct = new CT_PhieuNhap
                        {
                            MaPN = phieu.MaPN,
                            MaSP = item.MaSP,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            ThanhTien = item.SoLuong * item.DonGia
                        };
                        _context.CT_PhieuNhap.Add(ct);

                        // Cập nhật Kho & Giá Nhập mới nhất
                        var sp = _context.SanPham.Find(item.MaSP);
                        if (sp != null)
                        {
                            sp.SoLuongTon += item.SoLuong; // Cộng tồn kho
                            sp.GiaNhap = item.DonGia;      // Cập nhật giá vốn
                            sp.DonGiaNhap = item.DonGia;   // Cập nhật giá nhập

                            _context.Update(sp);
                        }
                    }

                    // C. Ghi lịch sử
                    // SỬA: Lỗi không thể lưu log và transaction chung (Đã tạo instance mới)
                    var log = new LichSuHeThong
                    {
                        NguoiThucHien = HttpContext.Session.GetString("HoTen"),
                        HanhDong = $"Nhập kho phiếu {phieu.MaPN}",
                        ThoiGian = DateTime.Now
                    };
                    _context.LichSuHeThong.Add(log);

                    _context.SaveChanges();
                    transaction.Commit(); // [QUAN TRỌNG] Phải Commit mới lưu thật

                    return Json(new { success = true, msg = "Nhập kho thành công!" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, msg = "Lỗi: " + ex.Message });
                }
            }
        }

        // =============================================================
        // 3. API Thêm nhanh Nhà Cung Cấp (Đã bỏ Transaction để lưu ngay lập tức)
        // =============================================================
        [HttpPost]
        public IActionResult CreateQuickSupplier([FromBody] NhaCungCap model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.TenNCC))
                    return Json(new { success = false, msg = "Tên NCC là bắt buộc" });

                if (string.IsNullOrEmpty(model.MaNCC))
                    model.MaNCC = "NCC" + DateTime.Now.ToString("HHmmss");

                // [QUAN TRỌNG] Phải set true để nó hiện ra trong dropdown (vì Create đang lọc Where TrangThai == true)
                model.TrangThai = true;

                _context.NhaCungCap.Add(model);
                _context.SaveChanges(); // Lưu thẳng vào DB

                return Json(new { success = true, data = model });
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                return Json(new { success = false, msg = "Lỗi SQL: " + ex.Message + " " + innerMsg });
            }
        }

        // =============================================================
        // 4. API Thêm nhanh Sản Phẩm (Đã bỏ Transaction để lưu ngay lập tức)
        // =============================================================
        [HttpPost]
        public IActionResult CreateQuickProduct([FromBody] SanPham model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.MaSP) || string.IsNullOrEmpty(model.TenSP))
                    return Json(new { success = false, msg = "Mã và Tên SP là bắt buộc!" });

                var exists = _context.SanPham.FirstOrDefault(x => x.MaSP == model.MaSP);
                if (exists != null)
                    return Json(new { success = false, msg = "Mã sản phẩm đã tồn tại!" });

                // Gán giá trị mặc định
                model.SoLuongTon = 0;
                model.TrangThai = true; // [QUAN TRỌNG] Set true để không bị ẩn
                model.DonGiaNhap = model.GiaNhap ?? 0;

                // Lưu ý: Các trường GiaBan, ThoiGianBaoHanh sẽ tự động nhận từ model (do JS gửi lên)

                _context.SanPham.Add(model);
                _context.SaveChanges(); // Lưu thẳng vào DB

                return Json(new { success = true, data = model });
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                return Json(new { success = false, msg = "Lỗi SQL: " + ex.Message + " " + innerMsg });
            }
        }

        // =============================================================
        // 5. [MỚI THÊM] Hiển thị Lịch sử nhập hàng cho Chủ Shop
        // =============================================================
        [HttpGet]
        public IActionResult LichSuNhap()
        {
            // 1. Kiểm tra đăng nhập
            var role = HttpContext.Session.GetString("VaiTro");
            if (string.IsNullOrEmpty(role)) return RedirectToAction("Index", "Home");

            // 2. Truy vấn dữ liệu từ DB
            // Dùng .Include() để JOIN bảng PhieuNhap với NhaCungCap và NguoiDung để lấy tên
            var lichSu = _context.PhieuNhap
                .Include(pn => pn.NhaCungCap)
                .Include(pn => pn.NguoiDung)
                .OrderByDescending(pn => pn.NgayNhap)
                .ToList();

            return View(lichSu);
        }
    }
}