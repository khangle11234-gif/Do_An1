using Core.Entities;
using Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Presentation.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly QuanLyKhoContext _context;
        public SanPhamController(QuanLyKhoContext context) { _context = context; }

        // =========================================================
        // 1. MÀN HÌNH QUẢN LÝ SẢN PHẨM & GIÁ
        // =========================================================
        [HttpGet]
        public IActionResult Index()
        {
            // Kiểm tra đăng nhập (Chỉ Owner, Admin hoặc Quản lý Kho được vào)
            var role = HttpContext.Session.GetString("VaiTro");
            if (string.IsNullOrEmpty(role)) return RedirectToAction("Index", "Home");

            // Lấy danh sách sản phẩm (Bao gồm cả thông tin Danh Mục)
            var danhSachSP = _context.SanPham
                                     .Include(sp => sp.DanhMuc)
                                     .OrderByDescending(sp => sp.MaSP)
                                     .ToList();

            // Gửi danh sách Danh mục qua ViewBag để dùng cho Dropdown chọn danh mục
            ViewBag.DanhMucs = _context.DanhMuc.ToList();

            return View(danhSachSP);
        }

        // =========================================================
        // 2. CẬP NHẬT THÔNG TIN & GIÁ SẢN PHẨM (POST)
        // =========================================================
        [HttpPost]
        public IActionResult Edit(SanPham model)
        {
            try
            {
                var sp = _context.SanPham.Find(model.MaSP);
                if (sp == null)
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm!";
                    return RedirectToAction("Index");
                }

                // Cập nhật các trường thông tin
                sp.TenSP = model.TenSP;
                sp.MaDM = model.MaDM;
                sp.DonViTinh = model.DonViTinh;
                sp.MaVach = model.MaVach;

                // Cập nhật GIÁ
                sp.GiaBan = model.GiaBan;
                // Giá nhập thường được cập nhật tự động khi Nhập Kho, 
                // nhưng nếu cho phép sửa tay thì bật dòng dưới lên:
                // sp.DonGiaNhap = model.DonGiaNhap; 

                sp.ThoiGianBaoHanh = model.ThoiGianBaoHanh;
                sp.TrangThai = model.TrangThai; // Bật/Tắt hiển thị

                _context.SanPham.Update(sp);
                _context.SaveChanges();

                TempData["Message"] = "Cập nhật sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // =========================================================
        // 3. ẨN / XÓA SẢN PHẨM
        // =========================================================
        [HttpPost]
        public IActionResult Delete(string maSP)
        {
            try
            {
                var sp = _context.SanPham.Find(maSP);
                if (sp != null)
                {
                    // Lời khuyên: Trong phần mềm bán hàng KHÔNG NÊN XÓA (Delete), 
                    // mà chỉ nên chuyển trạng thái thành False (Ẩn đi) để không mất lịch sử hóa đơn.
                    sp.TrangThai = false;
                    _context.SaveChanges();
                    TempData["Message"] = "Đã ngừng kinh doanh sản phẩm này!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        // =========================================================
        // 4. API THÊM NHANH (Dùng cho Modal Nhập Hàng cũ)
        // =========================================================
        [HttpPost]
        public IActionResult CreateQuick([FromBody] SanPham model)
        {
            try
            {
                if (_context.SanPham.Find(model.MaSP) != null)
                    return Json(new { success = false, msg = "Mã sản phẩm này đã tồn tại!" });

                model.SoLuongTon = 0; // Mới tạo thì tồn = 0
                model.TrangThai = true;

                _context.SanPham.Add(model);
                _context.SaveChanges();

                return Json(new { success = true, data = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
        }
    }
}