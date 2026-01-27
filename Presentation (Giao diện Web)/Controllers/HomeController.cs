using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTOs.Input;
using Microsoft.AspNetCore.Http;
using Data.Context;
using Core.Entities;
using System.Net;       // Thư viện gửi mail
using System.Net.Mail;  // Thư viện gửi mail

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
            // Nếu đã đăng nhập thì chuyển hướng luôn
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
            var user = _authService.Login(request);

            if (user != null)
            {
                // Lưu thông tin vào Session
                HttpContext.Session.SetString("MaND", user.MaND);
                HttpContext.Session.SetString("HoTen", user.HoTen ?? "User");
                HttpContext.Session.SetString("VaiTro", user.VaiTro);

                // Ghi nhật ký đăng nhập
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

            // Đăng nhập thất bại -> Gửi lỗi về View để kích hoạt hiệu ứng Rung
            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View(request);
        }

        // ============================================================
        // 3. XỬ LÝ QUÊN MẬT KHẨU (GỬI VỀ EMAIL THẬT)
        // ============================================================
        [HttpPost]
        public IActionResult ForgotPassword(string username, string email)
        {
            try
            {
                // Bước 1: Tìm nhân viên có đúng Tên đăng nhập VÀ Email này
                var user = _context.NguoiDung.FirstOrDefault(u => u.TenDangNhap == username && u.Email == email);

                if (user == null)
                {
                    TempData["Error"] = "Thông tin không khớp! Vui lòng kiểm tra lại Tên tài khoản và Email.";
                    return RedirectToAction("Index");
                }

                // Bước 2: Chặn Admin reset kiểu này (Bảo mật)
                if (user.VaiTro == "Admin")
                {
                    TempData["Error"] = "Tài khoản Admin không được phép khôi phục qua Email!";
                    return RedirectToAction("Index");
                }

                // Bước 3: Tạo mật khẩu ngẫu nhiên (6 ký tự viết hoa)
                string newPassword = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

                // Bước 4: Lưu mật khẩu mới vào Database
                // (Lưu ý: Nếu hệ thống có mã hóa MD5 thì nhớ mã hóa newPassword trước khi gán)
                user.MatKhau = newPassword;
                _context.Update(user);
                _context.SaveChanges();

                // Bước 5: Gửi Email thật
                bool sendResult = SendEmailToUser(user.Email, user.TenDangNhap, newPassword);

                if (sendResult)
                {
                    TempData["Message"] = $"Thành công! Mật khẩu mới đã được gửi tới: {HideEmail(email)}";
                }
                else
                {
                    // Trường hợp gửi mail thất bại (do mạng hoặc sai pass ứng dụng)
                    // Ta tạm thời hiện mật khẩu ra để test (Khi chạy thật thì xóa dòng này đi)
                    TempData["Error"] = $"Gửi mail thất bại. Mật khẩu tạm thời: {newPassword}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ============================================================
        // HÀM GỬI EMAIL (QUAN TRỌNG NHẤT)
        // ============================================================
        private bool SendEmailToUser(string toEmail, string username, string newPass)
        {
            try
            {
                // --- CẤU HÌNH GMAIL CỦA BẠN (SỬA Ở ĐÂY) ---
                // 1. Nhập địa chỉ Gmail dùng để gửi
                string fromEmail = "email_cua_ban@gmail.com";

                // 2. Nhập Mật khẩu ứng dụng (KHÔNG PHẢI MẬT KHẨU ĐĂNG NHẬP GMAIL)
                // Cách lấy: Vào Google Account -> Bảo mật -> Xác minh 2 bước -> Mật khẩu ứng dụng
                string emailPassword = "mat_khau_ung_dung_16_ky_tu";
                // ------------------------------------------------------------

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, "Hệ Thống Kho (No-Reply)"); // Tên người gửi hiển thị
                mail.To.Add(toEmail); // Gửi đến email thật của nhân viên
                mail.Subject = "[Smart Warehouse] Cấp lại mật khẩu mới";
                mail.IsBodyHtml = true; // Cho phép gửi nội dung HTML đẹp

                // Nội dung email
                mail.Body = $@"
                    <div style='font-family:Arial, sans-serif; padding:20px; border:1px solid #ddd; border-radius:10px;'>
                        <h2 style='color:#0d6efd;'>Yêu cầu khôi phục mật khẩu</h2>
                        <p>Xin chào <b>{username}</b>,</p>
                        <p>Hệ thống đã nhận được yêu cầu cấp lại mật khẩu của bạn.</p>
                        <p>Mật khẩu đăng nhập mới của bạn là: <b style='font-size:18px; color:red;'>{newPass}</b></p>
                        <hr>
                        <p style='color:#777; font-size:12px;'>Vui lòng đăng nhập và đổi lại mật khẩu ngay để bảo mật tài khoản.</p>
                    </div>";

                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.Port = 587; // Port chuẩn của Gmail
                smtp.Credentials = new NetworkCredential(fromEmail, emailPassword);
                smtp.EnableSsl = true; // Bắt buộc bật SSL

                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi vào Console để debug nếu cần
                Console.WriteLine("Gửi mail lỗi: " + ex.Message);
                return false;
            }
        }

        // Hàm ẩn bớt email (vd: ng***@gmail.com)
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
        public IActionResult EmployeeManager() => View();      // Dành cho Owner
        public IActionResult SalesDashboard() => View();       // Dành cho Sale
        public IActionResult WarehouseDashboard() => View();   // Dành cho Kho
        public IActionResult Dashboard() => View();            // Trang mặc định
    }
}