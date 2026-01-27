using Business.DTOs.Input;
using Business.DTOs.Output;
using Core.Entities;
using Data.Context;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class AdminController : Controller
    {
        private readonly QuanLyKhoContext _context;

        public AdminController(QuanLyKhoContext context)
        {
            _context = context;
        }

        // Helper check quyền
        private bool IsAdmin() => HttpContext.Session.GetString("VaiTro") == "Admin";

        // ============================================================
        // 1. TRANG CHỦ ADMIN (DASHBOARD)
        // ============================================================
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var info = _context.ThongTinCongTy.FirstOrDefault();
            var users = _context.NguoiDung.OrderBy(u => u.MaND).ToList();
            var logs = _context.LichSuHeThong.OrderByDescending(x => x.ThoiGian).Take(50).ToList();
            ViewBag.DanhSachQuyen = _context.NhomQuyen.ToList();

            var model = new AdminDashboardViewModel
            {
                CongTy = info ?? new ThongTinCongTy(),
                DanhSachNhanVien = users,
                LichSu = logs
            };

            return View(model);
        }

        // ============================================================
        // 2. XỬ LÝ LƯU CẤU HÌNH CÔNG TY
        // ============================================================
        [HttpPost]
        public IActionResult SaveConfig(ThongTinCongTy model)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var info = _context.ThongTinCongTy.FirstOrDefault();
            if (info == null) _context.ThongTinCongTy.Add(model);
            else
            {
                info.TenCongTy = model.TenCongTy;
                info.MaSoThue = model.MaSoThue;
                info.DiaChi = model.DiaChi;
                info.Email = model.Email;
                info.SoDienThoai = model.SoDienThoai;
                info.ChoPhepBanHang = model.ChoPhepBanHang;
            }

            _context.LichSuHeThong.Add(new LichSuHeThong
            {
                NguoiThucHien = HttpContext.Session.GetString("HoTen"),
                HanhDong = "Cập nhật thông tin công ty",
                ThoiGian = DateTime.Now
            });

            _context.SaveChanges();
            TempData["Message"] = "Đã lưu cấu hình doanh nghiệp!";
            return RedirectToAction("Index");
        }

        // ============================================================
        // 3. XỬ LÝ LƯU USER (THÊM MỚI HOẶC SỬA) - ĐÃ CẬP NHẬT EMAIL
        // ============================================================
        [HttpPost]
        public IActionResult SaveUser(UserCreateViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            try
            {
                // === LOGIC 1: THÊM MỚI (Nếu MaND rỗng hoặc null) ===
                if (string.IsNullOrEmpty(model.MaND))
                {
                    // Kiểm tra trùng tên đăng nhập
                    if (_context.NguoiDung.Any(u => u.TenDangNhap == model.TenDangNhap))
                    {
                        TempData["Error"] = "Tên đăng nhập đã tồn tại!";
                        return RedirectToAction("Index");
                    }

                    // [MỚI] Kiểm tra trùng Email
                    if (_context.NguoiDung.Any(u => u.Email == model.Email))
                    {
                        TempData["Error"] = "Email này đã được sử dụng cho nhân viên khác!";
                        return RedirectToAction("Index");
                    }

                    var newUser = new NguoiDung
                    {
                        MaND = "NV-" + DateTime.Now.Ticks.ToString().Substring(12),
                        TenDangNhap = model.TenDangNhap,
                        MatKhau = model.MatKhau, // (Thực tế nên mã hóa MD5/BCrypt)
                        HoTen = model.HoTen,
                        VaiTro = model.VaiTro,
                        TrangThai = model.HienThi,

                        // [QUAN TRỌNG] Lưu Email thật từ form nhập vào
                        Email = model.Email,

                        MaCN = "CN01",
                        NgayTao = DateTime.Now,
                        MaNhomQuyen = model.MaNhomQuyen
                    };

                    _context.NguoiDung.Add(newUser);
                    _context.LichSuHeThong.Add(new LichSuHeThong
                    {
                        NguoiThucHien = HttpContext.Session.GetString("HoTen"),
                        HanhDong = $"Thêm nhân viên mới: {model.TenDangNhap}",
                        ThoiGian = DateTime.Now
                    });

                    TempData["Message"] = "Thêm nhân viên mới thành công!";
                }
                // === LOGIC 2: CẬP NHẬT (Khi có MaND gửi lên) ===
                else
                {
                    // Bước 1: Tìm nhân viên cũ trong DB dựa vào MaND
                    var existingUser = _context.NguoiDung.FirstOrDefault(u => u.MaND == model.MaND);

                    // Bước 2: Kiểm tra xem có tìm thấy không
                    if (existingUser == null)
                    {
                        TempData["Error"] = $"Lỗi: Không tìm thấy nhân viên có mã {model.MaND} để sửa!";
                        return RedirectToAction("Index");
                    }

                    // [MỚI] Kiểm tra trùng Email khi cập nhật (nếu có thay đổi email)
                    if (existingUser.Email != model.Email && _context.NguoiDung.Any(u => u.Email == model.Email))
                    {
                        TempData["Error"] = "Email mới bị trùng với nhân viên khác!";
                        return RedirectToAction("Index");
                    }

                    // Bước 3: Gán dữ liệu MỚI vào đối tượng CŨ
                    existingUser.HoTen = model.HoTen;
                    existingUser.VaiTro = model.VaiTro;
                    existingUser.TrangThai = model.HienThi;

                    // [QUAN TRỌNG] Cập nhật Email thật
                    existingUser.Email = model.Email;

                    // Chỉ cập nhật quyền nếu có dữ liệu gửi lên
                    if (!string.IsNullOrEmpty(model.MaNhomQuyen))
                    {
                        existingUser.MaNhomQuyen = model.MaNhomQuyen;
                    }

                    // Chỉ đổi mật khẩu nếu người dùng nhập mật khẩu mới
                    if (!string.IsNullOrEmpty(model.MatKhau))
                    {
                        existingUser.MatKhau = model.MatKhau;
                    }

                    // Đánh dấu bản ghi đã bị thay đổi
                    _context.Update(existingUser);

                    // Ghi lịch sử
                    _context.LichSuHeThong.Add(new LichSuHeThong
                    {
                        NguoiThucHien = HttpContext.Session.GetString("HoTen"),
                        HanhDong = $"Cập nhật nhân viên: {existingUser.TenDangNhap}",
                        ThoiGian = DateTime.Now
                    });

                    TempData["Message"] = "Cập nhật thông tin thành công!";
                }

                // === CHỐT ĐƠN: LƯU XUỐNG DATABASE ===
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ============================================================
        // 4. SAO LƯU DỮ LIỆU
        // ============================================================
        public IActionResult BackupData()
        {
            // Code backup cũ của bạn...
            TempData["Message"] = "Đã tạo bản sao lưu thành công!";
            return RedirectToAction("Index");
        }

        // ============================================================
        // 5. XỬ LÝ XÓA NHÂN VIÊN
        // ============================================================
        [HttpPost]
        public IActionResult DeleteUser(string id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            try
            {
                // 1. Tìm nhân viên cần xóa
                var userToDelete = _context.NguoiDung.Find(id);

                if (userToDelete == null)
                {
                    TempData["Error"] = "Không tìm thấy nhân viên này!";
                    return RedirectToAction("Index");
                }

                // === [QUAN TRỌNG] LOGIC BẢO VỆ TÀI KHOẢN ADMIN ===

                // Lấy ID của người đang đăng nhập hiện tại
                string currentLoggedInUser = HttpContext.Session.GetString("MaND");

                // Check 1: Không được xóa chính mình
                if (userToDelete.MaND == currentLoggedInUser)
                {
                    TempData["Error"] = "NGUY HIỂM: Bạn không thể tự xóa tài khoản của chính mình!";
                    return RedirectToAction("Index");
                }

                // Check 2: Không được xóa tài khoản Admin gốc (Super Admin)
                if (userToDelete.TenDangNhap.ToLower() == "admin")
                {
                    TempData["Error"] = "BẢO MẬT: Không thể xóa tài khoản Quản trị gốc (Root)!";
                    return RedirectToAction("Index");
                }

                // =================================================

                // 2. Thực hiện xóa
                _context.NguoiDung.Remove(userToDelete);

                // 3. Ghi lịch sử (Ai xóa ai)
                _context.LichSuHeThong.Add(new LichSuHeThong
                {
                    NguoiThucHien = HttpContext.Session.GetString("HoTen"),
                    HanhDong = $"Đã xóa vĩnh viễn nhân viên: {userToDelete.TenDangNhap}",
                    ThoiGian = DateTime.Now
                });

                _context.SaveChanges();
                TempData["Message"] = "Đã xóa nhân viên thành công!";
            }
            catch (Exception ex)
            {
                // Lỗi này xảy ra khi nhân viên đó đã có dữ liệu (đã từng bán hàng, nhập kho...)
                // SQL Server sẽ chặn không cho xóa để bảo vệ dữ liệu -> Bắt lỗi này để báo người dùng.
                TempData["Error"] = "Không thể xóa nhân viên này vì họ đã có lịch sử giao dịch! Hãy chọn 'Sửa' và tắt trạng thái hoạt động thay vì xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}