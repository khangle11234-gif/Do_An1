using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTOs.Input;
using Business.DTOs.Output; // [MỚI] Thêm dòng này để dùng ViewModel
using Microsoft.AspNetCore.Http;
using Data.Context;
using Core.Entities;
using System.Net;        // Thư viện gửi mail
using System.Net.Mail;   // Thư viện gửi mail

namespace Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly QuanLyKhoContext _context;

        public HomeController(IAuthService authService, QuanLyKhoContext context)
        {
            _authService = authService;
            _context = context;
        }

        // ============================================================
        // 1. TRANG ĐĂNG NHẬP (GET)
        // ============================================================
        [HttpGet]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("MaND") != null)
            {
                return RedirectBasedOnRole(HttpContext.Session.GetString("VaiTro"));
            }
            return View();
        }

        // ============================================================
        // 2. XỬ LÝ ĐĂNG NHẬP (POST)
        // ============================================================
        [HttpPost]
        public IActionResult Index(LoginRequest request)
        {
            var user = _context.NguoiDung.FirstOrDefault(u => u.TenDangNhap == request.TenDangNhap);

            if (user == null)
            {
                ViewBag.Error = "Tài khoản không tồn tại!";
                return View(request);
            }

            if (user.MatKhau != request.MatKhau)
            {
                ViewBag.Error = "Mật khẩu không chính xác!";
                return View(request);
            }

            if (user.TrangThai != true)
            {
                ViewBag.Error = "Tài khoản của bạn đang bị TẠM DỪNG hoạt động. Vui lòng liên hệ Admin.";
                return View(request);
            }

            // Đăng nhập thành công
            HttpContext.Session.SetString("MaND", user.MaND);
            HttpContext.Session.SetString("HoTen", user.HoTen ?? "User");
            HttpContext.Session.SetString("VaiTro", user.VaiTro);

            try
            {
                _context.LichSuHeThong.Add(new LichSuHeThong
                {
                    NguoiThucHien = user.HoTen + " (" + request.TenDangNhap + ")",
                    HanhDong = "Đăng nhập vào hệ thống",
                    ThoiGian = DateTime.Now
                });
                _context.SaveChanges();
            }
            catch { }

            return RedirectBasedOnRole(user.VaiTro);
        }

        // ============================================================
        // 3. XỬ LÝ QUÊN MẬT KHẨU
        // ============================================================
        [HttpPost]
        public IActionResult ForgotPassword(string username, string email)
        {
            try
            {
                var user = _context.NguoiDung.FirstOrDefault(u => u.TenDangNhap == username && u.Email == email);

                if (user == null)
                {
                    TempData["Error"] = "Thông tin không khớp! Email này không thuộc về tài khoản trên.";
                    return RedirectToAction("Index");
                }

                if (user.VaiTro == "Admin")
                {
                    TempData["Error"] = "Tài khoản Admin không được phép khôi phục qua Email!";
                    return RedirectToAction("Index");
                }

                string newPassword = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

                user.MatKhau = newPassword;
                _context.Update(user);
                _context.SaveChanges();

                bool sendResult = SendEmailToUser(email, user.TenDangNhap, newPassword);

                if (sendResult)
                {
                    TempData["Message"] = $"Thành công! Mật khẩu mới đã được gửi tới: {HideEmail(email)}";
                }
                else
                {
                    TempData["Error"] = "Gửi mail thất bại. Vui lòng thử lại sau.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ============================================================
        // HÀM GỬI EMAIL
        // ============================================================
        private bool SendEmailToUser(string toEmail, string username, string newPass)
        {
            try
            {
                string fromEmail = "khangle11234@gmail.com";
                string emailPassword = "wnhn epsn fnfa bsdg";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, "Hệ Thống Kho (Smart Warehouse)");
                mail.To.Add(toEmail);
                mail.Subject = "[Smart Warehouse] Cấp lại mật khẩu mới";
                mail.IsBodyHtml = true;

                mail.Body = $@"
                    <div style='font-family:Arial, sans-serif; padding:20px; border:1px solid #ddd; border-radius:10px; background-color:#f9f9f9;'>
                        <h2 style='color:#0d6efd;'>Yêu cầu khôi phục mật khẩu</h2>
                        <p>Xin chào <b>{username}</b>,</p>
                        <p>Hệ thống nhận được yêu cầu cấp lại mật khẩu cho tài khoản liên kết với email này.</p>
                        <div style='background: #fff; padding: 15px; border-left: 4px solid #0d6efd; margin: 15px 0;'>
                            Mật khẩu đăng nhập mới của bạn là: <b style='font-size:20px; color:#d9534f; letter-spacing: 2px;'>{newPass}</b>
                        </div>
                        <p style='color:#555;'>Vui lòng đăng nhập và đổi lại mật khẩu ngay để bảo mật tài khoản.</p>
                        <hr>
                        <small style='color:#999;'>Đây là email tự động, vui lòng không trả lời.</small>
                    </div>";

                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential(fromEmail, emailPassword);
                smtp.EnableSsl = true;

                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gửi mail lỗi: " + ex.Message);
                return false;
            }
        }

        private string HideEmail(string email)
        {
            try
            {
                var parts = email.Split('@');
                if (parts[0].Length > 3)
                    return parts[0].Substring(0, 3) + "***@" + parts[1];
                return email;
            }
            catch { return email; }
        }

        private IActionResult RedirectBasedOnRole(string role)
        {
            if (role == "Admin") return RedirectToAction("Index", "Admin");
            if (role == "Owner") return RedirectToAction("EmployeeManager");
            if (role == "Sale") return RedirectToAction("SalesDashboard");
            if (role == "Kho") return RedirectToAction("WarehouseDashboard");
            return RedirectToAction("Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // ============================================================
        // [CẬP NHẬT] TRANG QUẢN LÝ NHÂN VIÊN (BOSS)
        // ============================================================
        // Trong HomeController.cs
        public IActionResult EmployeeManager()
        {
            // Kiểm tra quyền
            var role = HttpContext.Session.GetString("VaiTro");
            if (role != "Owner" && role != "Admin") return RedirectToAction("Index");

            // Lấy dữ liệu
            var listUser = _context.NguoiDung
                                   .Where(u => u.VaiTro != "Admin") // Boss không thấy Admin
                                   .OrderByDescending(x => x.MaND)
                                   .ToList();

            ViewBag.DanhSachQuyen = _context.NhomQuyen.ToList();

            var model = new AdminDashboardViewModel { DanhSachNhanVien = listUser };
            return View(model);
        }

        public IActionResult SalesDashboard() => View();
        public IActionResult WarehouseDashboard() => View();
        public IActionResult Dashboard() => View();
    }
}