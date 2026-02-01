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

        // ============================================================
        // HELPER CHECK QUYỀN
        // ============================================================
        private bool IsAdmin() => HttpContext.Session.GetString("VaiTro") == "Admin";
        private bool IsOwner() => HttpContext.Session.GetString("VaiTro") == "Owner";

        // ============================================================
        // 1. TRANG CHỦ ADMIN (DASHBOARD)
        // ============================================================
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var info = _context.ThongTinCongTy.FirstOrDefault();
            var users = _context.NguoiDung.OrderBy(u => u.MaND).ToList();

            // [QUAN TRỌNG] Lấy 50 dòng lịch sử mới nhất để hiển thị và đếm thông báo
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

            // [GHI NHẬT KÝ] Để hiện thông báo đỏ
            GhiNhatKy("Cập nhật thông tin doanh nghiệp");

            _context.SaveChanges();
            TempData["Message"] = "Đã lưu cấu hình doanh nghiệp!";
            return RedirectToAction("Index");
        }

        // ============================================================
        // 3. XỬ LÝ LƯU USER (THÊM MỚI HOẶC SỬA)
        // ============================================================
        [HttpPost]
        public IActionResult SaveUser(UserCreateViewModel model)
        {
            // Cho phép cả Admin và Owner đều được dùng hàm này
            if (!IsAdmin() && !IsOwner()) return RedirectToAction("Index", "Home");

            try
            {
                // === LOGIC 1: THÊM MỚI (Nếu MaND rỗng hoặc null) ===
                if (string.IsNullOrEmpty(model.MaND))
                {
                    // Kiểm tra trùng tên đăng nhập
                    if (_context.NguoiDung.Any(u => u.TenDangNhap == model.TenDangNhap))
                    {
                        TempData["Error"] = "Tên đăng nhập đã tồn tại!";
                        return RedirectBack();
                    }

                    // Kiểm tra trùng Email
                    if (_context.NguoiDung.Any(u => u.Email == model.Email))
                    {
                        TempData["Error"] = "Email này đã được sử dụng cho nhân viên khác!";
                        return RedirectBack();
                    }

                    var newUser = new NguoiDung
                    {
                        MaND = "NV-" + DateTime.Now.Ticks.ToString().Substring(12),
                        TenDangNhap = model.TenDangNhap,
                        MatKhau = model.MatKhau, // (Thực tế nên mã hóa)
                        HoTen = model.HoTen,
                        VaiTro = model.VaiTro,
                        TrangThai = model.HienThi,
                        Email = model.Email,
                        MaCN = "CN01",
                        NgayTao = DateTime.Now,
                        MaNhomQuyen = model.MaNhomQuyen
                    };

                    _context.NguoiDung.Add(newUser);

                    // [GHI NHẬT KÝ] Thông báo hành động thêm mới
                    GhiNhatKy($"Thêm nhân viên mới: {model.TenDangNhap} ({model.VaiTro})");

                    TempData["Message"] = "Thêm nhân viên mới thành công!";
                }
                // === LOGIC 2: CẬP NHẬT (Khi có MaND gửi lên) ===
                else
                {
                    var existingUser = _context.NguoiDung.FirstOrDefault(u => u.MaND == model.MaND);

                    if (existingUser == null)
                    {
                        TempData["Error"] = $"Lỗi: Không tìm thấy nhân viên {model.MaND}!";
                        return RedirectBack();
                    }

                    // Kiểm tra trùng Email khi cập nhật
                    if (existingUser.Email != model.Email && _context.NguoiDung.Any(u => u.Email == model.Email))
                    {
                        TempData["Error"] = "Email mới bị trùng với nhân viên khác!";
                        return RedirectBack();
                    }

                    // Cập nhật thông tin
                    existingUser.HoTen = model.HoTen;
                    existingUser.VaiTro = model.VaiTro;
                    existingUser.TrangThai = model.HienThi;
                    existingUser.Email = model.Email;

                    if (!string.IsNullOrEmpty(model.MaNhomQuyen))
                    {
                        existingUser.MaNhomQuyen = model.MaNhomQuyen;
                    }

                    if (!string.IsNullOrEmpty(model.MatKhau))
                    {
                        existingUser.MatKhau = model.MatKhau;
                    }

                    _context.Update(existingUser);

                    // [GHI NHẬT KÝ] Thông báo hành động sửa
                    GhiNhatKy($"Cập nhật thông tin nhân viên: {existingUser.TenDangNhap}");

                    TempData["Message"] = "Cập nhật thông tin thành công!";
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectBack();
        }

        // ============================================================
        // 4. SAO LƯU DỮ LIỆU
        // ============================================================
        public IActionResult BackupData()
        {
            // Code backup của bạn (giả lập)
            // ...

            // [GHI NHẬT KÝ]
            GhiNhatKy("Thực hiện sao lưu dữ liệu hệ thống");
            _context.SaveChanges();

            TempData["Message"] = "Đã tạo bản sao lưu thành công!";
            return RedirectToAction("Index");
        }

        // ============================================================
        // 5. XỬ LÝ XÓA NHÂN VIÊN
        // ============================================================
        [HttpPost]
        public IActionResult DeleteUser(string id)
        {
            if (!IsAdmin() && !IsOwner()) return RedirectToAction("Index", "Home");

            try
            {
                var userToDelete = _context.NguoiDung.Find(id);

                if (userToDelete == null)
                {
                    TempData["Error"] = "Không tìm thấy nhân viên này!";
                    return RedirectBack();
                }

                string currentLoggedInUser = HttpContext.Session.GetString("MaND");

                // Check an toàn
                if (userToDelete.MaND == currentLoggedInUser)
                {
                    TempData["Error"] = "Bạn không thể tự xóa tài khoản của chính mình!";
                    return RedirectBack();
                }
                if (userToDelete.TenDangNhap.ToLower() == "admin")
                {
                    TempData["Error"] = "Không thể xóa tài khoản Quản trị gốc (Root)!";
                    return RedirectBack();
                }

                string tenUserBiXoa = userToDelete.TenDangNhap; // Lưu tên lại để ghi log trước khi xóa
                _context.NguoiDung.Remove(userToDelete);

                // [GHI NHẬT KÝ] Thông báo hành động xóa
                GhiNhatKy($"Đã xóa vĩnh viễn nhân viên: {tenUserBiXoa}");

                _context.SaveChanges();
                TempData["Message"] = "Đã xóa nhân viên thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể xóa nhân viên này vì họ đã có dữ liệu liên quan (Bán hàng/Nhập kho).";
            }

            return RedirectBack();
        }

        // ============================================================
        // [QUAN TRỌNG] HÀM DÙNG CHUNG ĐỂ GHI NHẬT KÝ (LOGGING)
        // ============================================================
        private void GhiNhatKy(string hanhDong)
        {
            try
            {
                var log = new LichSuHeThong
                {
                    NguoiThucHien = HttpContext.Session.GetString("HoTen") ?? "Unknown",
                    HanhDong = hanhDong,
                    ThoiGian = DateTime.Now
                };

                // Thêm vào DbContext (Chưa SaveChanges ngay, để hàm gọi tự Save)
                _context.LichSuHeThong.Add(log);
            }
            catch
            {
                // Nếu lỗi ghi log thì bỏ qua, không làm crash app
            }
        }

        // Helper điều hướng
        private IActionResult RedirectBack()
        {
            if (IsOwner()) return RedirectToAction("EmployeeManager", "Home");
            return RedirectToAction("Index", "Admin");
        }
    }
}