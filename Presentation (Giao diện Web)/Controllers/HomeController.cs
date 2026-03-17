using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTOs.Input;
using Business.DTOs.Output;
using Microsoft.AspNetCore.Http;
using Data.Context;
using Core.Entities;
using System.Net;        // Thư viện gửi mail
using System.Net.Mail;   // Thư viện gửi mail
using System;
using System.Linq;

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

        // ============================================================
        // 4. XỬ LÝ ĐỔI MẬT KHẨU TỪ GIAO DIỆN MODAL (ĐÃ THÊM MỚI)
        // ============================================================
        [HttpPost]
        public IActionResult ChangePassword(string matKhauCu, string matKhauMoi)
        {
            try
            {
                // Lấy Mã Người Dùng đang đăng nhập từ Session (Lưu dưới dạng chuỗi)
                var maND = HttpContext.Session.GetString("MaND");

                if (string.IsNullOrEmpty(maND))
                {
                    return Json(new { success = false, msg = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!" });
                }

                // Tìm tài khoản trong CSDL
                var user = _context.NguoiDung.FirstOrDefault(u => u.MaND == maND);

                if (user == null)
                {
                    return Json(new { success = false, msg = "Không tìm thấy tài khoản trong hệ thống!" });
                }

                // Kiểm tra mật khẩu cũ
                if (user.MatKhau != matKhauCu)
                {
                    return Json(new { success = false, msg = "Mật khẩu hiện tại không chính xác!" });
                }

                // Cập nhật mật khẩu mới và lưu lại
                user.MatKhau = matKhauMoi;
                _context.NguoiDung.Update(user);
                _context.SaveChanges();

                return Json(new { success = true, msg = "Đổi mật khẩu thành công! Vui lòng ghi nhớ mật khẩu mới." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ============================================================
        // [ĐÃ SỬA CHUẨN] ĐIỀU HƯỚNG PHÂN QUYỀN ĐĂNG NHẬP (AUTO-ROUTING)
        // ============================================================
        private IActionResult RedirectBasedOnRole(string role)
        {
            if (role == "Admin") return RedirectToAction("Index", "Admin");
            if (role == "Owner") return RedirectToAction("Index", "Owner");

            // Tự động đá Sale và Kho về đúng Controller của nó
            if (role == "Sale") return RedirectToAction("Index", "Sale");
            if (role == "Kho") return RedirectToAction("Index", "Kho");

            return RedirectToAction("Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult Dashboard() => View();
    }
}